using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockUserService
    {
        private static MockUserService? _instance;
        public static MockUserService Instance => _instance ??= new MockUserService();

        public List<User> Users { get; private set; } = new();
        public List<AccountLog> AccountLogs { get; private set; } = new();

        private MockUserService()
        {
            SeedUsers();
            SeedAccountLogs();
        }

        private void SeedUsers()
        {
            Users = new List<User>
            {
                new() { Id = 1, Username = "nguyenvana", DisplayName = "Nguyen Van A", Email = "vana@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-6), LastActive = DateTime.Now.AddHours(-1), ServersJoined = 3, ReportCount = 0 },
                new() { Id = 2, Username = "tranthib", DisplayName = "Tran Thi B", Email = "thib@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-5), LastActive = DateTime.Now.AddHours(-2), ServersJoined = 5, ReportCount = 1 },
                new() { Id = 3, Username = "levanc", DisplayName = "Le Van C", Email = "vanc@email.com", Status = UserStatus.Banned, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-4), LastActive = DateTime.Now.AddDays(-10), ServersJoined = 2, ReportCount = 5, BanReason = "Spam and harassment", BannedBy = "Admin Vinh", BanTime = DateTime.Now.AddDays(-10), BanDuration = "Permanent" },
                new() { Id = 4, Username = "phamthid", DisplayName = "Pham Thi D", Email = "thid@email.com", Status = UserStatus.Active, Role = "Moderator", CreatedDate = DateTime.Now.AddMonths(-3), LastActive = DateTime.Now.AddMinutes(-30), ServersJoined = 7, ReportCount = 0 },
                new() { Id = 5, Username = "hoangvane", DisplayName = "Hoang Van E", Email = "vane@email.com", Status = UserStatus.Suspended, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-2), LastActive = DateTime.Now.AddDays(-3), ServersJoined = 1, ReportCount = 3, BanReason = "Suspicious activity", BannedBy = "Admin Hung", BanTime = DateTime.Now.AddDays(-3), BanDuration = "7 days" },
                new() { Id = 6, Username = "dangvanf", DisplayName = "Dang Van F", Email = "vanf@email.com", Status = UserStatus.Banned, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-8), LastActive = DateTime.Now.AddDays(-30), ServersJoined = 4, ReportCount = 8, BanReason = "Repeated violations", BannedBy = "Admin Vinh", BanTime = DateTime.Now.AddDays(-30), BanDuration = "Permanent" },
                new() { Id = 7, Username = "vuthig", DisplayName = "Vu Thi G", Email = "thig@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-1), LastActive = DateTime.Now.AddHours(-5), ServersJoined = 2, ReportCount = 0 },
                new() { Id = 8, Username = "buivanh", DisplayName = "Bui Van H", Email = "vanh@email.com", Status = UserStatus.Active, Role = "Admin", CreatedDate = DateTime.Now.AddMonths(-7), LastActive = DateTime.Now, ServersJoined = 6, ReportCount = 0 },
                new() { Id = 9, Username = "ngothii", DisplayName = "Ngo Thi I", Email = "thii@email.com", Status = UserStatus.Banned, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-9), LastActive = DateTime.Now.AddDays(-45), ServersJoined = 3, ReportCount = 6, BanReason = "Scam attempt", BannedBy = "Admin Linh", BanTime = DateTime.Now.AddDays(-45), BanDuration = "Permanent" },
                new() { Id = 10, Username = "dovank", DisplayName = "Do Van K", Email = "vank@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-1), LastActive = DateTime.Now.AddHours(-8), ServersJoined = 1, ReportCount = 0 },
                new() { Id = 11, Username = "lythil", DisplayName = "Ly Thi L", Email = "thil@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddDays(-20), LastActive = DateTime.Now.AddHours(-3), ServersJoined = 2, ReportCount = 1 },
                new() { Id = 12, Username = "trinhvanm", DisplayName = "Trinh Van M", Email = "vanm@email.com", Status = UserStatus.Banned, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-10), LastActive = DateTime.Now.AddDays(-60), ServersJoined = 5, ReportCount = 10, BanReason = "Account trading", BannedBy = "Admin Vinh", BanTime = DateTime.Now.AddDays(-60), BanDuration = "Permanent" },
                new() { Id = 13, Username = "maithio", DisplayName = "Mai Thi O", Email = "thio@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddDays(-15), LastActive = DateTime.Now.AddHours(-1), ServersJoined = 1, ReportCount = 0 },
                new() { Id = 14, Username = "duongvanp", DisplayName = "Duong Van P", Email = "vanp@email.com", Status = UserStatus.Banned, Role = "Member", CreatedDate = DateTime.Now.AddMonths(-11), LastActive = DateTime.Now.AddDays(-90), ServersJoined = 8, ReportCount = 12, BanReason = "Distributing malware", BannedBy = "Admin Hung", BanTime = DateTime.Now.AddDays(-90), BanDuration = "Permanent" },
                new() { Id = 15, Username = "caothiq", DisplayName = "Cao Thi Q", Email = "thiq@email.com", Status = UserStatus.Active, Role = "Member", CreatedDate = DateTime.Now.AddDays(-5), LastActive = DateTime.Now, ServersJoined = 1, ReportCount = 0 },
            };
        }

        private void SeedAccountLogs()
        {
            AccountLogs = new List<AccountLog>();
            var actions = new[] { "Login", "Logout", "Profile update", "Password change", "Server join", "Server leave", "Report created", "Ban", "Unban" };
            var rand = new Random(42);
            foreach (var user in Users)
            {
                for (int i = 0; i < rand.Next(3, 8); i++)
                {
                    AccountLogs.Add(new AccountLog
                    {
                        Time = DateTime.Now.AddDays(-rand.Next(0, 60)).AddHours(-rand.Next(0, 24)),
                        Action = actions[rand.Next(actions.Length)],
                        Source = user.Username,
                        Description = $"{user.DisplayName} performed {actions[rand.Next(actions.Length)].ToLower()}"
                    });
                }
            }
            AccountLogs = AccountLogs.OrderByDescending(l => l.Time).ToList();
        }

        public List<AccountLog> GetLogsForUser(int userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return new();
            return AccountLogs.Where(l => l.Source == user.Username).OrderByDescending(l => l.Time).ToList();
        }

        public void BanUser(int userId, string reason, string bannedBy)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Status = UserStatus.Banned;
                user.BanReason = reason;
                user.BannedBy = bannedBy;
                user.BanTime = DateTime.Now;
                user.BanDuration = "Permanent";
            }
        }

        public void UnbanUser(int userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Status = UserStatus.Active;
                user.BanReason = null;
                user.BannedBy = null;
                user.BanTime = null;
                user.BanDuration = null;
            }
        }

        public void AddUser(User user)
        {
            user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
            Users.Add(user);
        }

        public void UpdateUser(User user)
        {
            var existing = Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.DisplayName = user.DisplayName;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.Status = user.Status;
            }
        }

        public void DeleteUser(int userId)
        {
            Users.RemoveAll(u => u.Id == userId);
        }
    }
}
