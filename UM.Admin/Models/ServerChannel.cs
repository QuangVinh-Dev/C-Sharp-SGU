namespace UM.Admin.Models
{
    public class ServerChannel
    {
        public long Id { get; set; }
        public long ServerId { get; set; }
        public long? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte Type { get; set; } = 1; // 1: Text, 2: Voice
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string TypeName => Type == 2 ? "Voice" : "Text";
    }
}
