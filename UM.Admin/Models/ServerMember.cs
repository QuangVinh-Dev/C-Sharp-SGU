namespace UM.Admin.Models
{
    public class ServerMember
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
