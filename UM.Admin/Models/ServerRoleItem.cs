namespace UM.Admin.Models
{
    public class ServerRoleItem
    {
        public long Id { get; set; }
        public long ServerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#99AAB5";
        public int Position { get; set; }
        public bool IsSystem { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
