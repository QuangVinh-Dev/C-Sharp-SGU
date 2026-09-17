namespace UM.Admin.Models
{
    public enum UserStatus { Active, Banned, Suspended }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public string Role { get; set; } = "Member";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public int ServersJoined { get; set; }
        public int ReportCount { get; set; }
        public string? BanReason { get; set; }
        public string? BannedBy { get; set; }
        public DateTime? BanTime { get; set; }
        public string? BanDuration { get; set; }
    }
}
