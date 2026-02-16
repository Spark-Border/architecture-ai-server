using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Infrastructure.Services;
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
        // Change to Singleton as EncryptionService is stateless (key is immutable)
        services.AddSingleton<IEncryptionService, EncryptionService>();

        return services;
    }
}
