using Microsoft.EntityFrameworkCore;
using UM.Core.config;
using UM.Core.entity;

namespace UM.Core.repository
{
    public interface IServerRoleRepository
    {
        Task<ServerRole?> GetByIdAsync(long id);
        Task<ServerRole?> GetByNameAsync(long serverId, string name);
        Task<List<ServerRole>> GetByServerIdAsync(long serverId);
        Task AddAsync(ServerRole role);
        Task UpdateAsync(ServerRole role);
        Task DeleteAsync(ServerRole role);
        Task SaveChangesAsync();
    }

    public class ServerRoleRepository : IServerRoleRepository
    {
        private readonly AppDbContext _context;

        public ServerRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServerRole?> GetByIdAsync(long id)
        {
            return await _context.ServerRoles
                .Include(sr => sr.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<ServerRole?> GetByNameAsync(long serverId, string name)
        {
            return await _context.ServerRoles
                .FirstOrDefaultAsync(sr => sr.ServerId == serverId && sr.Name == name);
        }

        public async Task<List<ServerRole>> GetByServerIdAsync(long serverId)
        {
            return await _context.ServerRoles
                .Where(sr => sr.ServerId == serverId)
                .OrderBy(sr => sr.Position)
                .Include(sr => sr.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(ServerRole role)
        {
            await _context.ServerRoles.AddAsync(role);
        }

        public Task UpdateAsync(ServerRole role)
        {
            _context.ServerRoles.Update(role);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ServerRole role)
        {
            _context.ServerRoles.Remove(role);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
