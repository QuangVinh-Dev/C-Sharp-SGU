using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UM.Admin.Models;

namespace UM.Admin.Services
{
    /// <summary>
    /// API Service kết nối trực tiếp WinForms UM.Admin với ASP.NET Core Backend (UM.Core).
    /// Hỗ trợ graceful fallback sang Mock data khi Backend offline để đảm bảo UI không bao giờ crash.
    /// </summary>
    public class ServerAdminApiService
    {
        private static ServerAdminApiService? _instance;
        public static ServerAdminApiService Instance => _instance ??= new ServerAdminApiService();

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        public bool IsBackendConnected { get; private set; } = false;
        public string BaseUrl { get; set; } = "http://localhost:5000";

        private ServerAdminApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(5)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private void PrepareHeaders()
        {
            var admin = MockAdminService.Instance.CurrentAdmin;
            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            _httpClient.DefaultRequestHeaders.Remove("X-Admin-Role");

            _httpClient.DefaultRequestHeaders.Add("X-User-Id", admin.Id.ToString());
            _httpClient.DefaultRequestHeaders.Add("X-Admin-Role", admin.Role.ToString());
        }

        #region Server Operations

        public async Task<List<Server>> GetAllServersAsync(bool includeDeleted = true)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/servers?includeDeleted={includeDeleted}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<List<ServerSummaryApiDto>>(content, _jsonOptions);
                    if (list != null)
                    {
                        IsBackendConnected = true;
                        return list.Select(s => new Server
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Owner = !string.IsNullOrEmpty(s.OwnerName) ? s.OwnerName : $"User #{s.OwnerId}",
                            OwnerId = s.OwnerId,
                            OwnerPublicCode = s.OwnerPublicCode,
                            IconFileId = s.IconFileId,
                            Members = s.MembersCount,
                            Channels = s.ChannelsCount,
                            StorageUsed = s.StorageUsed ?? "0 MB",
                            Status = s.DeletedAt.HasValue 
                                ? ServerStatus.PendingDeletion 
                                : (s.IsSuspended ? ServerStatus.Blocked : ServerStatus.Active),
                            CreatedDate = s.CreatedAt,
                            UpdatedDate = s.UpdatedAt,
                            DeletedAt = s.DeletedAt,
                            ScheduledDeleteAt = s.ScheduledDeleteAt
                        }).ToList();
                    }
                }
            }
            catch
            {
                IsBackendConnected = false;
            }

            // Fallback sang MockServerService khi Backend offline
            return MockServerService.Instance.Servers;
        }

        public async Task<Server?> GetServerDetailAsync(long serverId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/servers/{serverId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var detail = JsonSerializer.Deserialize<ServerDetailApiDto>(content, _jsonOptions);
                    if (detail != null)
                    {
                        IsBackendConnected = true;
                        var server = new Server
                        {
                            Id = detail.Id,
                            Name = detail.Name,
                            Owner = !string.IsNullOrEmpty(detail.OwnerName) ? detail.OwnerName : $"User #{detail.OwnerId}",
                            OwnerId = detail.OwnerId,
                            OwnerPublicCode = detail.OwnerPublicCode,
                            IconFileId = detail.IconFileId,
                            Members = detail.MembersCount,
                            Channels = (detail.Categories?.Sum(c => c.Channels?.Count ?? 0) ?? 0) + (detail.Channels?.Count ?? 0),
                            StorageUsed = "0 MB",
                            Status = detail.DeletedAt.HasValue
                                ? ServerStatus.PendingDeletion
                                : (detail.IsSuspended ? ServerStatus.Blocked : ServerStatus.Active),
                            CreatedDate = detail.CreatedAt,
                            UpdatedDate = detail.UpdatedAt,
                            DeletedAt = detail.DeletedAt,
                            ScheduledDeleteAt = detail.ScheduledDeleteAt,
                            Categories = detail.Categories?.Select(c => new ServerCategory
                            {
                                Id = c.Id,
                                ServerId = c.ServerId,
                                Name = c.Name,
                                Position = c.Position,
                                CreatedAt = c.CreatedAt,
                                Channels = c.Channels?.Select(ch => new ServerChannel
                                {
                                    Id = ch.Id,
                                    ServerId = ch.ServerId,
                                    CategoryId = ch.CategoryId,
                                    Name = ch.Name,
                                    Type = ch.Type,
                                    Position = ch.Position,
                                    CreatedAt = ch.CreatedAt
                                }).ToList() ?? new()
                            }).ToList() ?? new(),
                            RootChannels = detail.Channels?.Select(ch => new ServerChannel
                            {
                                Id = ch.Id,
                                ServerId = ch.ServerId,
                                CategoryId = ch.CategoryId,
                                Name = ch.Name,
                                Type = ch.Type,
                                Position = ch.Position,
                                CreatedAt = ch.CreatedAt
                            }).ToList() ?? new()
                        };
                        return server;
                    }
                }
            }
            catch
            {
                IsBackendConnected = false;
            }

            return MockServerService.Instance.GetServerById(serverId);
        }

        public async Task<Server> CreateServerAsync(string name, long? iconFileId = null)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, iconFileId }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/v1/servers", content);

                if (response.IsSuccessStatusCode)
                {
                    var resString = await response.Content.ReadAsStringAsync();
                    var detail = JsonSerializer.Deserialize<ServerDetailApiDto>(resString, _jsonOptions);
                    if (detail != null)
                    {
                        IsBackendConnected = true;
                        // Đồng bộ cả mock để thống nhất
                        var mock = MockServerService.Instance.AddServer(detail.Name, detail.OwnerId, detail.OwnerName);
                        mock.Id = detail.Id;
                        return mock;
                    }
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            // Fallback sang Mock
            var admin = MockAdminService.Instance.CurrentAdmin;
            return MockServerService.Instance.AddServer(name, admin.Id, admin.Name);
        }

        public async Task UpdateServerAsync(long serverId, string? name, long? iconFileId = null, bool? isSuspended = null)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, iconFileId, isSuspended }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"/api/v1/servers/{serverId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            // Cập nhật mock
            if (!string.IsNullOrEmpty(name))
            {
                MockServerService.Instance.UpdateServer(serverId, name);
            }
            if (isSuspended.HasValue)
            {
                if (isSuspended.Value) MockServerService.Instance.LockServer(serverId);
                else MockServerService.Instance.UnlockServer(serverId);
            }
        }

        public async Task DeleteServerAsync(long serverId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/v1/servers/{serverId}");
                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            // Đánh dấu delay delete trên mock
            MockServerService.Instance.ScheduleDeleteServer(serverId);
        }

        public async Task TransferOwnershipAsync(long serverId, long newOwnerId, string newOwnerName = "")
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { newOwnerId }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/v1/servers/{serverId}/transfer-ownership", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            MockServerService.Instance.TransferOwnership(serverId, newOwnerId, newOwnerName);
        }

        public async Task SetServerLockAsync(long serverId, bool isLocked)
        {
            await UpdateServerAsync(serverId, null, null, isLocked);
        }

        #endregion

        #region Category Operations

        public async Task<List<ServerCategory>> GetCategoriesAsync(long serverId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/servers/{serverId}/categories");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<List<CategoryApiDto>>(content, _jsonOptions);
                    if (list != null)
                    {
                        IsBackendConnected = true;
                        return list.Select(c => new ServerCategory
                        {
                            Id = c.Id,
                            ServerId = c.ServerId,
                            Name = c.Name,
                            Position = c.Position,
                            CreatedAt = c.CreatedAt,
                            Channels = c.Channels?.Select(ch => new ServerChannel
                            {
                                Id = ch.Id,
                                ServerId = ch.ServerId,
                                CategoryId = ch.CategoryId,
                                Name = ch.Name,
                                Type = ch.Type,
                                Position = ch.Position,
                                CreatedAt = ch.CreatedAt
                            }).ToList() ?? new()
                        }).ToList();
                    }
                }
            }
            catch
            {
                IsBackendConnected = false;
            }

            var server = MockServerService.Instance.GetServerById(serverId);
            return server?.Categories ?? new();
        }

        public async Task<ServerCategory> CreateCategoryAsync(long serverId, string name, int position = 0)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, position }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/v1/servers/{serverId}/categories", content);

                if (response.IsSuccessStatusCode)
                {
                    var resStr = await response.Content.ReadAsStringAsync();
                    var resDto = JsonSerializer.Deserialize<CategoryApiDto>(resStr, _jsonOptions);
                    if (resDto != null)
                    {
                        IsBackendConnected = true;
                        return new ServerCategory
                        {
                            Id = resDto.Id,
                            ServerId = resDto.ServerId,
                            Name = resDto.Name,
                            Position = resDto.Position,
                            CreatedAt = resDto.CreatedAt
                        };
                    }
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            // Fallback mock
            var s = MockServerService.Instance.GetServerById(serverId);
            var newCat = new ServerCategory
            {
                Id = DateTime.Now.Ticks % 100000,
                ServerId = serverId,
                Name = name,
                Position = position,
                CreatedAt = DateTime.Now
            };
            s?.Categories.Add(newCat);
            return newCat;
        }

        public async Task UpdateCategoryAsync(long categoryId, string name, int? position = null)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, position }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"/api/v1/categories/{categoryId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }
        }

        public async Task DeleteCategoryAsync(long categoryId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/v1/categories/{categoryId}");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }
        }

        #endregion

        #region Channel Operations

        public async Task<List<ServerChannel>> GetChannelsAsync(long serverId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/servers/{serverId}/channels");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<List<ChannelApiDto>>(content, _jsonOptions);
                    if (list != null)
                    {
                        IsBackendConnected = true;
                        return list.Select(ch => new ServerChannel
                        {
                            Id = ch.Id,
                            ServerId = ch.ServerId,
                            CategoryId = ch.CategoryId,
                            Name = ch.Name,
                            Type = ch.Type,
                            Position = ch.Position,
                            CreatedAt = ch.CreatedAt
                        }).ToList();
                    }
                }
            }
            catch
            {
                IsBackendConnected = false;
            }

            var s = MockServerService.Instance.GetServerById(serverId);
            var channels = new List<ServerChannel>();
            if (s != null)
            {
                channels.AddRange(s.RootChannels);
                foreach (var cat in s.Categories)
                {
                    channels.AddRange(cat.Channels);
                }
            }
            return channels;
        }

        public async Task<ServerChannel> CreateChannelAsync(long serverId, string name, long? categoryId = null, byte type = 1, int position = 0)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, categoryId, type, position }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/v1/servers/{serverId}/channels", content);

                if (response.IsSuccessStatusCode)
                {
                    var resStr = await response.Content.ReadAsStringAsync();
                    var resDto = JsonSerializer.Deserialize<ChannelApiDto>(resStr, _jsonOptions);
                    if (resDto != null)
                    {
                        IsBackendConnected = true;
                        return new ServerChannel
                        {
                            Id = resDto.Id,
                            ServerId = resDto.ServerId,
                            CategoryId = resDto.CategoryId,
                            Name = resDto.Name,
                            Type = resDto.Type,
                            Position = resDto.Position,
                            CreatedAt = resDto.CreatedAt
                        };
                    }
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }

            return new ServerChannel
            {
                Id = DateTime.Now.Ticks % 100000,
                ServerId = serverId,
                CategoryId = categoryId,
                Name = name,
                Type = type,
                Position = position,
                CreatedAt = DateTime.Now
            };
        }

        public async Task UpdateChannelAsync(long channelId, string name, long? categoryId = null, int? position = null)
        {
            PrepareHeaders();
            try
            {
                var payload = JsonSerializer.Serialize(new { name, categoryId, position }, _jsonOptions);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"/api/v1/channels/{channelId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }
        }

        public async Task DeleteChannelAsync(long channelId)
        {
            PrepareHeaders();
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/v1/channels/{channelId}");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Backend error ({response.StatusCode}): {err}");
                }
                IsBackendConnected = true;
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Backend error")))
            {
                IsBackendConnected = false;
            }
        }

        #endregion

        #region Internal DTOs for API Communication

        private class ServerSummaryApiDto
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public long OwnerId { get; set; }
            public string OwnerName { get; set; } = string.Empty;
            public string OwnerPublicCode { get; set; } = string.Empty;
            public long? IconFileId { get; set; }
            public int MembersCount { get; set; }
            public int ChannelsCount { get; set; }
            public string? StorageUsed { get; set; }
            public bool IsSuspended { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime? DeletedAt { get; set; }
            public DateTime? ScheduledDeleteAt { get; set; }
        }

        private class ServerDetailApiDto
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public long OwnerId { get; set; }
            public string OwnerName { get; set; } = string.Empty;
            public string OwnerPublicCode { get; set; } = string.Empty;
            public long? IconFileId { get; set; }
            public int MembersCount { get; set; }
            public bool IsSuspended { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime? DeletedAt { get; set; }
            public DateTime? ScheduledDeleteAt { get; set; }
            public List<CategoryApiDto>? Categories { get; set; }
            public List<ChannelApiDto>? Channels { get; set; }
        }

        private class CategoryApiDto
        {
            public long Id { get; set; }
            public long ServerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Position { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<ChannelApiDto>? Channels { get; set; }
        }

        private class ChannelApiDto
        {
            public long Id { get; set; }
            public long ServerId { get; set; }
            public long? CategoryId { get; set; }
            public string Name { get; set; } = string.Empty;
            public byte Type { get; set; }
            public int Position { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        #endregion
    }
}
