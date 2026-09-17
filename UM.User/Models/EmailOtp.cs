namespace BackendApi.Models;

public class EmailOtp
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public byte[] CodeHash { get; set; } = [];
    public string Purpose { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
