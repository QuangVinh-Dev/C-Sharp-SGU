namespace UM.Admin.Models
{
    public enum EvidenceType { Screenshot, File, MessageExcerpt, RelatedInfo }

    public class Evidence
    {
        public int Id { get; set; }
        public EvidenceType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
