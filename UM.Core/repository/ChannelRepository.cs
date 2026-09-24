using LinqToDB;
using UM.Core.config;
using UM.Core.Entities;

namespace UM.Core.repository;

public class ChannelRepository
{
    private readonly DatabaseConnection _db;

    public ChannelRepository(DatabaseConnection db)
    {
        _db = db;
    }

    // Tao channel moi
    public async Task<Channel> CreateAsync(Channel channel)
    {
        channel.Id = await _db.InsertWithInt64IdentityAsync(channel);

        return channel;
    }

    // Lay danh sach channel trong category
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

    // Kiem tra ten channel da ton tai trong category
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

    // Lay position lon nhat trong category
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
}