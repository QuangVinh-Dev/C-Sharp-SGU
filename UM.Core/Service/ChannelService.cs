using UM.Core.DTOS.Request.Channel;
using UM.Core.DTOS.Response.Channel;
using UM.Core.Entities;
using UM.Core.Repository;

namespace UM.Core.Service;

public class ChannelService : IChannelService
{
    private readonly ChannelRepository _channelRepository;

    public ChannelService(ChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ChannelResponse> CreateChannelAsync(
        long categoryId,
        CreateChannelRequest request,
        long userId)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tên channel không được để trống.");
        }

        if (!Enum.IsDefined(typeof(ChannelType), request.Type))
        {
            throw new ArgumentException(
                "Loại channel không hợp lệ.");
        }

        var category = await _channelRepository
            .GetCategoryByIdAsync(categoryId);

        if (category == null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy category.");
        }

        var hasPermission = await _channelRepository
            .HasManageChannelsPermissionAsync(
                category.ServerId,
                userId);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                "Bạn không có quyền MANAGE_CHANNELS.");
        }

        var exists = await _channelRepository
            .ExistsByNameInCategoryAsync(categoryId, name);

        if (exists)
        {
            throw new InvalidOperationException(
                "Tên channel đã tồn tại trong category.");
        }

        var now = DateTime.UtcNow;

        var channel = new Channel
        {
            ServerId = category.ServerId,
            CategoryId = categoryId,
            Name = name,
            Type = request.Type,
            Position = await _channelRepository
                .GetMaxPositionAsync(categoryId) + 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _channelRepository.CreateAsync(channel);

        return new ChannelResponse
        {
            Id = channel.Id,
            ServerId = channel.ServerId,
            CategoryId = channel.CategoryId,
            Name = channel.Name,
            Type = channel.Type,
            Position = channel.Position,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt
        };
    }
}