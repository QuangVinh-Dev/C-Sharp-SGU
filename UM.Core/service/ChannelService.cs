using UM.Core.config;
using UM.Core.dto;
using UM.Core.entity;
using UM.Core.repository;
using UM.Core.sercurity;

namespace UM.Core.service
{
    public interface IChannelService
    {
        Task<ChannelResponseDto> CreateChannelAsync(long serverId, CreateChannelRequest request, long currentUserId);
        Task<List<ChannelResponseDto>> GetChannelsByServerAsync(long serverId, long currentUserId);
        Task<ChannelResponseDto> UpdateChannelAsync(long channelId, UpdateChannelRequest request, long currentUserId);
        Task DeleteChannelAsync(long channelId, long currentUserId);
    }

    public class ChannelService : IChannelService
    {
        private readonly IChannelRepository _channelRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IServerRepository _serverRepository;
        private readonly IServerPermissionService _permissionService;
        private readonly ICurrentUserService _currentUserService;

        public ChannelService(
            IChannelRepository channelRepository,
            ICategoryRepository categoryRepository,
            IServerRepository serverRepository,
            IServerPermissionService permissionService,
            ICurrentUserService currentUserService)
        {
            _channelRepository = channelRepository;
            _categoryRepository = categoryRepository;
            _serverRepository = serverRepository;
            _permissionService = permissionService;
            _currentUserService = currentUserService;
        }

        public async Task<ChannelResponseDto> CreateChannelAsync(long serverId, CreateChannelRequest request, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(serverId, currentUserId, "MANAGE_CHANNELS")
                           || await _permissionService.HasPermissionAsync(serverId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to manage channels in this server.");
                }
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Channel name cannot be empty.");
            }

            // Nếu có CategoryId: Đảm bảo Category tồn tại VÀ THUỘC ĐÚNG SERVER NÀY!
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                {
                    throw new NotFoundException($"Category with ID {request.CategoryId.Value} was not found.");
                }
                if (category.ServerId != serverId)
                {
                    throw new BadRequestException($"Category with ID {request.CategoryId.Value} does not belong to Server {serverId}. Cannot cross-link categories across servers!");
                }
            }

            var channel = new Channel
            {
                ServerId = serverId,
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Type = request.Type,
                Position = request.Position,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _channelRepository.AddAsync(channel);
            await _channelRepository.SaveChangesAsync();

            return new ChannelResponseDto
            {
                Id = channel.Id,
                ServerId = channel.ServerId,
                CategoryId = channel.CategoryId,
                Name = channel.Name,
                Type = channel.Type,
                Position = channel.Position,
                CreatedAt = channel.CreatedAt
            };
        }

        public async Task<List<ChannelResponseDto>> GetChannelsByServerAsync(long serverId, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                await _permissionService.EnsureActiveMemberAsync(serverId, currentUserId);
            }

            var channels = await _channelRepository.GetByServerIdAsync(serverId);

            return channels.Select(ch => new ChannelResponseDto
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

        public async Task<ChannelResponseDto> UpdateChannelAsync(long channelId, UpdateChannelRequest request, long currentUserId)
        {
            var channel = await _channelRepository.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new NotFoundException($"Channel with ID {channelId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(channel.ServerId, currentUserId, "MANAGE_CHANNELS")
                           || await _permissionService.HasPermissionAsync(channel.ServerId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to edit channels in this server.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                channel.Name = request.Name.Trim();
            }

            if (request.CategoryId.HasValue)
            {
                // Kiểm tra Category mới có thuộc cùng Server không
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                {
                    throw new NotFoundException($"Category with ID {request.CategoryId.Value} was not found.");
                }
                if (category.ServerId != channel.ServerId)
                {
                    throw new BadRequestException($"Category {request.CategoryId.Value} does not belong to the same Server as Channel {channelId}.");
                }
                channel.CategoryId = request.CategoryId.Value;
            }

            if (request.Position.HasValue)
            {
                channel.Position = request.Position.Value;
            }

            channel.UpdatedAt = DateTime.UtcNow;
            await _channelRepository.UpdateAsync(channel);
            await _channelRepository.SaveChangesAsync();

            return new ChannelResponseDto
            {
                Id = channel.Id,
                ServerId = channel.ServerId,
                CategoryId = channel.CategoryId,
                Name = channel.Name,
                Type = channel.Type,
                Position = channel.Position,
                CreatedAt = channel.CreatedAt
            };
        }

        public async Task DeleteChannelAsync(long channelId, long currentUserId)
        {
            var channel = await _channelRepository.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new NotFoundException($"Channel with ID {channelId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(channel.ServerId, currentUserId, "MANAGE_CHANNELS")
                           || await _permissionService.HasPermissionAsync(channel.ServerId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to delete channels in this server.");
                }
            }

            // Soft-delete channel
            channel.DeletedAt = DateTime.UtcNow;
            channel.UpdatedAt = DateTime.UtcNow;

            await _channelRepository.UpdateAsync(channel);
            await _channelRepository.SaveChangesAsync();
        }
    }
}
