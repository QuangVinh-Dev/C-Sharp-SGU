namespace UM.Admin.Models
{
    public enum AdminRole { SuperAdmin, UserAdmin, ServerAdmin }
    public enum AdminStatus { Active, Inactive }

    public class AdminUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public AdminRole Role { get; set; } = AdminRole.UserAdmin;
        public AdminStatus Status { get; set; } = AdminStatus.Active;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
    }
}
