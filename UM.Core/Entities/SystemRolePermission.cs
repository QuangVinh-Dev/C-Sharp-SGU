using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("SystemRolePermissions")]
public class SystemRolePermission
{
    [PrimaryKey(1)]
    [Column("SystemRoleId")]
    public int SystemRoleId { get; set; }

    [PrimaryKey(2)]
    [Column("PermissionId")]
    public int PermissionId { get; set; }
}
