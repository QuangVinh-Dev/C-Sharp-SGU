using LinqToDB;
using LinqToDB.Async;
using UM.Core.Entities;
using UM.Core.Repository;

namespace UM.Core.Seed;

public class DatabaseSeeder
{
    private readonly DatabaseConnection _db;

    public DatabaseSeeder(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedPermissionsAsync();
        await SeedRolePermissionsAsync();
        await SeedAdminAsync();
    }

    // ============================================================
    // 1. SEED SYSTEM ROLES
    // ============================================================
    private async Task SeedRolesAsync()
    {
        var roles = _db.GetTable<SystemRole>();

        var roleList = new[]
        {
            new SystemRole
            {
                Name = "SuperAdmin",
                Description = "SuperAdmin",
                CreatedAt = DateTime.UtcNow
            },

            new SystemRole
            {
                Name = "UserAdmin",
                Description = "UserAdmin",
                CreatedAt = DateTime.UtcNow
            },

            new SystemRole
            {
                Name = "ServerAdmin",
                Description = "ServerAdmin",
                CreatedAt = DateTime.UtcNow
            },

            new SystemRole
            {
                Name = "Member",
                Description = "Member",
                CreatedAt = DateTime.UtcNow
            },

            new SystemRole
            {
                Name = "ServerOwner",
                Description = "ServerOwner",
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in roleList)
        {
            var exists = await roles
                .AnyAsync(x => x.Name == role.Name);

            if (!exists)
            {
                await _db.InsertAsync(role);
            }
        }
    }

    // ============================================================
    // 2. SEED SYSTEM PERMISSIONS
    // DB v1.1: Id KHÔNG phải IDENTITY
    // ============================================================
    private async Task SeedPermissionsAsync()
    {
        var permissions = _db.GetTable<SystemPermission>();

        var permissionList = new[]
        {
            new SystemPermission
            {
                Id = 1,
                Code = "users.read",
                Description = "Read users"
            },

            new SystemPermission
            {
                Id = 2,
                Code = "users.create",
                Description = "Create users"
            },

            new SystemPermission
            {
                Id = 3,
                Code = "users.update",
                Description = "Update users"
            },

            new SystemPermission
            {
                Id = 4,
                Code = "users.suspend",
                Description = "Suspend users"
            },

            new SystemPermission
            {
                Id = 5,
                Code = "users.ban",
                Description = "Ban users"
            },

            new SystemPermission
            {
                Id = 6,
                Code = "admins.read",
                Description = "Read administrators"
            },

            new SystemPermission
            {
                Id = 7,
                Code = "admins.create",
                Description = "Create administrators"
            },

            new SystemPermission
            {
                Id = 8,
                Code = "admins.update",
                Description = "Update administrators"
            },

            new SystemPermission
            {
                Id = 9,
                Code = "admins.revoke",
                Description = "Revoke administrators"
            },

            new SystemPermission
            {
                Id = 10,
                Code = "servers.read",
                Description = "Read servers"
            },

            new SystemPermission
            {
                Id = 11,
                Code = "servers.create",
                Description = "Create servers"
            },

            new SystemPermission
            {
                Id = 12,
                Code = "servers.update",
                Description = "Update servers"
            },

            new SystemPermission
            {
                Id = 13,
                Code = "servers.suspend",
                Description = "Suspend servers"
            },

            new SystemPermission
            {
                Id = 14,
                Code = "servers.delete",
                Description = "Delete servers"
            },

            new SystemPermission
            {
                Id = 15,
                Code = "reports.read",
                Description = "Read reports"
            },

            new SystemPermission
            {
                Id = 16,
                Code = "reports.review",
                Description = "Review reports"
            },

            new SystemPermission
            {
                Id = 17,
                Code = "sanctions.create",
                Description = "Create sanctions"
            },

            new SystemPermission
            {
                Id = 18,
                Code = "sanctions.revoke",
                Description = "Revoke sanctions"
            },

            new SystemPermission
            {
                Id = 19,
                Code = "audit.read",
                Description = "Read audit logs"
            },

            new SystemPermission
            {
                Id = 20,
                Code = "sanctions.read",
                Description = "Read sanctions"
            }
        };

        foreach (var permission in permissionList)
        {
            var existing = await permissions
                .FirstOrDefaultAsync(x => x.Id == permission.Id);

            if (existing == null)
            {
                await _db.InsertAsync(permission);
                continue;
            }

            // Bảo vệ seed khỏi việc Id đã tồn tại nhưng Code bị sai.
            if (existing.Code != permission.Code)
            {
                throw new InvalidOperationException(
                    $"SystemPermission Id {permission.Id} đang có Code '{existing.Code}', " +
                    $"nhưng Seeder yêu cầu '{permission.Code}'.");
            }
        }
    }

    // ============================================================
    // 3. SEED SYSTEM ROLE - PERMISSION
    // ============================================================
    private async Task SeedRolePermissionsAsync()
    {
        var roles = _db.GetTable<SystemRole>();
        var permissions = _db.GetTable<SystemPermission>();
        var rolePermissions = _db.GetTable<SystemRolePermission>();

        // --------------------------------------------------------
        // SuperAdmin: toàn bộ 20 quyền
        // --------------------------------------------------------
        var superAdminRole = await roles
            .FirstAsync(x => x.Name == "SuperAdmin");

        await AssignPermissionsAsync(
            superAdminRole.Id,
            new[]
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

                "audit.read",

                "sanctions.read"
            },
            permissions,
            rolePermissions
        );

        // --------------------------------------------------------
        // UserAdmin: users.*
        // --------------------------------------------------------
        var userAdminRole = await roles
            .FirstAsync(x => x.Name == "UserAdmin");

        await AssignPermissionsAsync(
            userAdminRole.Id,
            new[]
            {
                "users.read",
                "users.create",
                "users.update",
                "users.suspend",
                "users.ban"
            },
            permissions,
            rolePermissions
        );

        // --------------------------------------------------------
        // ServerAdmin: servers.read -> servers.delete
        // --------------------------------------------------------
        var serverAdminRole = await roles
            .FirstAsync(x => x.Name == "ServerAdmin");

        await AssignPermissionsAsync(
            serverAdminRole.Id,
            new[]
            {
                "servers.read",
                "servers.create",
                "servers.update",
                "servers.suspend",
                "servers.delete"
            },
            permissions,
            rolePermissions
        );

        // --------------------------------------------------------
        // Member: users.read
        // --------------------------------------------------------
        var memberRole = await roles
            .FirstAsync(x => x.Name == "Member");

        await AssignPermissionsAsync(
            memberRole.Id,
            new[]
            {
                "users.read"
            },
            permissions,
            rolePermissions
        );

        // --------------------------------------------------------
        // ServerOwner: servers.read/create/update
        // --------------------------------------------------------
        var serverOwnerRole = await roles
            .FirstAsync(x => x.Name == "ServerOwner");

        await AssignPermissionsAsync(
            serverOwnerRole.Id,
            new[]
            {
                "servers.read",
                "servers.create",
                "servers.update"
            },
            permissions,
            rolePermissions
        );
    }

    // ============================================================
    // HELPER: ASSIGN PERMISSIONS
    // ============================================================
    private async Task AssignPermissionsAsync(
        int roleId,
        string[] permissionCodes,
        ITable<SystemPermission> permissions,
        ITable<SystemRolePermission> rolePermissions)
    {
        foreach (var code in permissionCodes)
        {
            var permission = await permissions
                .FirstOrDefaultAsync(x => x.Code == code);

            if (permission == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy SystemPermission với Code '{code}'.");
            }

            var exists = await rolePermissions
                .AnyAsync(x =>
                    x.SystemRoleId == roleId &&
                    x.PermissionId == permission.Id);

            if (!exists)
            {
                await _db.InsertAsync(new SystemRolePermission
                {
                    SystemRoleId = roleId,
                    PermissionId = permission.Id
                });
            }
        }
    }

    // ============================================================
    // 4. VERIFY / BOOTSTRAP ADMIN
    // ============================================================
    private async Task SeedAdminAsync()
{
    const string adminEmail = "admin@umcore.local";

    var adminUser = await _db
        .GetTable<User>()
        .FirstOrDefaultAsync(x => x.Email == adminEmail);

    if (adminUser == null)
    {
        throw new InvalidOperationException(
            $"Không tìm thấy bootstrap user '{adminEmail}'. " +
            "User này phải được tạo bởi DB schema v1.1.");
    }

    var superAdminRole = await _db
        .GetTable<SystemRole>()
        .FirstAsync(x => x.Name == "SuperAdmin");

    var systemAdmins = _db.GetTable<SystemAdmin>();

    var alreadyAdmin = await systemAdmins
        .AnyAsync(x =>
            x.UserId == adminUser.Id &&
            x.RevokedAt == null);

    if (alreadyAdmin)
    {
        return;
    }

    await _db.InsertAsync(new SystemAdmin
    {
        UserId = adminUser.Id,
        SystemRoleId = superAdminRole.Id,

        // DB v1.1 mặc định RequireMfa = 1
        RequireMfa = true,

        // Bootstrap admin do hệ thống tạo
        GrantedBy = null,

        GrantedAt = DateTime.UtcNow,

        RevokedBy = null,
        RevokedAt = null
    });
}
}