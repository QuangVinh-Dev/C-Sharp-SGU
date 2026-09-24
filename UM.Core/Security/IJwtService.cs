using UM.Core.Entities;

namespace UM.Core.Security;

public interface IJwtService
{
    string GenerateAccessToken(User user, List<string> roles, List<string> permissions);

    string GenerateRefreshToken();

    byte[] HashRefreshToken(string token);
}
