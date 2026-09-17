using System.Data;
using Microsoft.Data.SqlClient;
using BackendApi.Infrastructure.Database;
using BackendApi.Models;

namespace BackendApi.Repositories;

public interface IAuthRepository
{
    Task<bool> IsEmailTakenAsync(string email);
    Task RegisterUserTransactionAsync(User user, UserProfile profile, EmailOtp otp);
    Task<EmailOtp?> GetLatestOtpAsync(string email, string purpose);
    Task UpdateOtpAsync(EmailOtp otp, SqlTransaction? transaction = null);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task AddRefreshTokenAsync(RefreshToken token, SqlTransaction? transaction = null);
    Task UpdateRefreshTokenAsync(RefreshToken token, SqlTransaction? transaction = null);
    Task RevokeAllUserRefreshTokensAsync(long userId, SqlTransaction? transaction = null);
    Task SaveEmailOtpAsync(EmailOtp otp);
}

public class AuthRepository : IAuthRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuthRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);
        var result = (int)(await command.ExecuteScalarAsync() ?? 0);
        return result > 0;
    }

    public async Task RegisterUserTransactionAsync(User user, UserProfile profile, EmailOtp otp)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userQuery = @"INSERT INTO Users (PublicCode, Email, PasswordHash, IsEmailVerified, IsActive, IsSuspended, FailedLoginCount, CreatedAt, UpdatedAt) 
                              OUTPUT INSERTED.Id
                              VALUES (@PublicCode, @Email, @PasswordHash, 0, 1, 0, 0, GETUTCDATE(), GETUTCDATE())";
            using var userCommand = new SqlCommand(userQuery, connection, transaction);
            userCommand.Parameters.AddWithValue("@PublicCode", Guid.NewGuid().ToString("N").Substring(0, 8));
            userCommand.Parameters.AddWithValue("@Email", user.Email);
            userCommand.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            var scalarResult = await userCommand.ExecuteScalarAsync();
            var userId = scalarResult != null ? (long)scalarResult : 0;

            var profileQuery = @"INSERT INTO UserProfiles (UserId, FullName, DateOfBirth, PhoneNumber, CreatedAt, UpdatedAt) 
                                 VALUES (@UserId, @FullName, @DateOfBirth, @PhoneNumber, GETUTCDATE(), GETUTCDATE())";
            using var profileCommand = new SqlCommand(profileQuery, connection, transaction);
            profileCommand.Parameters.AddWithValue("@UserId", userId);
            profileCommand.Parameters.AddWithValue("@FullName", profile.FullName);
            profileCommand.Parameters.AddWithValue("@DateOfBirth", profile.DateOfBirth);
            profileCommand.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber);
            await profileCommand.ExecuteNonQueryAsync();

            var otpQuery = @"INSERT INTO EmailOtps (UserId, CodeHash, Purpose, ExpiresAt, AttemptCount, MaxAttempts, CreatedAt) 
                             VALUES (@UserId, @CodeHash, @Purpose, @ExpiresAt, 0, 5, GETUTCDATE())";
            using var otpCommand = new SqlCommand(otpQuery, connection, transaction);
            otpCommand.Parameters.AddWithValue("@UserId", userId);
            otpCommand.Parameters.AddWithValue("@CodeHash", otp.CodeHash);
            otpCommand.Parameters.AddWithValue("@Purpose", otp.Purpose);
            otpCommand.Parameters.AddWithValue("@ExpiresAt", otp.ExpiresAt);
            await otpCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<EmailOtp?> GetLatestOtpAsync(string email, string purpose)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = @"
            SELECT TOP 1 eo.Id, eo.UserId, eo.CodeHash, eo.Purpose, eo.ExpiresAt, eo.UsedAt, eo.AttemptCount, eo.CreatedAt 
            FROM EmailOtps eo
            JOIN Users u ON eo.UserId = u.Id
            WHERE u.Email = @Email AND eo.Purpose = @Purpose
            ORDER BY eo.CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Purpose", purpose);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new EmailOtp
            {
                Id = reader.GetInt64(0),
                UserId = reader.GetInt64(1),
                CodeHash = (byte[])reader[2],
                Purpose = reader.GetString(3),
                ExpiresAt = reader.GetDateTime(4),
                UsedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                AttemptCount = reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7)
            };
        }
        return null;
    }

    public async Task UpdateOtpAsync(EmailOtp otp, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();
        
        var query = "UPDATE EmailOtps SET UsedAt = @UsedAt, AttemptCount = @AttemptCount WHERE Id = @Id";
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@Id", otp.Id);
        command.Parameters.AddWithValue("@UsedAt", (object?)otp.UsedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@AttemptCount", otp.AttemptCount);
        
        await command.ExecuteNonQueryAsync();
        
        if (shouldClose) await connection!.CloseAsync();
    }
    
    public async Task SaveEmailOtpAsync(EmailOtp otp)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = @"INSERT INTO EmailOtps (UserId, CodeHash, Purpose, ExpiresAt, AttemptCount, MaxAttempts, CreatedAt) 
                         VALUES (@UserId, @CodeHash, @Purpose, @ExpiresAt, 0, 5, GETUTCDATE())";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", otp.UserId);
        command.Parameters.AddWithValue("@CodeHash", otp.CodeHash);
        command.Parameters.AddWithValue("@Purpose", otp.Purpose);
        command.Parameters.AddWithValue("@ExpiresAt", otp.ExpiresAt);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = "SELECT Id, UserId, TokenHash, ExpiresAt, RevokedAt, RevokedByIp, ReplacedByTokenId, CreatedAt FROM RefreshTokens WHERE TokenHash = @TokenHash";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TokenHash", System.Text.Encoding.UTF8.GetBytes(tokenHash));
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new RefreshToken
            {
                Id = reader.GetGuid(0),
                UserId = reader.GetInt64(1),
                TokenHash = (byte[])reader[2],
                ExpiresAt = reader.GetDateTime(3),
                RevokedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                RevokedByIp = reader.IsDBNull(5) ? null : reader.GetString(5),
                ReplacedByTokenId = reader.IsDBNull(6) ? null : reader.GetGuid(6),
                CreatedAt = reader.GetDateTime(7)
            };
        }
        return null;
    }

    public async Task AddRefreshTokenAsync(RefreshToken token, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();

        var query = @"INSERT INTO RefreshTokens (Id, UserId, TokenHash, ExpiresAt, RevokedAt, RevokedByIp, ReplacedByTokenId, CreatedAt) 
                      VALUES (@Id, @UserId, @TokenHash, @ExpiresAt, @RevokedAt, @RevokedByIp, @ReplacedByTokenId, @CreatedAt)";
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@Id", token.Id);
        command.Parameters.AddWithValue("@UserId", token.UserId);
        command.Parameters.AddWithValue("@TokenHash", token.TokenHash);
        command.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);
        command.Parameters.AddWithValue("@RevokedAt", (object?)token.RevokedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@RevokedByIp", (object?)token.RevokedByIp ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReplacedByTokenId", (object?)token.ReplacedByTokenId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
        
        await command.ExecuteNonQueryAsync();
        
        if (shouldClose) await connection!.CloseAsync();
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken token, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();

        var query = "UPDATE RefreshTokens SET RevokedAt = @RevokedAt, RevokedByIp = @RevokedByIp, ReplacedByTokenId = @ReplacedByTokenId WHERE Id = @Id";
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@Id", token.Id);
        command.Parameters.AddWithValue("@RevokedAt", (object?)token.RevokedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@RevokedByIp", (object?)token.RevokedByIp ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReplacedByTokenId", (object?)token.ReplacedByTokenId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();

        if (shouldClose) await connection!.CloseAsync();
    }

    public async Task RevokeAllUserRefreshTokensAsync(long userId, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();

        var query = "UPDATE RefreshTokens SET RevokedAt = GETUTCDATE() WHERE UserId = @UserId AND RevokedAt IS NULL";
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@UserId", userId);

        await command.ExecuteNonQueryAsync();

        if (shouldClose) await connection!.CloseAsync();
    }
}
