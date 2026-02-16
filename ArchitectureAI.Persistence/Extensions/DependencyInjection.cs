using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services; // Added missing namespace
using ArchitectureAI.Application.Services;
using ArchitectureAI.Persistence.Context;
using ArchitectureAI.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureAI.Persistence.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor(); // Required for TenantService
        services.AddScoped<ITenantService, CurrentTenantService>();
        
        services.AddSingleton<FirestoreDbContext>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(FirestoreRepository<>));
        
        return services;
    }
}
