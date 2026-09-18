using System.Data;
using Microsoft.Data.SqlClient;
using BackendApi.Infrastructure.Database;
using BackendApi.Models;

namespace BackendApi.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(long userId);
    Task UpdateAsync(UserProfile profile, SqlTransaction? transaction = null);
    Task CreateAsync(UserProfile profile, SqlTransaction? transaction = null);
}

public class UserProfileRepository : IUserProfileRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserProfileRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserProfile?> GetByUserIdAsync(long userId)
    {
        using var connection = _connectionFactory.CreateConnection() as SqlConnection;
        await connection!.OpenAsync();

        var query = "SELECT UserId, FullName, DateOfBirth, PhoneNumber, CreatedAt, UpdatedAt FROM UserProfiles WHERE UserId = @UserId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UserProfile
            {
                UserId = reader.GetInt64(0),
                FullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                DateOfBirth = reader.IsDBNull(2) ? default : reader.GetDateTime(2),
                PhoneNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                CreatedAt = reader.IsDBNull(4) ? default : reader.GetDateTime(4),
                UpdatedAt = reader.IsDBNull(5) ? default : reader.GetDateTime(5)
            };
        }
        return null;
    }

    public async Task CreateAsync(UserProfile profile, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();

        var query = @"INSERT INTO UserProfiles (UserId, FullName, DateOfBirth, PhoneNumber, CreatedAt, UpdatedAt)
                      VALUES (@UserId, @FullName, @DateOfBirth, @PhoneNumber, GETUTCDATE(), GETUTCDATE())";

        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@UserId", profile.UserId);
        command.Parameters.AddWithValue("@FullName", profile.FullName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DateOfBirth", profile.DateOfBirth == default ? (object)DBNull.Value : profile.DateOfBirth);
        command.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();

        if (shouldClose) await connection!.CloseAsync();
    }

    public async Task UpdateAsync(UserProfile profile, SqlTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? (_connectionFactory.CreateConnection() as SqlConnection);
        bool shouldClose = transaction == null;
        if (shouldClose) await connection!.OpenAsync();

        var query = @"UPDATE UserProfiles SET 
            FullName = @FullName, 
            DateOfBirth = @DateOfBirth, 
            PhoneNumber = @PhoneNumber, 
            UpdatedAt = GETUTCDATE()
            WHERE UserId = @UserId";

        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@UserId", profile.UserId);
        command.Parameters.AddWithValue("@FullName", profile.FullName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DateOfBirth", profile.DateOfBirth == default ? (object)DBNull.Value : profile.DateOfBirth);
        command.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();

        if (shouldClose) await connection!.CloseAsync();
    }
}
