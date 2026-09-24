using Microsoft.AspNetCore.Authorization;

namespace UM.Core.Security;

/// <summary>
/// Custom authorization requirement that checks for a specific system permission code.
/// Usage: [Authorize(Policy = "USER_READ")]
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
