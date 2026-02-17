using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Application.Services;
using ArchitectureAI.Infrastructure.Identity;
using ArchitectureAI.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureAI.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        if (string.IsNullOrEmpty(projectId))
        {
             // Fallback to config for backward compatibility, but ideally should be Env Var
             projectId = configuration["Firebase:ProjectId"];
             
             if (string.IsNullOrEmpty(projectId))
             {
                 throw new InvalidOperationException("Firebase Project ID is not configured (FIREBASE_PROJECT_ID).");
             }
        }

        // Change to Singleton as EncryptionService is stateless (key is immutable)
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Identity Configuration
        services
            .AddIdentity<
                ArchitectureAI.Domain.Users.ApplicationUser,
                ArchitectureAI.Domain.Users.ApplicationRole
            >(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddUserStore<FirestoreUserStore>()
            .AddRoleStore<FirestoreRoleStore>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITenantService, CurrentTenantService>();
        services.AddScoped<ITokenService, TokenService>();

        // RBAC: Register Authorization Policies dynamically or statically
        services.AddAuthorization(options =>
        {
            foreach (var permission in ArchitectureAI.Application.Constants.Permissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy => policy.RequireClaim("permission", permission)
                );
            }
        });

        services.AddScoped<ArchitectureAI.Infrastructure.Data.DataSeeder>();

        return services;
    }
}
