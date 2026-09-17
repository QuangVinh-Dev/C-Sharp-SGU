namespace UM.Core.websocket
{
    /// <summary>
    /// Integration Point cho WebSocket/STOMP:
    /// Phát sự kiện SERVER_DELETED hoặc các sự kiện realtime tới client.
    /// Không tự implement WebSocket server (chờ module WebSocket của thành viên khác merge).
    /// </summary>
    public interface IServerEventPublisher
    {
        Task PublishServerDeletedAsync(long serverId, IEnumerable<long> affectedUserIds);
    }

    public class ServerEventPublisher : IServerEventPublisher
    {
        private readonly ILogger<ServerEventPublisher> _logger;

        public ServerEventPublisher(ILogger<ServerEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishServerDeletedAsync(long serverId, IEnumerable<long> affectedUserIds)
        {
            var userList = string.Join(", ", affectedUserIds);
            _logger.LogInformation("[WEBSOCKET_EVENT] Event 'SERVER_DELETED' triggered for ServerId: {ServerId}. Notified members: [{UserList}]", 
                serverId, userList);

            // Integration Point: Khi module STOMP/WebSocket của đồng đội hoàn thành,
            // chỉ cần inject IHubContext hoặc STOMP client vào đây để gửi message ra frontend.
            return Task.CompletedTask;
        }
    }
}
