using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("Permissions")]
public class Permission
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column("Code")]
    public string Code { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }
}