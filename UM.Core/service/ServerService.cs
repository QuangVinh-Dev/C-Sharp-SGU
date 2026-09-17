using UM.Core.config;
using UM.Core.dto;
using UM.Core.entity;
using UM.Core.repository;
using UM.Core.sercurity;
using UM.Core.websocket;

namespace UM.Core.service
{
    public interface IServerService
    {
        Task<ServerDetailDto> CreateServerAsync(CreateServerRequest request, long currentUserId);
        Task<List<ServerSummaryDto>> GetAllServersAsync(bool includeDeleted = false);
        Task<List<ServerSidebarDto>> GetMyServersAsync(long currentUserId);
        Task<ServerDetailDto> GetServerDetailAsync(long serverId, long currentUserId);
        Task<ServerDetailDto> UpdateServerAsync(long serverId, UpdateServerRequest request, long currentUserId);
        Task DeleteServerAsync(long serverId, long currentUserId);
        Task TransferOwnershipAsync(long serverId, TransferOwnershipRequest request, long currentUserId);
    }

    public class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;
        private readonly IServerRoleRepository _roleRepository;
        private readonly IServerMemberRepository _memberRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IServerPermissionService _permissionService;
        private readonly IServerEventPublisher _eventPublisher;
        private readonly ICurrentUserService _currentUserService;

        public ServerService(
            IServerRepository serverRepository,
            IServerRoleRepository roleRepository,
            IServerMemberRepository memberRepository,
            ICategoryRepository categoryRepository,
            IChannelRepository channelRepository,
            IServerPermissionService permissionService,
            IServerEventPublisher eventPublisher,
            ICurrentUserService currentUserService)
        {
            _serverRepository = serverRepository;
            _roleRepository = roleRepository;
            _memberRepository = memberRepository;
            _categoryRepository = categoryRepository;
            _channelRepository = channelRepository;
            _permissionService = permissionService;
            _eventPublisher = eventPublisher;
            _currentUserService = currentUserService;
        }

        public async Task<ServerDetailDto> CreateServerAsync(CreateServerRequest request, long currentUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Server name cannot be empty.");
            }

            if (request.IconFileId.HasValue)
            {
                var fileValid = await _serverRepository.FileExistsAndAccessibleAsync(request.IconFileId.Value, currentUserId);
                if (!fileValid)
                {
                    throw new BadRequestException("Icon file does not exist or you do not have permission to use it.");
                }
            }

            // Bắt đầu Transaction đảm bảo tính toàn vẹn 100%
            using var transaction = await _serverRepository.BeginTransactionAsync();
            try
            {
                // 1. Tạo Server
                var server = new Server
                {
                    Name = request.Name.Trim(),
                    OwnerId = currentUserId,
                    IconFileId = request.IconFileId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _serverRepository.AddAsync(server);
                await _serverRepository.SaveChangesAsync();

                // 2. Tạo System Role "Owner"
                var ownerRole = new ServerRole
                {
                    ServerId = server.Id,
                    Name = "Owner",
                    IsSystem = true,
                    Position = 0,
                    Color = "#FFD700",
                    CreatedAt = DateTime.UtcNow
                };
                await _roleRepository.AddAsync(ownerRole);

                // 3. Tạo System Role "Member"
                var memberRole = new ServerRole
                {
                    ServerId = server.Id,
                    Name = "Member",
                    IsSystem = true,
                    Position = 1,
                    Color = "#99AAB5",
                    CreatedAt = DateTime.UtcNow
                };
                await _roleRepository.AddAsync(memberRole);
                await _roleRepository.SaveChangesAsync();

                // 4. Thêm Creator vào ServerMembers với role Owner
                var serverMember = new ServerMember
                {
                    ServerId = server.Id,
                    UserId = currentUserId,
                    ServerRoleId = ownerRole.Id,
                    JoinedAt = DateTime.UtcNow
                };
                await _memberRepository.AddAsync(serverMember);

                // 5. Tạo Category mặc định "Text Channels"
                var defaultCategory = new Category
                {
                    ServerId = server.Id,
                    Name = "Text Channels",
                    Position = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _categoryRepository.AddAsync(defaultCategory);
                await _categoryRepository.SaveChangesAsync();

                // 6. Tạo Channel mặc định "general"
                var defaultChannel = new Channel
                {
                    ServerId = server.Id,
                    CategoryId = defaultCategory.Id,
                    Name = "general",
                    Type = 1, // 1: Text Channel
                    Position = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _channelRepository.AddAsync(defaultChannel);
                await _channelRepository.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ServerDetailDto
                {
                    Id = server.Id,
                    Name = server.Name,
                    OwnerId = server.OwnerId,
                    IconFileId = server.IconFileId,
                    IsSuspended = server.IsSuspended,
                    CreatedAt = server.CreatedAt,
                    UpdatedAt = server.UpdatedAt,
                    Categories = new List<CategoryResponseDto>
                    {
                        new CategoryResponseDto
                        {
                            Id = defaultCategory.Id,
                            ServerId = server.Id,
                            Name = defaultCategory.Name,
                            Position = defaultCategory.Position,
                            CreatedAt = defaultCategory.CreatedAt,
                            Channels = new List<ChannelResponseDto>
                            {
                                new ChannelResponseDto
                                {
                                    Id = defaultChannel.Id,
                                    ServerId = server.Id,
                                    CategoryId = defaultCategory.Id,
                                    Name = defaultChannel.Name,
                                    Type = defaultChannel.Type,
                                    Position = defaultChannel.Position,
                                    CreatedAt = defaultChannel.CreatedAt
                                }
                            }
                        }
                    },
                    Channels = new List<ChannelResponseDto>()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ServerSummaryDto>> GetAllServersAsync(bool includeDeleted = false)
        {
            var servers = await _serverRepository.GetAllServersAsync(includeDeleted);
            return servers.Select(s => new ServerSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                OwnerId = s.OwnerId,
                OwnerName = s.Owner != null ? (string.IsNullOrEmpty(s.Owner.Email) ? $"User #{s.Owner.Id}" : s.Owner.Email) : $"User #{s.OwnerId}",
                OwnerPublicCode = s.Owner?.PublicCode ?? string.Empty,
                IconFileId = s.IconFileId,
                MembersCount = s.ServerMembers?.Count(sm => sm.LeftAt == null) ?? 0,
                ChannelsCount = s.Channels?.Count(ch => ch.DeletedAt == null) ?? 0,
                StorageUsed = "0 MB",
                IsSuspended = s.IsSuspended,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                DeletedAt = s.DeletedAt,
                ScheduledDeleteAt = s.ScheduledDeleteAt
            }).ToList();
        }

        public async Task<List<ServerSidebarDto>> GetMyServersAsync(long currentUserId)
        {
            var servers = await _serverRepository.GetServersByUserIdAsync(currentUserId);
            return servers.Select(s => new ServerSidebarDto
            {
                Id = s.Id,
                Name = s.Name,
                IconUrl = s.IconFileId.HasValue ? $"/api/v1/files/{s.IconFileId.Value}" : null
            }).ToList();
        }

        public async Task<ServerDetailDto> GetServerDetailAsync(long serverId, long currentUserId)
        {
            var server = await _serverRepository.GetDetailAsync(serverId, includeDeleted: true);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            // Kiểm tra user là active member và không bị ban (Admin hệ thống được bypass)
            if (!_currentUserService.IsSystemAdmin())
            {
                await _permissionService.EnsureActiveMemberAsync(serverId, currentUserId);
            }

            return new ServerDetailDto
            {
                Id = server.Id,
                Name = server.Name,
                OwnerId = server.OwnerId,
                OwnerName = server.Owner != null ? (string.IsNullOrEmpty(server.Owner.Email) ? $"User #{server.Owner.Id}" : server.Owner.Email) : $"User #{server.OwnerId}",
                OwnerPublicCode = server.Owner?.PublicCode ?? string.Empty,
                IconFileId = server.IconFileId,
                MembersCount = server.ServerMembers?.Count(sm => sm.LeftAt == null) ?? 0,
                IsSuspended = server.IsSuspended,
                CreatedAt = server.CreatedAt,
                UpdatedAt = server.UpdatedAt,
                DeletedAt = server.DeletedAt,
                ScheduledDeleteAt = server.ScheduledDeleteAt,
                Categories = server.Categories.Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    ServerId = c.ServerId,
                    Name = c.Name,
                    Position = c.Position,
                    CreatedAt = c.CreatedAt,
                    Channels = c.Channels.Select(ch => new ChannelResponseDto
                    {
                        Id = ch.Id,
                        ServerId = ch.ServerId,
                        CategoryId = ch.CategoryId,
                        Name = ch.Name,
                        Type = ch.Type,
                        Position = ch.Position,
                        CreatedAt = ch.CreatedAt
                    }).ToList()
                }).ToList(),
                Channels = server.Channels.Where(ch => ch.CategoryId == null).Select(ch => new ChannelResponseDto
                {
                    Id = ch.Id,
                    ServerId = ch.ServerId,
                    CategoryId = null,
                    Name = ch.Name,
                    Type = ch.Type,
                    Position = ch.Position,
                    CreatedAt = ch.CreatedAt
                }).ToList()
            };
        }

        public async Task<ServerDetailDto> UpdateServerAsync(long serverId, UpdateServerRequest request, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            // Kiểm tra quyền: Owner HOẶC có permission MANAGE_SERVER (Admin hệ thống được bypass)
            if (!_currentUserService.IsSystemAdmin())
            {
                var isOwner = await _permissionService.IsOwnerAsync(serverId, currentUserId);
                if (!isOwner)
                {
                    var hasManageServer = await _permissionService.HasPermissionAsync(serverId, currentUserId, "MANAGE_SERVER");
                    if (!hasManageServer)
                    {
                        throw new ForbiddenException("You do not have permission to update this server (Requires Owner or MANAGE_SERVER).");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                server.Name = request.Name.Trim();
            }

            if (request.IconFileId.HasValue && request.IconFileId != server.IconFileId)
            {
                var fileValid = await _serverRepository.FileExistsAndAccessibleAsync(request.IconFileId.Value, currentUserId);
                if (!fileValid && !_currentUserService.IsSystemAdmin())
                {
                    throw new BadRequestException("Icon file does not exist or you do not have permission to use it.");
                }
                server.IconFileId = request.IconFileId.Value;
            }

            if (request.IsSuspended.HasValue)
            {
                server.IsSuspended = request.IsSuspended.Value;
            }

            server.UpdatedAt = DateTime.UtcNow;
            await _serverRepository.UpdateAsync(server);
            await _serverRepository.SaveChangesAsync();

            return await GetServerDetailAsync(serverId, currentUserId);
        }

        public async Task DeleteServerAsync(long serverId, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            // Chỉ Owner được phép xóa Server (Admin hệ thống được bypass)
            if (!_currentUserService.IsSystemAdmin())
            {
                await _permissionService.EnsureOwnerAsync(serverId, currentUserId);
            }

            // Lấy danh sách thành viên trước khi soft-delete để phát event
            var activeMembers = await _memberRepository.GetActiveMembersByServerIdAsync(serverId);
            var recipientIds = activeMembers.Select(m => m.UserId).ToList();

            // Soft-delete: Lên lịch xóa vĩnh viễn sau 14 ngày (14-day deletion schedule)
            server.DeletedAt = DateTime.UtcNow;
            server.ScheduledDeleteAt = DateTime.UtcNow.AddDays(14);
            server.UpdatedAt = DateTime.UtcNow;

            await _serverRepository.UpdateAsync(server);
            await _serverRepository.SaveChangesAsync();

            // Phát WebSocket event SERVER_DELETED
            await _eventPublisher.PublishServerDeletedAsync(serverId, recipientIds);
        }

        public async Task TransferOwnershipAsync(long serverId, TransferOwnershipRequest request, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            // 1. Phải là Owner hiện tại (Admin hệ thống được bypass)
            if (!_currentUserService.IsSystemAdmin())
            {
                await _permissionService.EnsureOwnerAsync(serverId, currentUserId);
            }

            if (request.NewOwnerId == currentUserId)
            {
                throw new BadRequestException("You are already the owner of this server.");
            }

            // 2. newOwner phải là member active và không bị ban
            var newOwnerMember = await _memberRepository.GetActiveMemberAsync(serverId, request.NewOwnerId);
            if (newOwnerMember == null || newOwnerMember.IsBanned)
            {
                throw new BadRequestException("New owner must be an active, non-banned member of this server.");
            }

            var currentOwnerMember = await _memberRepository.GetActiveMemberAsync(serverId, currentUserId);
            if (currentOwnerMember == null)
            {
                throw new BadRequestException("Current owner membership record not found.");
            }

            // Transaction
            using var transaction = await _serverRepository.BeginTransactionAsync();
            try
            {
                // Lấy role "Owner" và role "Member"
                var ownerRole = await _roleRepository.GetByNameAsync(serverId, "Owner");
                var memberRole = await _roleRepository.GetByNameAsync(serverId, "Member");

                // Cập nhật OwnerId của Server
                server.OwnerId = request.NewOwnerId;
                server.UpdatedAt = DateTime.UtcNow;
                await _serverRepository.UpdateAsync(server);

                // Gán role Owner cho New Owner
                if (ownerRole != null)
                {
                    newOwnerMember.ServerRoleId = ownerRole.Id;
                    await _memberRepository.UpdateAsync(newOwnerMember);
                }

                // Gỡ role Owner của Old Owner (chuyển sang Member role)
                if (memberRole != null)
                {
                    currentOwnerMember.ServerRoleId = memberRole.Id;
                    await _memberRepository.UpdateAsync(currentOwnerMember);
                }

                await _serverRepository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
