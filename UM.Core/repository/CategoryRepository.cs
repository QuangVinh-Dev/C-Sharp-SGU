using Microsoft.EntityFrameworkCore;
using UM.Core.config;
using UM.Core.entity;

namespace UM.Core.repository
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(long id, bool includeDeleted = false);
        Task<List<Category>> GetByServerIdAsync(long serverId);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task SaveChangesAsync();
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(long id, bool includeDeleted = false)
        {
            var query = _context.Categories.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(c => c.DeletedAt == null);
            }
            return await query
                .Include(c => c.Channels.Where(ch => ch.DeletedAt == null).OrderBy(ch => ch.Position))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetByServerIdAsync(long serverId)
        {
            return await _context.Categories
                .Where(c => c.ServerId == serverId && c.DeletedAt == null)
                .OrderBy(c => c.Position)
                .Include(c => c.Channels.Where(ch => ch.DeletedAt == null).OrderBy(ch => ch.Position))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
