using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace ArchitectureAI.Infrastructure.Services;

public class TokenService : ITokenService
{
    public string GenerateJwtToken(
        ApplicationUser user,
        IList<string> roles,
        IList<string> permissions
    )
    {
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException(
                "Firebase Project ID is not configured (FIREBASE_PROJECT_ID)."
            );
        }

        var issuer = $"https://securetoken.google.com/{projectId}";
        var audience = projectId;

        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret is not configured (JWT_SECRET).");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("name", user.Name),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
