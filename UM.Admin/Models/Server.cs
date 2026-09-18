namespace UM.Admin.Models
{
    public enum ServerStatus 
    { 
        Active, 
        Blocked, 
        Locked = Blocked, 
        PendingDeletion 
    }

    public class Server
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public long OwnerId { get; set; }
        public string OwnerPublicCode { get; set; } = string.Empty;
        public long? IconFileId { get; set; }
        public int Members { get; set; }
        public int Channels { get; set; }
        public string StorageUsed { get; set; } = "0 MB";
        public ServerStatus Status { get; set; } = ServerStatus.Active;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ScheduledDeleteAt { get; set; }

        public List<ServerCategory> Categories { get; set; } = new();
        public List<ServerChannel> RootChannels { get; set; } = new();
        public List<ServerRoleItem> Roles { get; set; } = new();
    }
}
