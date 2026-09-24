using LinqToDB;
using LinqToDB.Async;
using UM.Core.Entities;

namespace UM.Core.Repository;

public interface IAuthRepository
{
    Task<bool> IsEmailTakenAsync(string email);

    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByIdAsync(long userId);

    Task RegisterUserTransactionAsync(
        User user,
        UserProfile profile,
        EmailOtp otp);

    Task<EmailOtp?> GetLatestOtpAsync(
        string email,
        string purpose);

    Task UpdateOtpAsync(EmailOtp otp);

    Task SaveEmailOtpAsync(EmailOtp otp);

    Task<RefreshToken?> GetRefreshTokenAsync(
        byte[] tokenHash);

    Task AddRefreshTokenAsync(
        RefreshToken token);

    Task UpdateRefreshTokenAsync(
        RefreshToken token);

    Task RevokeAllUserRefreshTokensAsync(
        long userId, string? ipAddress = null);

    Task UpdateUserAsync(User user);

    // System Admin / Permissions
    Task<List<string>> GetUserSystemRolesAsync(long userId);

    Task<List<string>> GetUserSystemPermissionsAsync(long userId);

    // Audit
    Task AddAuditLogAsync(AuditLog auditLog);

    Task<bool> TryConsumeEmailOtpAsync(long otpId, DateTime usedAt);
    Task<bool> TryConsumeRefreshTokenAsync(Guid tokenId, DateTime revokedAt, string? revokedByIp, Guid replacedByTokenId);
    Task IncrementFailedLoginCountAsync(long userId, int maxAttempts, DateTime lockedUntilTime, DateTime updatedAt);
    Task<bool> ResetPasswordTransactionAsync(long userId, string passwordHash, long otpId, DateTime usedAt, DateTime updatedAt);
}

public class AuthRepository : IAuthRepository
{
    private readonly DatabaseConnection _db;

    public AuthRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _db
            .GetTable<User>()
            .AnyAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db
            .GetTable<User>()
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        return await _db
            .GetTable<User>()
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _db.UpdateAsync(user);
    }

    public async Task<EmailOtp?> GetLatestOtpAsync(
        string email,
        string purpose)
    {
        // Use LINQ join instead of LoadWith to avoid navigation property issues
        return await (
            from otp in _db.GetTable<EmailOtp>()
            join user in _db.GetTable<User>()
                on otp.UserId equals user.Id
            where user.Email == email
                  && otp.Purpose == purpose
            orderby otp.CreatedAt descending
            select otp
        ).FirstOrDefaultAsync();
    }

    public async Task SaveEmailOtpAsync(EmailOtp otp)
    {
        await _db.InsertAsync(otp);
    }

    public async Task UpdateOtpAsync(EmailOtp otp)
    {
        await _db.UpdateAsync(otp);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(
        byte[] tokenHash)
    {
        return await _db
            .GetTable<RefreshToken>()
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash);
    }

    public async Task AddRefreshTokenAsync(
        RefreshToken token)
    {
        await _db.InsertAsync(token);
    }

    public async Task UpdateRefreshTokenAsync(
        RefreshToken token)
    {
        await _db.UpdateAsync(token);
    }

    public async Task RevokeAllUserRefreshTokensAsync(
        long userId, string? ipAddress = null)
    {
        var now = DateTime.UtcNow;

        await _db.GetTable<RefreshToken>()
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .Set(x => x.RevokedAt, now)
            .Set(x => x.RevokedByIp, ipAddress)
            .UpdateAsync();
    }

    public async Task RegisterUserTransactionAsync(
        User user,
        UserProfile profile,
        EmailOtp otp)
    {
        await using var transaction =
            await _db.BeginTransactionAsync();

        try
        {
            // InsertWithInt64IdentityAsync returns the generated Id
            user.Id = await _db.InsertWithInt64IdentityAsync(user);

            profile.UserId = user.Id;
            otp.UserId = user.Id;

            await _db.InsertAsync(profile);
            await _db.InsertAsync(otp);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ─── System Admin / Permissions ───────────────────────────────

    public async Task<List<string>> GetUserSystemRolesAsync(long userId)
    {
        return await (
            from sa in _db.GetTable<SystemAdmin>()
            join sr in _db.GetTable<SystemRole>()
                on sa.SystemRoleId equals sr.Id
            where sa.UserId == userId
                  && sa.RevokedAt == null
            select sr.Name
        ).ToListAsync();
    }

    public async Task<List<string>> GetUserSystemPermissionsAsync(long userId)
    {
        return await (
            from sa in _db.GetTable<SystemAdmin>()
            join srp in _db.GetTable<SystemRolePermission>()
                on sa.SystemRoleId equals srp.SystemRoleId
            join sp in _db.GetTable<SystemPermission>()
                on srp.PermissionId equals sp.Id
            where sa.UserId == userId
                  && sa.RevokedAt == null
            select sp.Code
        ).Distinct().ToListAsync();
    }

    // ─── Audit ────────────────────────────────────────────────────

    public async Task AddAuditLogAsync(AuditLog auditLog)
    {
        await _db.InsertAsync(auditLog);
    }

    public async Task<bool> TryConsumeEmailOtpAsync(long otpId, DateTime usedAt)
    {
        var updated = await _db.GetTable<EmailOtp>()
            .Where(x => x.Id == otpId && x.UsedAt == null)
            .Set(x => x.UsedAt, usedAt)
            .UpdateAsync();
        return updated > 0;
    }

    public async Task<bool> TryConsumeRefreshTokenAsync(Guid tokenId, DateTime revokedAt, string? revokedByIp, Guid replacedByTokenId)
    {
        var updated = await _db.GetTable<RefreshToken>()
            .Where(x => x.Id == tokenId && x.RevokedAt == null)
            .Set(x => x.RevokedAt, revokedAt)
            .Set(x => x.RevokedByIp, revokedByIp)
            .Set(x => x.ReplacedByTokenId, replacedByTokenId)
            .UpdateAsync();
        return updated > 0;
    }

    public async Task IncrementFailedLoginCountAsync(long userId, int maxAttempts, DateTime lockedUntilTime, DateTime updatedAt)
    {
        await _db.GetTable<User>()
            .Where(x => x.Id == userId)
            .Set(x => x.FailedLoginCount, x => x.FailedLoginCount + 1)
            .Set(x => x.LockedUntil, x => (x.FailedLoginCount + 1) >= maxAttempts ? lockedUntilTime : x.LockedUntil)
            .Set(x => x.UpdatedAt, updatedAt)
            .UpdateAsync();
    }

    public async Task<bool> ResetPasswordTransactionAsync(long userId, string passwordHash, long otpId, DateTime usedAt, DateTime updatedAt)
    {
        await using var transaction = await _db.BeginTransactionAsync();
        try
        {
            var otpUpdated = await _db.GetTable<EmailOtp>()
                .Where(x => x.Id == otpId && x.UsedAt == null)
                .Set(x => x.UsedAt, usedAt)
                .UpdateAsync();

            if (otpUpdated == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await _db.GetTable<User>()
                .Where(x => x.Id == userId)
                .Set(x => x.PasswordHash, passwordHash)
                .Set(x => x.UpdatedAt, updatedAt)
                .UpdateAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}