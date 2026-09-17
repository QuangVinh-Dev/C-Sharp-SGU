using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockServerService
    {
        private static MockServerService? _instance;
        public static MockServerService Instance => _instance ??= new MockServerService();

        public List<Server> Servers { get; private set; } = new();
        public Dictionary<int, List<ServerMember>> ServerMembers { get; private set; } = new();

        private MockServerService()
        {
            SeedServers();
            SeedServerMembers();
        }

        private void SeedServers()
        {
            var users = MockUserService.Instance.Users;
            Servers = new List<Server>
            {
                new() { Id = 1, Name = "Du an SGU", Owner = users[0].DisplayName, OwnerId = users[0].Id, Members = 25, Channels = 8, StorageUsed = "128 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-6) },
                new() { Id = 2, Name = "Nhom Hoc Tap", Owner = users[1].DisplayName, OwnerId = users[1].Id, Members = 15, Channels = 5, StorageUsed = "64 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-5) },
                new() { Id = 3, Name = "Cau Lac Bo IT", Owner = users[3].DisplayName, OwnerId = users[3].Id, Members = 42, Channels = 12, StorageUsed = "256 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-8) },
                new() { Id = 4, Name = "Game Community", Owner = users[7].DisplayName, OwnerId = users[7].Id, Members = 120, Channels = 20, StorageUsed = "512 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-10) },
                new() { Id = 5, Name = "Music Lovers", Owner = users[6].DisplayName, OwnerId = users[6].Id, Members = 35, Channels = 6, StorageUsed = "96 MB", Status = ServerStatus.Locked, CreatedDate = DateTime.Now.AddMonths(-4) },
                new() { Id = 6, Name = "Design Hub", Owner = users[9].DisplayName, OwnerId = users[9].Id, Members = 18, Channels = 4, StorageUsed = "200 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-3) },
                new() { Id = 7, Name = "Coding Dojo", Owner = users[0].DisplayName, OwnerId = users[0].Id, Members = 55, Channels = 10, StorageUsed = "150 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-7) },
                new() { Id = 8, Name = "Startup Network", Owner = users[3].DisplayName, OwnerId = users[3].Id, Members = 30, Channels = 7, StorageUsed = "80 MB", Status = ServerStatus.Locked, CreatedDate = DateTime.Now.AddMonths(-2) },
            };
        }

        private void SeedServerMembers()
        {
            var users = MockUserService.Instance.Users;
            var rand = new Random(42);
            var roles = new[] { "Owner", "Admin", "Moderator", "Member", "Member", "Member", "Member" };

            foreach (var server in Servers)
            {
                var memberList = new List<ServerMember>();
                // Owner always first
                var owner = users.FirstOrDefault(u => u.Id == server.OwnerId);
                if (owner != null)
                {
                    memberList.Add(new ServerMember
                    {
                        UserId = owner.Id, Username = owner.Username,
                        DisplayName = owner.DisplayName, Role = "Owner",
                        JoinedDate = server.CreatedDate, Status = owner.Status
                    });
                }
                // Add random members
                var memberCount = Math.Min(server.Members - 1, users.Count - 1);
                var shuffled = users.Where(u => u.Id != server.OwnerId).OrderBy(_ => rand.Next()).Take(memberCount);
                foreach (var user in shuffled)
                {
                    memberList.Add(new ServerMember
                    {
                        UserId = user.Id, Username = user.Username,
                        DisplayName = user.DisplayName, Role = roles[rand.Next(roles.Length)],
                        JoinedDate = server.CreatedDate.AddDays(rand.Next(0, 60)),
                        Status = user.Status
                    });
                }
                ServerMembers[server.Id] = memberList;
            }
        }

        public List<ServerMember> GetMembers(int serverId)
        {
            return ServerMembers.GetValueOrDefault(serverId, new());
        }

        public void LockServer(int serverId)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null) server.Status = ServerStatus.Locked;
        }

        public void UnlockServer(int serverId)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null) server.Status = ServerStatus.Active;
        }
    }
}
