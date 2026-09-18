using System.Security.Claims;
using UM.Core.config;

namespace UM.Core.sercurity
{
    /// <summary>
    /// Integration Point cho Authentication: Lấy ID của người dùng hiện tại
    /// Không tự implement JWT hay Login/Register (chờ module Authentication/JWT của thành viên khác merge).
    /// </summary>
    public interface ICurrentUserService
    {
        long? GetCurrentUserId();
        long GetRequiredUserId();
        bool IsSystemAdmin();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsSystemAdmin()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            // 1. Kiểm tra header X-Admin-Role từ UM.Admin client
            if (httpContext.Request.Headers.TryGetValue("X-Admin-Role", out var roleHeader))
            {
                var roleStr = roleHeader.ToString();
                if (roleStr == "SuperAdmin" || roleStr == "ServerAdmin")
                {
                    return true;
                }
            }

            // 2. Kiểm tra Role Claims nếu JWT đã cấu hình
            var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value
                         ?? httpContext.User.FindFirst("role")?.Value;

            return roleClaim == "SuperAdmin" || roleClaim == "ServerAdmin";
        }

        public long? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // 1. Kiểm tra JWT Claims nếu module JWT của thành viên khác đã hoạt động
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) 
                     ?? httpContext.User.FindFirst("sub")
                     ?? httpContext.User.FindFirst("id");

            if (claim != null && long.TryParse(claim.Value, out var userIdFromClaim))
            {
                return userIdFromClaim;
            }

            // 2. Integration Point / Test Header: Cho phép truyền qua Header 'X-User-Id' khi JWT chưa merge
            if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var headerValue) &&
                long.TryParse(headerValue, out var userIdFromHeader))
            {
                return userIdFromHeader;
            }

            return null;
        }

        public long GetRequiredUserId()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedException("User is not authenticated. Please provide valid credentials or X-User-Id header.");
            }
            return userId.Value;
        }
    }
}
