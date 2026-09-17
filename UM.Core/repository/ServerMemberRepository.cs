using Microsoft.EntityFrameworkCore;
using UM.Core.config;
using UM.Core.entity;

namespace UM.Core.repository
{
    public interface IServerMemberRepository
    {
        Task<ServerMember?> GetActiveMemberAsync(long serverId, long userId);
        Task<List<ServerMember>> GetActiveMembersByServerIdAsync(long serverId);
        Task AddAsync(ServerMember member);
        Task UpdateAsync(ServerMember member);
        Task SaveChangesAsync();
    }

    public class ServerMemberRepository : IServerMemberRepository
    {
        private readonly AppDbContext _context;

        public ServerMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServerMember?> GetActiveMemberAsync(long serverId, long userId)
        {
            return await _context.ServerMembers
                .Include(sm => sm.ServerRole)
                .FirstOrDefaultAsync(sm => sm.ServerId == serverId && sm.UserId == userId && sm.LeftAt == null);
        }

        public async Task<List<ServerMember>> GetActiveMembersByServerIdAsync(long serverId)
        {
            return await _context.ServerMembers
                .Where(sm => sm.ServerId == serverId && sm.LeftAt == null)
                .Include(sm => sm.ServerRole)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(ServerMember member)
        {
            await _context.ServerMembers.AddAsync(member);
        }

        public Task UpdateAsync(ServerMember member)
        {
            _context.ServerMembers.Update(member);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
