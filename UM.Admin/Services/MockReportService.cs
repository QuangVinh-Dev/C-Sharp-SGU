using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockReportService
    {
        private static MockReportService? _instance;
        public static MockReportService Instance => _instance ??= new MockReportService();

        public List<Report> Reports { get; private set; } = new();

        private MockReportService()
        {
            SeedReports();
        }

        private void SeedReports()
        {
            var users = MockUserService.Instance.Users;
            Reports = new List<Report>
            {
                new() { Id = 1, Reporter = users[0].DisplayName, ReporterId = users[0].Id, TargetType = ReportTargetType.User, Target = users[2].DisplayName, TargetId = users[2].Id, Reason = "Spam messages", Description = "User is sending spam messages repeatedly in multiple channels.", Status = ReportStatus.Pending, CreatedTime = DateTime.Now.AddDays(-1), Evidences = new() { new() { Id = 1, Type = EvidenceType.Screenshot, Name = "spam_screenshot.png", Content = "[Screenshot of spam messages]" }, new() { Id = 2, Type = EvidenceType.MessageExcerpt, Name = "Message log", Content = "Buy cheap items at www.spam-link.com! Best deals!" } } },
                new() { Id = 2, Reporter = users[1].DisplayName, ReporterId = users[1].Id, TargetType = ReportTargetType.User, Target = users[4].DisplayName, TargetId = users[4].Id, Reason = "Harassment", Description = "This user has been harassing members with offensive language.", Status = ReportStatus.Reviewing, CreatedTime = DateTime.Now.AddDays(-2), AssignedAdmin = "Admin Vinh", Evidences = new() { new() { Id = 3, Type = EvidenceType.MessageExcerpt, Name = "Offensive messages", Content = "[Multiple offensive messages directed at other users]" } } },
                new() { Id = 3, Reporter = users[3].DisplayName, ReporterId = users[3].Id, TargetType = ReportTargetType.Server, Target = "Music Lovers", TargetId = 5, Reason = "Inappropriate content", Description = "Server is sharing inappropriate content in public channels.", Status = ReportStatus.Pending, CreatedTime = DateTime.Now.AddDays(-3), Evidences = new() { new() { Id = 4, Type = EvidenceType.Screenshot, Name = "content_screenshot.png", Content = "[Screenshot of inappropriate content]" } } },
                new() { Id = 4, Reporter = users[6].DisplayName, ReporterId = users[6].Id, TargetType = ReportTargetType.User, Target = users[5].DisplayName, TargetId = users[5].Id, Reason = "Scam", Description = "User is running a scam operation, asking for money.", Status = ReportStatus.Resolved, CreatedTime = DateTime.Now.AddDays(-5), AssignedAdmin = "Admin Hung", Evidences = new() { new() { Id = 5, Type = EvidenceType.MessageExcerpt, Name = "Scam messages", Content = "Send me 500k VND and I'll double it! Trust me!" }, new() { Id = 6, Type = EvidenceType.File, Name = "chat_log.txt", Content = "chat_log_evidence.txt" } } },
                new() { Id = 5, Reporter = users[9].DisplayName, ReporterId = users[9].Id, TargetType = ReportTargetType.User, Target = users[8].DisplayName, TargetId = users[8].Id, Reason = "Impersonation", Description = "User is impersonating an administrator.", Status = ReportStatus.Rejected, CreatedTime = DateTime.Now.AddDays(-7), AssignedAdmin = "Admin Linh", Evidences = new() { new() { Id = 7, Type = EvidenceType.Screenshot, Name = "impersonation_proof.png", Content = "[Screenshot showing fake admin badge]" } } },
                new() { Id = 6, Reporter = users[10].DisplayName, ReporterId = users[10].Id, TargetType = ReportTargetType.Message, Target = "Offensive message in #general", TargetId = 0, Reason = "Hate speech", Description = "A message containing hate speech was posted in general channel.", Status = ReportStatus.Pending, CreatedTime = DateTime.Now.AddHours(-6), Evidences = new() { new() { Id = 8, Type = EvidenceType.MessageExcerpt, Name = "Hate speech message", Content = "[Offensive content removed for review]" } } },
                new() { Id = 7, Reporter = users[7].DisplayName, ReporterId = users[7].Id, TargetType = ReportTargetType.Server, Target = "Startup Network", TargetId = 8, Reason = "Illegal activity", Description = "Server is being used to coordinate illegal activities.", Status = ReportStatus.Reviewing, CreatedTime = DateTime.Now.AddDays(-4), AssignedAdmin = "Admin Vinh", Evidences = new() { new() { Id = 9, Type = EvidenceType.Screenshot, Name = "illegal_activity.png", Content = "[Evidence of illegal coordination]" }, new() { Id = 10, Type = EvidenceType.RelatedInfo, Name = "Related server info", Content = "Server has been flagged by multiple users previously." } } },
                new() { Id = 8, Reporter = users[12].DisplayName, ReporterId = users[12].Id, TargetType = ReportTargetType.User, Target = users[11].DisplayName, TargetId = users[11].Id, Reason = "Account trading", Description = "User is selling/trading accounts in DMs.", Status = ReportStatus.Pending, CreatedTime = DateTime.Now.AddHours(-12), Evidences = new() { new() { Id = 11, Type = EvidenceType.MessageExcerpt, Name = "Account trading DM", Content = "Selling level 50 account, DM me for price" } } },
            };
        }

        public List<Report> GetReportsForServer(int serverId)
        {
            return Reports.Where(r => r.TargetType == ReportTargetType.Server && r.TargetId == serverId).ToList();
        }

        public List<Report> GetReportsForUser(int userId)
        {
            return Reports.Where(r => r.TargetType == ReportTargetType.User && r.TargetId == userId).ToList();
        }

        public void UpdateReportStatus(int reportId, ReportStatus newStatus, string? admin = null)
        {
            var report = Reports.FirstOrDefault(r => r.Id == reportId);
            if (report != null)
            {
                report.Status = newStatus;
                if (admin != null) report.AssignedAdmin = admin;
            }
        }
    }
}
