using LinqToDB;
using UM.Core.Entities;
using UM.Core.Repository;

namespace UM.Core.repository;

public class ChannelRepository
{
    private readonly DatabaseConnection _db;

    public ChannelRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task<Channel> CreateAsync(Channel channel)
    {
        channel.Id = await _db.InsertWithInt64IdentityAsync(channel);
        return channel;
    }

    public Task<List<Channel>> GetByCategoryIdAsync(long categoryId)
    {
        return Task.FromResult(
            _db.GetTable<Channel>()
                .Where(x =>
                    x.CategoryId == categoryId &&
                    x.DeletedAt == null)
                .OrderBy(x => x.Position)
                .ToList());
    }

    public Task<bool> ExistsByNameInCategoryAsync(long categoryId, string name)
    {
        return Task.FromResult(
            _db.GetTable<Channel>()
                .Any(x =>
                    x.CategoryId == categoryId &&
                    x.Name == name &&
                    x.DeletedAt == null));
    }

    public Task<int> GetMaxPositionAsync(long categoryId)
    {
        var maxPosition = _db.GetTable<Channel>()
            .Where(x =>
                x.CategoryId == categoryId &&
                x.DeletedAt == null)
            .Select(x => (int?)x.Position)
            .Max() ?? 0;

        return Task.FromResult(maxPosition);
    }

    public Task<Category?> GetCategoryByIdAsync(long categoryId)
    {
        return Task.FromResult(
            _db.GetTable<Category>()
                .FirstOrDefault(x =>
                    x.Id == categoryId &&
                    x.DeletedAt == null));
    }

    public Task<bool> HasManageChannelsPermissionAsync(
        long serverId,
        long userId)
    {
        var hasPermission =
            (from member in _db.GetTable<ServerMember>()
             join rolePermission in _db.GetTable<RolePermission>()
                 on member.ServerRoleId equals rolePermission.ServerRoleId
             join permission in _db.GetTable<Permission>()
                 on rolePermission.PermissionId equals permission.Id
             where member.ServerId == serverId
                   && member.UserId == userId
                   && member.ServerRoleId != null
                   && member.IsBanned == false
                   && member.LeftAt == null
                   && permission.Code == "MANAGE_CHANNELS"
             select permission.Id)
            .Any();

        return Task.FromResult(hasPermission);
    }
}