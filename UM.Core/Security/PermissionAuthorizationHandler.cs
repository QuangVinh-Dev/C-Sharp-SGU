using Microsoft.AspNetCore.Authorization;

namespace UM.Core.Security;

/// <summary>
/// Authorization handler that checks if the current user has the required
/// system permission claim in their JWT token.
/// Permission claims are added during token generation from SystemRolePermissions.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if the user has the required permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
