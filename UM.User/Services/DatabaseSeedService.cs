using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BackendApi.Infrastructure.Database;

namespace BackendApi.Services;

public class DatabaseSeedService : IHostedService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseSeedService> _logger;

    public DatabaseSeedService(
        ISqlConnectionFactory connectionFactory,
        ILogger<DatabaseSeedService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding...");

        try
        {
            using var connection =
                _connectionFactory.CreateConnection() as SqlConnection;

            await connection!.OpenAsync(cancellationToken);

            // =========================
            // SEED SYSTEM ROLES
            // =========================

            var roles = new[]
            {
                "SuperAdmin",
                "UserAdmin",
                "ServerAdmin",
                "Member",
                "ServerOwner"
            };

            foreach (var role in roles)
            {
                const string query = @"
                    IF NOT EXISTS (
                        SELECT 1
                        FROM SystemRoles
                        WHERE Name = @Name
                    )
                    BEGIN
                        INSERT INTO SystemRoles
                            (Name, Description, CreatedAt)
                        VALUES
                            (@Name, @Name, GETUTCDATE())
                    END";

                using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Name", role);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // =========================
            // SEED SYSTEM PERMISSIONS
            // =========================

            var permissions = new[]
            {
                "users.read",
                "users.create",
                "users.update",
                "users.suspend",
                "users.ban",

                "admins.read",
                "admins.create",
                "admins.update",
                "admins.revoke",

                "servers.read",
                "servers.create",
                "servers.update",
                "servers.suspend",
                "servers.delete",

                "reports.read",
                "reports.review",

                "sanctions.create",
                "sanctions.revoke",

                "audit.read"
            };

            foreach (var permission in permissions)
            {
                const string query = @"
                    IF NOT EXISTS (
                        SELECT 1
                        FROM SystemPermissions
                        WHERE Code = @Code
                    )
                    BEGIN
                        INSERT INTO SystemPermissions
                            (Id, Code, Description)
                        VALUES
                            (ISNULL((SELECT MAX(Id) FROM SystemPermissions), 0) + 1, @Code, @Description)
                    END";

                using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Code", permission);
                command.Parameters.AddWithValue("@Description", permission);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to seed database. The database might not be initialized yet.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
