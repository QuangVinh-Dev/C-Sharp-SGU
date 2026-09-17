using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chuẩn 100% bảng RolePermissions trong um_database
    /// Composite PK: (ServerRoleId, PermissionId)
    /// </summary>
    [Table("RolePermissions")]
    public class RolePermission
    {
        public long ServerRoleId { get; set; }

        public int PermissionId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ServerRoleId))]
        public ServerRole ServerRole { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;
    }
}
