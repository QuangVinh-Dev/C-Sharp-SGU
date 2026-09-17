using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using BackendApi.Configuration;
using BackendApi.DTOs.Auth;
using BackendApi.Infrastructure.Database;
using BackendApi.Infrastructure.Email;
using BackendApi.Models;
using BackendApi.Repositories;
using BackendApi.Security;
using Microsoft.Data.SqlClient;

namespace BackendApi.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task VerifyEmailAsync(VerifyEmailRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    Task LogoutAsync(LogoutRequest request, string ipAddress);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly AuthenticationOptions _authOptions;
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuthService(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender,
        IOptions<AuthenticationOptions> authOptions,
        ISqlConnectionFactory connectionFactory)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
        _authOptions = authOptions.Value;
        _connectionFactory = connectionFactory;
    }

    // ... Register and VerifyEmail ...
    public async Task RegisterAsync(RegisterRequest request)
    {
        if (await _authRepository.IsEmailTakenAsync(request.Email))
            throw new Exception("Email is already registered.");

        var user = new User { Email = request.Email, PasswordHash = _passwordService.HashPassword(request.Password) };
        var profile = new UserProfile { FullName = request.FullName, DateOfBirth = request.DateOfBirth, PhoneNumber = request.PhoneNumber };
        
        var otpCode = _otpService.GenerateOtp();
        var otp = new EmailOtp { CodeHash = _otpService.HashOtp(otpCode), Purpose = "EmailVerification", ExpiresAt = DateTime.UtcNow.AddMinutes(_authOptions.OtpExpirationMinutes) };

        await _authRepository.RegisterUserTransactionAsync(user, profile, otp);

        var emailBody = $"<h1>Email Verification</h1><p>Your OTP is: <strong>{otpCode}</strong></p>";
        await _emailSender.SendEmailAsync(user.Email, "Verify your email", emailBody);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("Invalid request.");

        var otp = await _authRepository.GetLatestOtpAsync(request.Email, "EmailVerification");
        if (otp == null || otp.UsedAt != null || otp.ExpiresAt < DateTime.UtcNow) throw new Exception("Invalid or expired OTP.");
        if (otp.AttemptCount >= _authOptions.OtpMaxAttempts) throw new Exception("Too many failed attempts.");
        if (!_otpService.VerifyOtp(request.Otp, otp.CodeHash))
        {
            otp.AttemptCount++;
            await _authRepository.UpdateOtpAsync(otp);
            throw new Exception("Invalid OTP.");
        }

        otp.UsedAt = DateTime.UtcNow;
        user.IsEmailVerified = true;

        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            await _authRepository.UpdateOtpAsync(otp, transaction);
            await _userRepository.UpdateAsync(user, transaction);
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive) throw new Exception("Invalid credentials.");
        if (user.IsSuspended) throw new Exception("Account is suspended.");
        if (user.LockedUntil > DateTime.UtcNow) throw new Exception("Account is temporarily locked.");
        if (!user.IsEmailVerified) throw new Exception("Email not verified.");

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= _authOptions.MaxFailedLoginAttempts)
                user.LockedUntil = DateTime.UtcNow.AddMinutes(_authOptions.LockoutMinutes);
            await _userRepository.UpdateAsync(user);
            throw new Exception("Invalid credentials.");
        }

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return await GenerateAuthResponseAsync(user, null);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string? ipAddress)
    {
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id);
        var roles = await _userRepository.GetUserRolesAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissions, roles);
        
        var randomString = GenerateRandomString();
        var tokenId = Guid.NewGuid();
        
        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            UserId = user.Id,
            TokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(randomString)),
            ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddRefreshTokenAsync(refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = $"{tokenId}:{randomString}",
            ExpiresIn = _authOptions.AccessTokenMinutes * 60
        };
    }

    private static string GenerateRandomString()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private (RefreshToken?, string) ParseAndGetTokenAsync(string tokenString)
    {
        var parts = tokenString.Split(':');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId)) return (null, string.Empty);
        
        // We need a method to get by ID! Let's modify AuthRepository to GetRefreshTokenByIdAsync.
        // Wait, I can implement it locally for now using a direct query, but better to put it in Repository.
        return (null, parts[1]); // Will fix
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        var parts = request.RefreshToken.Split(':');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId)) throw new Exception("Invalid token format.");
        
        var token = await GetRefreshTokenByIdAsync(tokenId);
        var inputHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(parts[1]));
        if (token == null || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(inputHash, token.TokenHash))
            throw new Exception("Invalid refresh token.");

        if (token.RevokedAt != null || token.ExpiresAt < DateTime.UtcNow)
        {
            if (token.RevokedAt != null)
            {
                // Token reuse detected, revoke all tokens for this user
                await _authRepository.RevokeAllUserRefreshTokensAsync(token.UserId);
            }
            throw new Exception("Invalid or expired token.");
        }

        var user = await _userRepository.GetByIdAsync(token.UserId);
        if (user == null) throw new Exception("User not found.");

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        
        var newAuthResponse = await GenerateAuthResponseAsync(user, ipAddress);
        var newParts = newAuthResponse.RefreshToken.Split(':');
        token.ReplacedByTokenId = Guid.Parse(newParts[0]);

        await _authRepository.UpdateRefreshTokenAsync(token);
        return newAuthResponse;
    }

    private async Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = "SELECT Id, UserId, TokenHash, ExpiresAt, RevokedAt, RevokedByIp, ReplacedByTokenId, CreatedAt FROM RefreshTokens WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new RefreshToken
            {
                Id = reader.GetGuid(0), UserId = reader.GetInt64(1), TokenHash = (byte[])reader[2],
                ExpiresAt = reader.GetDateTime(3), RevokedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                RevokedByIp = reader.IsDBNull(5) ? null : reader.GetString(5),
                ReplacedByTokenId = reader.IsDBNull(6) ? null : reader.GetGuid(6), CreatedAt = reader.GetDateTime(7)
            };
        }
        return null;
    }

    public async Task LogoutAsync(LogoutRequest request, string ipAddress)
    {
        var parts = request.RefreshToken.Split(':');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId)) throw new Exception("Invalid token.");
        
        var token = await GetRefreshTokenByIdAsync(tokenId);
        var inputHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(parts[1]));
        if (token == null || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(inputHash, token.TokenHash))
            throw new Exception("Invalid token.");

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _authRepository.UpdateRefreshTokenAsync(token);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user != null)
        {
            var otpCode = _otpService.GenerateOtp();
            var otp = new EmailOtp
            {
                UserId = user.Id,
                CodeHash = _otpService.HashOtp(otpCode),
                Purpose = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_authOptions.OtpExpirationMinutes)
            };
            
            await _authRepository.SaveEmailOtpAsync(otp);
            
            var emailBody = $"<h1>Password Reset</h1><p>Your OTP is: <strong>{otpCode}</strong></p>";
            await _emailSender.SendEmailAsync(user.Email, "Reset your password", emailBody);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("Invalid request.");

        var otp = await _authRepository.GetLatestOtpAsync(request.Email, "PasswordReset");
        if (otp == null || otp.UsedAt != null || otp.ExpiresAt < DateTime.UtcNow) throw new Exception("Invalid or expired OTP.");
        if (otp.AttemptCount >= _authOptions.OtpMaxAttempts) throw new Exception("Too many failed attempts.");
        
        if (!_otpService.VerifyOtp(request.Otp, otp.CodeHash))
        {
            otp.AttemptCount++;
            await _authRepository.UpdateOtpAsync(otp);
            throw new Exception("Invalid OTP.");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        otp.UsedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            await _authRepository.UpdateOtpAsync(otp, transaction);
            await _userRepository.UpdateAsync(user, transaction);
            await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id, transaction);
            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
    }
}
