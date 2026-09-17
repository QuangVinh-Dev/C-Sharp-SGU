namespace UM.Admin.Models
{
    public class AdminActivity
    {
        public int Id { get; set; }
        public string Admin { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.Now;
        public string Result { get; set; } = "Success";
    }
}
