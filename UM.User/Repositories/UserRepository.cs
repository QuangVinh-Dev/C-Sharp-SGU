using System.Data;
using Microsoft.Data.SqlClient;
using BackendApi.Infrastructure.Database;
using BackendApi.Models;

namespace BackendApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(long id);
    Task UpdateAsync(User user, SqlTransaction? transaction = null);
    Task<IEnumerable<string>> GetUserRolesAsync(long userId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(long userId);
}

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        
        var query = "SELECT Id, Email, PasswordHash, IsEmailVerified, IsActive, IsSuspended, FailedLoginCount, LockedUntil, LastLoginAt, CreatedAt, UpdatedAt FROM Users WHERE Email = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        
        var query = "SELECT Id, Email, PasswordHash, IsEmailVerified, IsActive, IsSuspended, FailedLoginCount, LockedUntil, LastLoginAt, CreatedAt, UpdatedAt FROM Users WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task UpdateAsync(User user, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();
        
        var query = @"UPDATE Users SET 
            Email = @Email, PasswordHash = @PasswordHash, IsEmailVerified = @IsEmailVerified,
            IsActive = @IsActive, IsSuspended = @IsSuspended, FailedLoginCount = @FailedLoginCount,
            LockedUntil = @LockedUntil, LastLoginAt = @LastLoginAt, UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";
            
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@IsEmailVerified", user.IsEmailVerified);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@IsSuspended", user.IsSuspended);
        command.Parameters.AddWithValue("@FailedLoginCount", user.FailedLoginCount);
        command.Parameters.AddWithValue("@LockedUntil", (object?)user.LockedUntil ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastLoginAt", (object?)user.LastLoginAt ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();

        if (shouldClose) await connection!.CloseAsync();
    }
    
    public async Task<IEnumerable<string>> GetUserRolesAsync(long userId)
    {
        var roles = new List<string>();
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = @"
    SELECT DISTINCT sp.Code 
    FROM SystemAdmins sa
    JOIN SystemRoles sr 
        ON sa.SystemRoleId = sr.Id
    JOIN SystemRolePermissions srp 
        ON sr.Id = srp.SystemRoleId
    JOIN SystemPermissions sp 
        ON srp.PermissionId = sp.Id
    WHERE sa.UserId = @UserId
      AND sa.RevokedAt IS NULL";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            roles.Add(reader.GetString(0));
        }
        return roles;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
    {
        var permissions = new List<string>();
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();
        var query = @"
            SELECT sp.Code 
            FROM SystemAdmins sa
            JOIN SystemRoles sr ON sa.SystemRoleId = sr.Id
            JOIN SystemRolePermissions srp ON sr.Id = srp.SystemRoleId
            JOIN SystemPermissions sp ON srp.PermissionId = sp.Id
            WHERE sa.UserId = @UserId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            permissions.Add(reader.GetString(0));
        }
        return permissions;
    }

    private User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt64(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            IsEmailVerified = reader.GetBoolean(3),
            IsActive = reader.GetBoolean(4),
            IsSuspended = reader.GetBoolean(5),
            FailedLoginCount = reader.GetInt32(6),
            LockedUntil = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            LastLoginAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            CreatedAt = reader.GetDateTime(9),
            UpdatedAt = reader.GetDateTime(10)
        };
    }
}
