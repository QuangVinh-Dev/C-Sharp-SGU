using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("SystemRoles")]
public class SystemRole
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
