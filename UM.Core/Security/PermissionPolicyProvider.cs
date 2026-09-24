using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace UM.Core.Security;

/// <summary>
/// Dynamically creates authorization policies for any permission name.
/// When [Authorize(Policy = "USER_READ")] is used, this provider automatically
/// creates a policy with a PermissionRequirement("USER_READ").
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // If the policy name matches a known pattern (all caps with underscores),
        // treat it as a permission-based policy
        if (IsPermissionPolicy(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    private static bool IsPermissionPolicy(string policyName)
    {
        // Permission codes are uppercase with underscores, e.g., USER_READ, SYSTEM_ADMIN
        // This avoids conflicting with built-in ASP.NET Core policies
        return !string.IsNullOrEmpty(policyName)
               && policyName.All(c => char.IsUpper(c) || c == '_' || char.IsDigit(c));
    }
}
