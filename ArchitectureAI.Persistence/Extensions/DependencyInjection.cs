using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Application.Services;
using ArchitectureAI.Persistence.Context;
using ArchitectureAI.Persistence.Repositories;
using ArchitectureAI.Persistence.Services; // Ensure this namespace is correct for AuditBatchWorker
using Microsoft.AspNetCore.Http; // Required for IHttpContextAccessor
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Required for IHostedService

namespace ArchitectureAI.Persistence.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor(); // Required for TenantService & Auditing
        services.AddScoped<ITenantService, CurrentTenantService>();

        // 1. Register Firestore Context (Singleton is best for FirestoreDb wrapper)
        services.AddSingleton<FirestoreDbContext>();

        // 2. Register Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(FirestoreRepository<>));
        
        // 3. Register Asynchronous Audit Worker
        // Register the worker class itself as a Singleton
        services.AddSingleton<AuditBatchWorker>();
        
        // Bind IAuditService interface to the SAME Singleton instance
        services.AddSingleton<IAuditService>(provider => provider.GetRequiredService<AuditBatchWorker>());
        
        // Register as a Hosted Service (Background Worker) using the SAME instance
        services.AddHostedService(provider => provider.GetRequiredService<AuditBatchWorker>());

        return services;
    }
}
