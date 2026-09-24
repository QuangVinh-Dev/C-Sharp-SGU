using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("RolePermissions")]
public class RolePermission
{
    [PrimaryKey]
    [Column("ServerRoleId")]
    public long ServerRoleId { get; set; }

    [PrimaryKey]
    [Column("PermissionId")]
    public int PermissionId { get; set; }
}