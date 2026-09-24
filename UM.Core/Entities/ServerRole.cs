using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("ServerRoles")]
public class ServerRole
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("ServerId")]
    public long ServerId { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("IsSystem")]
    public bool IsSystem { get; set; }

    [Column("Position")]
    public int Position { get; set; }

    [Column("Color")]
    public string? Color { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}