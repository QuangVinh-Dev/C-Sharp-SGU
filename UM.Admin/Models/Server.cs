namespace UM.Admin.Models
{
    public enum ServerStatus { Active, Locked }

    public class Server
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public int Members { get; set; }
        public int Channels { get; set; }
        public string StorageUsed { get; set; } = "0 MB";
        public ServerStatus Status { get; set; } = ServerStatus.Active;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
