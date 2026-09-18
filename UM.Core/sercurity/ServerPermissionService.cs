using Microsoft.EntityFrameworkCore;
using UM.Core.config;

namespace UM.Core.sercurity
{
    public interface IServerPermissionService
    {
        Task<bool> IsOwnerAsync(long serverId, long userId);
        Task<bool> IsActiveMemberAsync(long serverId, long userId);
        Task<bool> IsBannedAsync(long serverId, long userId);
        Task<bool> HasPermissionAsync(long serverId, long userId, string permissionCode);
        Task EnsureOwnerAsync(long serverId, long userId);
        Task EnsureActiveMemberAsync(long serverId, long userId);
        Task EnsurePermissionAsync(long serverId, long userId, string permissionCode);
    }

    public class ServerPermissionService : IServerPermissionService
    {
        private readonly AppDbContext _context;

        public ServerPermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsOwnerAsync(long serverId, long userId)
        {
            return await _context.Servers
                .AsNoTracking()
                .AnyAsync(s => s.Id == serverId && s.OwnerId == userId && s.DeletedAt == null);
        }

        public async Task<bool> IsActiveMemberAsync(long serverId, long userId)
        {
            return await _context.ServerMembers
                .AsNoTracking()
                .AnyAsync(sm => sm.ServerId == serverId 
                             && sm.UserId == userId 
                             && sm.LeftAt == null 
                             && !sm.IsBanned);
        }

        public async Task<bool> IsBannedAsync(long serverId, long userId)
        {
            return await _context.ServerMembers
                .AsNoTracking()
                .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId && sm.IsBanned);
        }

        public async Task<bool> HasPermissionAsync(long serverId, long userId, string permissionCode)
        {
            // 1. Owner luôn có toàn quyền (bypass checks)
            if (await IsOwnerAsync(serverId, userId))
            {
                return true;
            }

            // 2. Nếu bị ban hoặc không phải active member -> Không có quyền
            var member = await _context.ServerMembers
                .AsNoTracking()
                .Include(sm => sm.ServerRole)
                    .ThenInclude(sr => sr!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(sm => sm.ServerId == serverId 
                                        && sm.UserId == userId 
                                        && sm.LeftAt == null 
                                        && !sm.IsBanned);

            if (member == null || member.ServerRole == null)
            {
                return false;
            }

            // 3. Kiểm tra permission code được gán cho role
            return member.ServerRole.RolePermissions
                .Any(rp => rp.Permission != null && rp.Permission.Code.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));
        }

        public async Task EnsureOwnerAsync(long serverId, long userId)
        {
            var isOwner = await IsOwnerAsync(serverId, userId);
            if (!isOwner)
            {
                throw new ForbiddenException("Only the server owner can perform this action.");
            }
        }

        public async Task EnsureActiveMemberAsync(long serverId, long userId)
        {
            var isBanned = await IsBannedAsync(serverId, userId);
            if (isBanned)
            {
                throw new ForbiddenException("You have been banned from this server.");
            }

            var isMember = await IsActiveMemberAsync(serverId, userId);
            if (!isMember)
            {
                throw new ForbiddenException("You are not a member of this server.");
            }
        }

        public async Task EnsurePermissionAsync(long serverId, long userId, string permissionCode)
        {
            var hasPerm = await HasPermissionAsync(serverId, userId, permissionCode);
            if (!hasPerm)
            {
                throw new ForbiddenException($"You do not have the required permission '{permissionCode}' for this server.");
            }
        }
    }
}
