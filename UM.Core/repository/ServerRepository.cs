using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UM.Core.config;
using UM.Core.entity;

namespace UM.Core.repository
{
    public interface IServerRepository
    {
        Task<Server?> GetByIdAsync(long id, bool includeDeleted = false);
        Task<Server?> GetDetailAsync(long id, bool includeDeleted = false);
        Task<List<Server>> GetServersByUserIdAsync(long userId);
        Task<List<Server>> GetAllServersAsync(bool includeDeleted = false);
        Task AddAsync(Server server);
        Task UpdateAsync(Server server);
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<bool> FileExistsAndAccessibleAsync(long fileId, long userId);
    }

    public class ServerRepository : IServerRepository
    {
        private readonly AppDbContext _context;

        public ServerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Server?> GetByIdAsync(long id, bool includeDeleted = false)
        {
            var query = _context.Servers.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(s => s.DeletedAt == null);
            }
            return await query.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Server?> GetDetailAsync(long id, bool includeDeleted = false)
        {
            var query = _context.Servers.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(s => s.DeletedAt == null);
            }

            return await query
                .Where(s => s.Id == id)
                .Include(s => s.Owner)
                .Include(s => s.ServerMembers)
                .Include(s => s.Categories.Where(c => c.DeletedAt == null).OrderBy(c => c.Position))
                    .ThenInclude(c => c.Channels.Where(ch => ch.DeletedAt == null).OrderBy(ch => ch.Position))
                .Include(s => s.Channels.Where(ch => ch.CategoryId == null && ch.DeletedAt == null).OrderBy(ch => ch.Position))
                .FirstOrDefaultAsync();
        }

        public async Task<List<Server>> GetAllServersAsync(bool includeDeleted = false)
        {
            var query = _context.Servers
                .Include(s => s.Owner)
                .Include(s => s.ServerMembers)
                .Include(s => s.Channels.Where(ch => ch.DeletedAt == null))
                .AsNoTracking()
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(s => s.DeletedAt == null);
            }

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        public async Task<List<Server>> GetServersByUserIdAsync(long userId)
        {
            return await _context.ServerMembers
                .Where(sm => sm.UserId == userId && sm.LeftAt == null && !sm.IsBanned && sm.Server.DeletedAt == null)
                .Select(sm => sm.Server)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Server server)
        {
            await _context.Servers.AddAsync(server);
        }

        public Task UpdateAsync(Server server)
        {
            _context.Servers.Update(server);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<bool> FileExistsAndAccessibleAsync(long fileId, long userId)
        {
            return await _context.Files
                .AsNoTracking()
                .AnyAsync(f => f.Id == fileId && f.DeletedAt == null && f.OwnerId == userId);
        }
    }
}
