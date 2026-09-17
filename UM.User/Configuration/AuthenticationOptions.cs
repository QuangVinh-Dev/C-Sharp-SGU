namespace BackendApi.Configuration;

public class AuthenticationOptions
{
    public int AccessTokenMinutes { get; set; }
    public int RefreshTokenDays { get; set; }
    public int MaxFailedLoginAttempts { get; set; }
    public int LockoutMinutes { get; set; }
    public int OtpExpirationMinutes { get; set; }
    public int OtpMaxAttempts { get; set; }
}
