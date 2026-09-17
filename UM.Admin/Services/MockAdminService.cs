using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockAdminService
    {
        private static MockAdminService? _instance;
        public static MockAdminService Instance => _instance ??= new MockAdminService();

        public AdminUser CurrentAdmin { get; set; }
        public List<AdminUser> Admins { get; private set; } = new();
        public List<AdminActivity> Activities { get; private set; } = new();

        private MockAdminService()
        {
            SeedAdmins();
            SeedActivities();
            CurrentAdmin = Admins[0]; // Default to SuperAdmin
        }

        private void SeedAdmins()
        {
            Admins = new List<AdminUser>
            {
                new() { Id = 1, Name = "Admin Vinh", Email = "vinh.admin@um.com", Role = AdminRole.SuperAdmin, Status = AdminStatus.Active, CreatedDate = DateTime.Now.AddYears(-1), LastActive = DateTime.Now },
                new() { Id = 2, Name = "Admin Hung", Email = "hung.admin@um.com", Role = AdminRole.UserAdmin, Status = AdminStatus.Active, CreatedDate = DateTime.Now.AddMonths(-8), LastActive = DateTime.Now.AddHours(-2) },
                new() { Id = 3, Name = "Admin Linh", Email = "linh.admin@um.com", Role = AdminRole.ServerAdmin, Status = AdminStatus.Active, CreatedDate = DateTime.Now.AddMonths(-6), LastActive = DateTime.Now.AddHours(-5) },
                new() { Id = 4, Name = "Admin Tuan", Email = "tuan.admin@um.com", Role = AdminRole.UserAdmin, Status = AdminStatus.Inactive, CreatedDate = DateTime.Now.AddMonths(-4), LastActive = DateTime.Now.AddDays(-30) },
                new() { Id = 5, Name = "Admin Hoa", Email = "hoa.admin@um.com", Role = AdminRole.ServerAdmin, Status = AdminStatus.Active, CreatedDate = DateTime.Now.AddMonths(-2), LastActive = DateTime.Now.AddHours(-1) },
            };
        }

        private void SeedActivities()
        {
            Activities = new List<AdminActivity>
            {
                new() { Id = 1, Admin = "Admin Vinh", Action = "Banned user", Target = "Le Van C", Time = DateTime.Now.AddHours(-1), Result = "Success" },
                new() { Id = 2, Admin = "Admin Hung", Action = "Unbanned user", Target = "Hoang Van E", Time = DateTime.Now.AddHours(-3), Result = "Success" },
                new() { Id = 3, Admin = "Admin Linh", Action = "Locked server", Target = "Music Lovers", Time = DateTime.Now.AddHours(-5), Result = "Success" },
                new() { Id = 4, Admin = "Admin Vinh", Action = "Resolved report", Target = "Report #4", Time = DateTime.Now.AddHours(-8), Result = "Success" },
                new() { Id = 5, Admin = "Admin Hung", Action = "Updated user info", Target = "Nguyen Van A", Time = DateTime.Now.AddDays(-1), Result = "Success" },
                new() { Id = 6, Admin = "Admin Vinh", Action = "Changed admin role", Target = "Admin Tuan", Time = DateTime.Now.AddDays(-1).AddHours(-2), Result = "Success" },
                new() { Id = 7, Admin = "Admin Linh", Action = "Unlocked server", Target = "Coding Dojo", Time = DateTime.Now.AddDays(-2), Result = "Success" },
                new() { Id = 8, Admin = "Admin Vinh", Action = "Banned user", Target = "Dang Van F", Time = DateTime.Now.AddDays(-2).AddHours(-5), Result = "Success" },
                new() { Id = 9, Admin = "Admin Hung", Action = "Rejected report", Target = "Report #5", Time = DateTime.Now.AddDays(-3), Result = "Success" },
                new() { Id = 10, Admin = "Admin Vinh", Action = "Added new admin", Target = "Admin Hoa", Time = DateTime.Now.AddDays(-5), Result = "Success" },
            };
        }

        public void AddActivity(string action, string target)
        {
            Activities.Insert(0, new AdminActivity
            {
                Id = Activities.Any() ? Activities.Max(a => a.Id) + 1 : 1,
                Admin = CurrentAdmin.Name,
                Action = action,
                Target = target,
                Time = DateTime.Now,
                Result = "Success"
            });
        }

        public void AddAdmin(AdminUser admin)
        {
            admin.Id = Admins.Any() ? Admins.Max(a => a.Id) + 1 : 1;
            Admins.Add(admin);
        }

        public void UpdateAdmin(AdminUser admin)
        {
            var existing = Admins.FirstOrDefault(a => a.Id == admin.Id);
            if (existing != null)
            {
                existing.Name = admin.Name;
                existing.Email = admin.Email;
                existing.Role = admin.Role;
                existing.Status = admin.Status;
            }
        }

        public void RemoveAdmin(int adminId)
        {
            Admins.RemoveAll(a => a.Id == adminId);
        }
    }
}
