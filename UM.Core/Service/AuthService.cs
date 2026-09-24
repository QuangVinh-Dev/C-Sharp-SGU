using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UM.Core.DTOS.Request.Auth;
using UM.Core.DTOS.Response.Auth;
using UM.Core.Entities;
using UM.Core.Repository;
using UM.Core.Security;

namespace UM.Core.Service;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int OtpExpirationMinutes = 5;
    private const int OtpMaxAttempts = 5;
    private const int OtpLength = 6;

    public AuthService(
        IAuthRepository authRepository,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _authRepository = authRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
    }

    // ─── REGISTER ─────────────────────────────────────────────────

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, string? ipAddress)
    {
        // Check if email already exists
        if (await _authRepository.IsEmailTakenAsync(request.Email))
        {
            throw new InvalidOperationException("Email đã được sử dụng.");
        }

        var now = DateTime.UtcNow;
        var publicCode = GeneratePublicCode();

        var user = new User
        {
            PublicCode = publicCode,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = false,
            FailedLoginCount = 0,
            LockedUntil = null,
            IsSuspended = false,
            SuspendedUntil = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var profile = new UserProfile
        {
            FullName = request.FullName,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Generate OTP
        var otpPlaintext = GenerateOtp();
        var otpHash = HashOtp(otpPlaintext);

        var emailOtp = new EmailOtp
        {
            CodeHash = otpHash,
            Purpose = "REGISTER",
            ExpiresAt = now.AddMinutes(OtpExpirationMinutes),
            AttemptCount = 0,
            MaxAttempts = OtpMaxAttempts,
            RequestIp = ipAddress,
            CreatedAt = now
        };

        // Save in transaction (user → profile → otp)
        await _authRepository.RegisterUserTransactionAsync(user, profile, emailOtp);

        // Audit log
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = user.Id,
            Action = "REGISTER",
            EntityType = "User",
            EntityId = user.Id,
            IpAddress = ipAddress,
            Metadata = JsonSerializer.Serialize(new { user.Email }),
            CreatedAt = now
        });

        // Send OTP email (fire and forget, don't block registration)
        try
        {
            await _emailService.SendOtpEmailAsync(
                request.Email,
                otpPlaintext,
                "Xác thực tài khoản",
                OtpExpirationMinutes);
        }
        catch
        {
            // Log but don't fail registration if email sending fails
        }

        return new RegisterResponse
        {
            Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản."
        };
    }

    // ─── VERIFY OTP ───────────────────────────────────────────────

    public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress)
    {
        if (request.Purpose != "REGISTER")
        {
            throw new InvalidOperationException("Mục đích OTP không hợp lệ cho tác vụ này.");
        }

        var user = await _authRepository.GetUserByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
        {
            throw new UnauthorizedAccessException("Thông tin không hợp lệ.");
        }

        var otp = await _authRepository.GetLatestOtpAsync(
            request.Email.Trim().ToLowerInvariant(),
            request.Purpose);

        if (otp == null)
        {
            throw new InvalidOperationException("Không tìm thấy mã OTP.");
        }

        // Check if already used
        if (otp.UsedAt != null)
        {
            throw new InvalidOperationException("Mã OTP đã được sử dụng.");
        }

        // Check expiration
        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Mã OTP đã hết hạn.");
        }

        // Check max attempts
        if (otp.AttemptCount >= otp.MaxAttempts)
        {
            throw new InvalidOperationException("Mã OTP đã vượt quá số lần thử cho phép.");
        }

        // Hash and compare
        var inputHash = HashOtp(request.Otp);
        if (!inputHash.SequenceEqual(otp.CodeHash))
        {
            // Wrong OTP - increment attempt count
            otp.AttemptCount++;
            await _authRepository.UpdateOtpAsync(otp);

            throw new InvalidOperationException("Mã OTP không đúng.");
        }

        // OTP is correct
        var now = DateTime.UtcNow;
        var consumed = await _authRepository.TryConsumeEmailOtpAsync(otp.Id, now);
        
        if (!consumed)
        {
            throw new InvalidOperationException("Mã OTP đã được sử dụng hoặc vừa được xử lý.");
        }

        user.IsEmailVerified = true;
        user.UpdatedAt = now;
        await _authRepository.UpdateUserAsync(user);

        // Audit log
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = user.Id,
            Action = "VERIFY_EMAIL",
            EntityType = "User",
            EntityId = user.Id,
            IpAddress = ipAddress,
            CreatedAt = now
        });

        return new VerifyOtpResponse
        {
            Message = "Xác thực OTP thành công."
        };
    }

    // ─── LOGIN ────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email.Trim().ToLowerInvariant());
        var now = DateTime.UtcNow;

        if (user == null)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        }

        // Check IsActive
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        }

        // Check IsSuspended
        if (user.IsSuspended)
        {
            if (user.SuspendedUntil == null || user.SuspendedUntil > now)
            {
                throw new UnauthorizedAccessException("Tài khoản đang bị tạm khóa.");
            }
            // Suspension expired, clear it
            user.IsSuspended = false;
            user.SuspendedUntil = null;
        }

        // Check LockedUntil
        if (user.LockedUntil != null && user.LockedUntil > now)
        {
            await _authRepository.AddAuditLogAsync(new AuditLog
            {
                ActorUserId = user.Id,
                Action = "LOGIN_FAILED",
                EntityType = "User",
                EntityId = user.Id,
                IpAddress = ipAddress,
                Metadata = JsonSerializer.Serialize(new { Reason = "Account locked" }),
                CreatedAt = now
            });

            throw new UnauthorizedAccessException("Tài khoản đang bị khóa. Vui lòng thử lại sau.");
        }

        // Check IsEmailVerified
        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Email chưa được xác thực. Vui lòng xác thực email trước khi đăng nhập.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            var lockedUntil = now.AddMinutes(LockoutMinutes);
            await _authRepository.IncrementFailedLoginCountAsync(user.Id, MaxFailedLoginAttempts, lockedUntil, now);

            // Fetch the updated user state to log accurately
            var updatedUser = await _authRepository.GetUserByIdAsync(user.Id);

            if (updatedUser != null && updatedUser.FailedLoginCount == MaxFailedLoginAttempts)
            {
                // Audit: account locked
                await _authRepository.AddAuditLogAsync(new AuditLog
                {
                    ActorUserId = user.Id,
                    Action = "ACCOUNT_LOCKED",
                    EntityType = "User",
                    EntityId = user.Id,
                    IpAddress = ipAddress,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        FailedAttempts = updatedUser.FailedLoginCount,
                        LockedUntil = updatedUser.LockedUntil
                    }),
                    CreatedAt = now
                });
            }

            // Audit: login failed
            await _authRepository.AddAuditLogAsync(new AuditLog
            {
                ActorUserId = user.Id,
                Action = "LOGIN_FAILED",
                EntityType = "User",
                EntityId = user.Id,
                IpAddress = ipAddress,
                Metadata = JsonSerializer.Serialize(new { FailedCount = updatedUser?.FailedLoginCount ?? user.FailedLoginCount + 1 }),
                CreatedAt = now
            });

            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        }

        // Login success — reset counters
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        await _authRepository.UpdateUserAsync(user);

        // Get system roles and permissions for JWT claims
        var roles = await _authRepository.GetUserSystemRolesAsync(user.Id);
        var permissions = await _authRepository.GetUserSystemPermissionsAsync(user.Id);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshTokenPlaintext = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshTokenPlaintext);

        var refreshTokenExpirationDays = int.Parse(
            _configuration.GetSection("JwtSettings")["RefreshTokenExpirationDays"]!);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = now.AddDays(refreshTokenExpirationDays),
            CreatedAt = now,
            CreatedByIp = ipAddress
        };

        await _authRepository.AddRefreshTokenAsync(refreshToken);

        // Audit: login success
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = user.Id,
            Action = "LOGIN_SUCCESS",
            EntityType = "User",
            EntityId = user.Id,
            IpAddress = ipAddress,
            CreatedAt = now
        });

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenPlaintext
        };
    }

    // ─── REFRESH TOKEN ────────────────────────────────────────────

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress)
    {
        var tokenHash = _jwtService.HashRefreshToken(request.RefreshToken);
        var existingToken = await _authRepository.GetRefreshTokenAsync(tokenHash);

        if (existingToken == null)
        {
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
        }

        if (existingToken.RevokedAt != null)
        {
            // Replay detected! Revoke all tokens for this user.
            await _authRepository.RevokeAllUserRefreshTokensAsync(existingToken.UserId, ipAddress);
            throw new UnauthorizedAccessException("Refresh token đã bị thu hồi. Toàn bộ phiên đăng nhập đã bị đăng xuất.");
        }

        if (existingToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");
        }

        var user = await _authRepository.GetUserByIdAsync(existingToken.UserId);

if (user == null || !user.IsActive)
{
    throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");
}

if (user.IsSuspended &&
    (user.SuspendedUntil == null || user.SuspendedUntil > DateTime.UtcNow))
{
    throw new UnauthorizedAccessException("Tài khoản đang bị tạm khóa.");
}

        var now = DateTime.UtcNow;

        // Generate new tokens
        var roles = await _authRepository.GetUserSystemRolesAsync(user.Id);
        var permissions = await _authRepository.GetUserSystemPermissionsAsync(user.Id);

        var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var newRefreshTokenPlaintext = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashRefreshToken(newRefreshTokenPlaintext);

        var refreshTokenExpirationDays = int.Parse(
            _configuration.GetSection("JwtSettings")["RefreshTokenExpirationDays"]!);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = now.AddDays(refreshTokenExpirationDays),
            CreatedAt = now,
            CreatedByIp = ipAddress
        };

        // Revoke old token with rotation link atomically
        var rotated = await _authRepository.TryConsumeRefreshTokenAsync(existingToken.Id, now, ipAddress, newRefreshToken.Id);
        
        if (!rotated)
        {
            // Another request already rotated it (race condition prevented)
            throw new UnauthorizedAccessException("Refresh token đã được xử lý bởi yêu cầu khác.");
        }

        await _authRepository.AddRefreshTokenAsync(newRefreshToken);

        // Audit
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = user.Id,
            Action = "REFRESH_TOKEN",
            EntityType = "RefreshToken",
            EntityId = user.Id,
            IpAddress = ipAddress,
            CreatedAt = now
        });

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenPlaintext
        };
    }

    // ─── LOGOUT ───────────────────────────────────────────────────

    public async Task LogoutAsync(string refreshToken, string? ipAddress, long userId)
    {
        var tokenHash = _jwtService.HashRefreshToken(refreshToken);
        var existingToken = await _authRepository.GetRefreshTokenAsync(tokenHash);

        if (existingToken != null &&
    existingToken.RevokedAt == null &&
    existingToken.UserId == userId)
{
    existingToken.RevokedAt = DateTime.UtcNow;
    existingToken.RevokedByIp = ipAddress;

    await _authRepository.UpdateRefreshTokenAsync(existingToken);
}

        // Audit
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = userId,
            Action = "LOGOUT",
            EntityType = "User",
            EntityId = userId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ─── FORGOT PASSWORD ──────────────────────────────────────────

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress)
    {
        // Generic message — don't reveal if email exists
        var genericMessage = "Nếu email tồn tại, mã OTP đã được gửi.";

        var user = await _authRepository.GetUserByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user != null)
        {
            var now = DateTime.UtcNow;
            var otpPlaintext = GenerateOtp();
            var otpHash = HashOtp(otpPlaintext);

            var emailOtp = new EmailOtp
            {
                UserId = user.Id,
                CodeHash = otpHash,
                Purpose = "RESET_PASSWORD",
                ExpiresAt = now.AddMinutes(OtpExpirationMinutes),
                AttemptCount = 0,
                MaxAttempts = OtpMaxAttempts,
                RequestIp = ipAddress,
                CreatedAt = now
            };

            await _authRepository.SaveEmailOtpAsync(emailOtp);

            // Audit
            await _authRepository.AddAuditLogAsync(new AuditLog
            {
                ActorUserId = user.Id,
                Action = "FORGOT_PASSWORD",
                EntityType = "User",
                EntityId = user.Id,
                IpAddress = ipAddress,
                CreatedAt = now
            });

            try
            {
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    otpPlaintext,
                    "Đặt lại mật khẩu",
                    OtpExpirationMinutes);
            }
            catch
            {
                // Don't fail even if email sending fails
            }
        }

        return new ForgotPasswordResponse
        {
            Message = genericMessage
        };
    }

    // ─── RESET PASSWORD ───────────────────────────────────────────

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _authRepository.GetUserByEmailAsync(email);

        if (user == null)
        {
            throw new InvalidOperationException("Thông tin không hợp lệ.");
        }
        if (!user.IsActive)
{
    throw new InvalidOperationException("Tài khoản đã bị vô hiệu hóa.");
}

if (user.IsSuspended &&
    (user.SuspendedUntil == null || user.SuspendedUntil > DateTime.UtcNow))
{
    throw new InvalidOperationException("Tài khoản đang bị tạm khóa.");
}

        // Verify OTP for RESET_PASSWORD purpose
        var otp = await _authRepository.GetLatestOtpAsync(email, "RESET_PASSWORD");

        if (otp == null)
        {
            throw new InvalidOperationException("Không tìm thấy mã OTP.");
        }

        if (otp.UsedAt != null)
        {
            throw new InvalidOperationException("Mã OTP đã được sử dụng.");
        }

        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Mã OTP đã hết hạn.");
        }

        if (otp.AttemptCount >= otp.MaxAttempts)
        {
            throw new InvalidOperationException("Mã OTP đã vượt quá số lần thử cho phép.");
        }

        var inputHash = HashOtp(request.Otp);
        if (!inputHash.SequenceEqual(otp.CodeHash))
        {
            otp.AttemptCount++;
            await _authRepository.UpdateOtpAsync(otp);
            throw new InvalidOperationException("Mã OTP không đúng.");
        }

        var now = DateTime.UtcNow;

        // Perform atomic reset in transaction
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var success = await _authRepository.ResetPasswordTransactionAsync(user.Id, newPasswordHash, otp.Id, now, now);

        if (!success)
        {
            throw new InvalidOperationException("Mã OTP đã được sử dụng hoặc vừa được xử lý.");
        }

        // Revoke all refresh tokens for this user
        await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id, ipAddress);

        // Audit
        await _authRepository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = user.Id,
            Action = "RESET_PASSWORD",
            EntityType = "User",
            EntityId = user.Id,
            IpAddress = ipAddress,
            CreatedAt = now
        });

        return new ResetPasswordResponse
        {
            Message = "Đặt lại mật khẩu thành công. Tất cả phiên đăng nhập đã bị đăng xuất."
        };
    }

    // ─── HELPERS ──────────────────────────────────────────────────

    private static string GeneratePublicCode()
    {
        // 16-char alphanumeric code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return new string(random.Select(b => chars[b % chars.Length]).ToArray());
    }

    private static string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return number.ToString($"D{OtpLength}");
    }

    private static byte[] HashOtp(string otp)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(otp));
    }
}
