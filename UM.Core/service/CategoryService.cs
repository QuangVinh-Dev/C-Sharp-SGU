using UM.Core.config;
using UM.Core.dto;
using UM.Core.entity;
using UM.Core.repository;
using UM.Core.sercurity;

namespace UM.Core.service
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateCategoryAsync(long serverId, CreateCategoryRequest request, long currentUserId);
        Task<List<CategoryResponseDto>> GetCategoriesByServerAsync(long serverId, long currentUserId);
        Task<CategoryResponseDto> UpdateCategoryAsync(long categoryId, UpdateCategoryRequest request, long currentUserId);
        Task DeleteCategoryAsync(long categoryId, long currentUserId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IServerRepository _serverRepository;
        private readonly IServerPermissionService _permissionService;
        private readonly ICurrentUserService _currentUserService;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IServerRepository serverRepository,
            IServerPermissionService permissionService,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _serverRepository = serverRepository;
            _permissionService = permissionService;
            _currentUserService = currentUserService;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(long serverId, CreateCategoryRequest request, long currentUserId)
        {
            var server = await _serverRepository.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new NotFoundException($"Server with ID {serverId} was not found.");
            }

            // Kiểm tra quyền MANAGE_CATEGORIES hoặc MANAGE_SERVER hoặc Owner (Admin hệ thống được bypass)
            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(serverId, currentUserId, "MANAGE_CATEGORIES")
                           || await _permissionService.HasPermissionAsync(serverId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to manage categories in this server.");
                }
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Category name cannot be empty.");
            }

            var category = new Category
            {
                ServerId = serverId,
                Name = request.Name.Trim(),
                Position = request.Position,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                ServerId = category.ServerId,
                Name = category.Name,
                Position = category.Position,
                CreatedAt = category.CreatedAt,
                Channels = new List<ChannelResponseDto>()
            };
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByServerAsync(long serverId, long currentUserId)
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

            var categories = await _categoryRepository.GetByServerIdAsync(serverId);

            return categories.Select(c => new CategoryResponseDto
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
            }).ToList();
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(long categoryId, UpdateCategoryRequest request, long currentUserId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(category.ServerId, currentUserId, "MANAGE_CATEGORIES")
                           || await _permissionService.HasPermissionAsync(category.ServerId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to edit categories in this server.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                category.Name = request.Name.Trim();
            }

            if (request.Position.HasValue)
            {
                category.Position = request.Position.Value;
            }

            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                ServerId = category.ServerId,
                Name = category.Name,
                Position = category.Position,
                CreatedAt = category.CreatedAt,
                Channels = category.Channels.Select(ch => new ChannelResponseDto
                {
                    Id = ch.Id,
                    ServerId = ch.ServerId,
                    CategoryId = ch.CategoryId,
                    Name = ch.Name,
                    Type = ch.Type,
                    Position = ch.Position,
                    CreatedAt = ch.CreatedAt
                }).ToList()
            };
        }

        public async Task DeleteCategoryAsync(long categoryId, long currentUserId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} was not found.");
            }

            if (!_currentUserService.IsSystemAdmin())
            {
                var hasPerm = await _permissionService.HasPermissionAsync(category.ServerId, currentUserId, "MANAGE_CATEGORIES")
                           || await _permissionService.HasPermissionAsync(category.ServerId, currentUserId, "MANAGE_SERVER");
                if (!hasPerm)
                {
                    throw new ForbiddenException("You do not have permission to delete categories in this server.");
                }
            }

            // Soft delete category & các channel con
            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            foreach (var channel in category.Channels)
            {
                channel.DeletedAt = DateTime.UtcNow;
                channel.UpdatedAt = DateTime.UtcNow;
            }

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }
}
