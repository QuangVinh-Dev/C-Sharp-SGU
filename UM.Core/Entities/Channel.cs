using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("Channels")]
public class Channel
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("ServerId")]
    public long ServerId { get; set; }

    [Column("CategoryId")]
    public long? CategoryId { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Type")]
    public ChannelType Type { get; set; }

    [Column("Position")]
    public int Position { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }
}