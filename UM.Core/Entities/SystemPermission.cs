using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("SystemPermissions")]
public class SystemPermission
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("Code")]
    public string Code { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }
}
