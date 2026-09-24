using UM.Core.DTOS.Request.Auth;
using UM.Core.DTOS.Response.Auth;

namespace UM.Core.Service;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, string? ipAddress);

    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress);

    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);

    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress);

    Task LogoutAsync(string refreshToken, string? ipAddress, long userId);

    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress);

    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress);
}
