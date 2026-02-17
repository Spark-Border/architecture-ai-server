using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureAI.Application.Extensions
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
