using LinqToDB;
using UM.Core.Entities;
namespace UM.Core.Repository;

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

    public Task<bool> ExistsByNameInCategoryAsync(
        long categoryId,
        string name)
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
        // Schema hien tai khong co ServerMembers.ServerRoleId.
        // Quyen quan ly channel duoc xac dinh theo OwnerId cua Server.
        var isOwner = _db.GetTable<Server>()
            .Any(x =>
                x.Id == serverId &&
                x.OwnerId == userId &&
                x.DeletedAt == null &&
                x.IsSuspended == false);

        return Task.FromResult(isOwner);
    }
}