namespace UM.Admin.Models
{
    public class DashboardStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalServers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int PendingReports { get; set; }
        public int BannedAccounts { get; set; }
    }
}
