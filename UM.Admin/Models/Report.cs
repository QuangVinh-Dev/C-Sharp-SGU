namespace UM.Admin.Models
{
    public enum ReportStatus { Pending, Reviewing, Resolved, Rejected }
    public enum ReportTargetType { User, Server, Message }

    public class Report
    {
        public int Id { get; set; }
        public string Reporter { get; set; } = string.Empty;
        public int ReporterId { get; set; }
        public ReportTargetType TargetType { get; set; } = ReportTargetType.User;
        public string Target { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string? AssignedAdmin { get; set; }
        public List<Evidence> Evidences { get; set; } = new();
    }
}
