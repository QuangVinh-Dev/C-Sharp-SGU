namespace BackendApi.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public byte[] TokenHash { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public DateTime CreatedAt { get; set; }
}
