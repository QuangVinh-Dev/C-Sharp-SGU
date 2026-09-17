namespace UM.Admin.Models
{
    public class ServerCategory
    {
        public long Id { get; set; }
        public long ServerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ServerChannel> Channels { get; set; } = new();
    }
}
