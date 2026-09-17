using UM.Admin.Models;

namespace UM.Admin.Services
{
    public class MockServerService
    {
        private static MockServerService? _instance;
        public static MockServerService Instance => _instance ??= new MockServerService();

        public List<Server> Servers { get; private set; } = new();
        public Dictionary<long, List<ServerMember>> ServerMembers { get; private set; } = new();

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
                new() { 
                    Id = 1, Name = "Du an SGU", Owner = users[0].DisplayName, OwnerId = users[0].Id, OwnerPublicCode = "USR001",
                    Members = 25, Channels = 8, StorageUsed = "128 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-6),
                    Categories = new List<ServerCategory>
                    {
                        new() { Id = 1, ServerId = 1, Name = "TEXT CHANNELS", Position = 0, Channels = new() {
                            new() { Id = 1, ServerId = 1, CategoryId = 1, Name = "general", Type = 1, Position = 0 },
                            new() { Id = 2, ServerId = 1, CategoryId = 1, Name = "announcements", Type = 1, Position = 1 },
                            new() { Id = 3, ServerId = 1, CategoryId = 1, Name = "random", Type = 1, Position = 2 }
                        }},
                        new() { Id = 2, ServerId = 1, Name = "VOICE CHANNELS", Position = 1, Channels = new() {
                            new() { Id = 4, ServerId = 1, CategoryId = 2, Name = "Lounge", Type = 2, Position = 0 },
                            new() { Id = 5, ServerId = 1, CategoryId = 2, Name = "Meeting Room", Type = 2, Position = 1 }
                        }}
                    },
                    Roles = new List<ServerRoleItem>
                    {
                        new() { Id = 1, ServerId = 1, Name = "Owner", Color = "#FFD700", Position = 0, IsSystem = true, Permissions = new() { "ADMINISTRATOR" } },
                        new() { Id = 2, ServerId = 1, Name = "Moderator", Color = "#3498DB", Position = 1, IsSystem = false, Permissions = new() { "MANAGE_CHANNELS", "KICK_MEMBERS" } },
                        new() { Id = 3, ServerId = 1, Name = "Member", Color = "#99AAB5", Position = 2, IsSystem = true, Permissions = new() { "SEND_MESSAGES", "VIEW_CHANNEL" } }
                    }
                },
                new() { 
                    Id = 2, Name = "Nhom Hoc Tap", Owner = users[1].DisplayName, OwnerId = users[1].Id, OwnerPublicCode = "USR002",
                    Members = 15, Channels = 5, StorageUsed = "64 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-5),
                    Categories = new List<ServerCategory>
                    {
                        new() { Id = 3, ServerId = 2, Name = "CHUNG", Position = 0, Channels = new() {
                            new() { Id = 6, ServerId = 2, CategoryId = 3, Name = "thao-luan", Type = 1, Position = 0 }
                        }}
                    }
                },
                new() { Id = 3, Name = "Cau Lac Bo IT", Owner = users[3].DisplayName, OwnerId = users[3].Id, OwnerPublicCode = "USR004", Members = 42, Channels = 12, StorageUsed = "256 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-8) },
                new() { Id = 4, Name = "Game Community", Owner = users[7].DisplayName, OwnerId = users[7].Id, OwnerPublicCode = "USR008", Members = 120, Channels = 20, StorageUsed = "512 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-10) },
                new() { Id = 5, Name = "Music Lovers", Owner = users[6].DisplayName, OwnerId = users[6].Id, OwnerPublicCode = "USR007", Members = 35, Channels = 6, StorageUsed = "96 MB", Status = ServerStatus.Blocked, CreatedDate = DateTime.Now.AddMonths(-4) },
                new() { Id = 6, Name = "Design Hub", Owner = users[9].DisplayName, OwnerId = users[9].Id, OwnerPublicCode = "USR010", Members = 18, Channels = 4, StorageUsed = "200 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-3) },
                new() { Id = 7, Name = "Coding Dojo", Owner = users[0].DisplayName, OwnerId = users[0].Id, OwnerPublicCode = "USR001", Members = 55, Channels = 10, StorageUsed = "150 MB", Status = ServerStatus.Active, CreatedDate = DateTime.Now.AddMonths(-7) },
                new() { Id = 8, Name = "Startup Network", Owner = users[3].DisplayName, OwnerId = users[3].Id, OwnerPublicCode = "USR004", Members = 30, Channels = 7, StorageUsed = "80 MB", Status = ServerStatus.Blocked, CreatedDate = DateTime.Now.AddMonths(-2) },
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

        public List<ServerMember> GetMembers(long serverId)
        {
            return ServerMembers.GetValueOrDefault(serverId, new());
        }

        public Server? GetServerById(long serverId)
        {
            return Servers.FirstOrDefault(s => s.Id == serverId);
        }

        public void LockServer(long serverId)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null) server.Status = ServerStatus.Blocked;
        }

        public void UnlockServer(long serverId)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null) server.Status = ServerStatus.Active;
        }

        public Server AddServer(string name, long ownerId = 1, string ownerName = "Admin User")
        {
            long newId = Servers.Any() ? Servers.Max(s => s.Id) + 1 : 1;
            var newServer = new Server
            {
                Id = newId,
                Name = name,
                Owner = ownerName,
                OwnerId = ownerId,
                OwnerPublicCode = $"USR{ownerId:D3}",
                Members = 1,
                Channels = 1,
                StorageUsed = "0 MB",
                Status = ServerStatus.Active,
                CreatedDate = DateTime.Now,
                Categories = new List<ServerCategory>
                {
                    new() { Id = newId * 100 + 1, ServerId = newId, Name = "Text Channels", Position = 0, Channels = new() {
                        new() { Id = newId * 1000 + 1, ServerId = newId, CategoryId = newId * 100 + 1, Name = "general", Type = 1, Position = 0 }
                    }}
                }
            };
            Servers.Insert(0, newServer);
            ServerMembers[newId] = new List<ServerMember>
            {
                new() { UserId = (int)ownerId, Username = ownerName.ToLower().Replace(" ", ""), DisplayName = ownerName, Role = "Owner", JoinedDate = DateTime.Now, Status = UserStatus.Active }
            };
            return newServer;
        }

        public void UpdateServer(long serverId, string name)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null)
            {
                server.Name = name;
                server.UpdatedDate = DateTime.Now;
            }
        }

        public void ScheduleDeleteServer(long serverId)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null)
            {
                server.Status = ServerStatus.PendingDeletion;
                server.DeletedAt = DateTime.Now;
                server.ScheduledDeleteAt = DateTime.Now.AddDays(14);
            }
        }

        public void TransferOwnership(long serverId, long newOwnerId, string newOwnerName)
        {
            var server = Servers.FirstOrDefault(s => s.Id == serverId);
            if (server != null)
            {
                server.Owner = newOwnerName;
                server.OwnerId = newOwnerId;
                server.OwnerPublicCode = $"USR{newOwnerId:D3}";
                server.UpdatedDate = DateTime.Now;
            }
        }
    }
}
