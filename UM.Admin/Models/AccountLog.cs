namespace UM.Admin.Models
{
    public class AccountLog
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Action { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
