using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }
        
        // SuperAdmin can bypass? Could be added here.
        if (context.User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "SuperAdmin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
