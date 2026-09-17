using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockDashboardService
    {
        private static MockDashboardService? _instance;
        public static MockDashboardService Instance => _instance ??= new MockDashboardService();

        private MockDashboardService() { }

        public DashboardStatistics GetStatistics()
        {
            var users = MockUserService.Instance.Users;
            var servers = MockServerService.Instance.Servers;
            var reports = MockReportService.Instance.Reports;

            return new DashboardStatistics
            {
                TotalUsers = users.Count,
                TotalServers = servers.Count,
                NewUsersToday = users.Count(u => u.CreatedDate.Date == DateTime.Today),
                NewUsersThisWeek = users.Count(u => u.CreatedDate >= DateTime.Now.AddDays(-7)),
                NewUsersThisMonth = users.Count(u => u.CreatedDate >= DateTime.Now.AddDays(-30)),
                PendingReports = reports.Count(r => r.Status == ReportStatus.Pending),
                BannedAccounts = users.Count(u => u.Status == UserStatus.Banned),
            };
        }
    }
}
