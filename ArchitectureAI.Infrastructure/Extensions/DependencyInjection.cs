using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Application.Interfaces.Infrastructure;
using ArchitectureAI.Application.Services;
using ArchitectureAI.Infrastructure.Identity;
using ArchitectureAI.Infrastructure.Services;
using Finbuckle.MultiTenant;
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
            throw new InvalidOperationException(
                "Firebase Project ID is not configured (FIREBASE_PROJECT_ID)."
            );
        }

        services.AddSingleton<IEncryptionService, EncryptionService>();

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
        
        // Email Service
        services.AddScoped<IEmailService, BrevoEmailService>();

        services
            .AddMultiTenant<TenantInfo>()
            .WithStore<Tenancy.FirestoreMultiTenantStore>(ServiceLifetime.Scoped)
            .WithHeaderStrategy("X-Tenant-ID")
            .WithClaimStrategy("tenant_id")
            .WithStaticStrategy("system");

        return services;
    }
}
