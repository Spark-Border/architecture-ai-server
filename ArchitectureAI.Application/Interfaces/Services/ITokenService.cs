using ArchitectureAI.Domain.Users;

namespace ArchitectureAI.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
}
