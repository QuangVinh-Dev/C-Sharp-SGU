using Microsoft.EntityFrameworkCore;
using UM.Core.config;
using UM.Core.entity;

namespace UM.Core.repository
{
    public interface IChannelRepository
    {
        Task<Channel?> GetByIdAsync(long id, bool includeDeleted = false);
        Task<List<Channel>> GetByServerIdAsync(long serverId);
        Task<List<Channel>> GetByCategoryIdAsync(long categoryId);
        Task AddAsync(Channel channel);
        Task UpdateAsync(Channel channel);
        Task SaveChangesAsync();
    }

    public class ChannelRepository : IChannelRepository
    {
        private readonly AppDbContext _context;

        public ChannelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Channel?> GetByIdAsync(long id, bool includeDeleted = false)
        {
            var query = _context.Channels.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(ch => ch.DeletedAt == null);
            }
            return await query.FirstOrDefaultAsync(ch => ch.Id == id);
        }

        public async Task<List<Channel>> GetByServerIdAsync(long serverId)
        {
            return await _context.Channels
                .Where(ch => ch.ServerId == serverId && ch.DeletedAt == null)
                .OrderBy(ch => ch.Position)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Channel>> GetByCategoryIdAsync(long categoryId)
        {
            return await _context.Channels
                .Where(ch => ch.CategoryId == categoryId && ch.DeletedAt == null)
                .OrderBy(ch => ch.Position)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Channel channel)
        {
            await _context.Channels.AddAsync(channel);
        }

        public Task UpdateAsync(Channel channel)
        {
            _context.Channels.Update(channel);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
