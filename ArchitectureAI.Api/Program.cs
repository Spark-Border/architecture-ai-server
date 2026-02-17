using ArchitectureAI.Api.Middlewares;
using ArchitectureAI.Application.Extensions;
using ArchitectureAI.Application.Services; // Updated namespace
using ArchitectureAI.Infrastructure.Data;
using ArchitectureAI.Infrastructure.Extensions;
using ArchitectureAI.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Web;

// Early init of NLog to allow logging "during" startup
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Ensure Environment Variables are loaded (defaults to true in CreateBuilder, but good to be explicit for hierarchy)
    builder.Configuration.AddEnvironmentVariables();

    // Add services to the container.
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ArchitectureAI.Api.Filters.AuditLogActionFilter>();
    });

    // 1. Security & Performance Services
    builder
        .Services.AddAuthentication()
        .AddJwtBearer(options =>
        {
            var projectId =
                Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
                ?? builder.Configuration["Firebase:ProjectId"];
            options.Authority = $"https://securetoken.google.com/{projectId}";
            options.TokenValidationParameters =
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true,
                };
        });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(
            "fixed",
            limiterOptions =>
            {
                limiterOptions.PermitLimit = 1000;
                limiterOptions.Window = TimeSpan.FromSeconds(10);
                limiterOptions.QueueProcessingOrder = System
                    .Threading
                    .RateLimiting
                    .QueueProcessingOrder
                    .OldestFirst;
                limiterOptions.QueueLimit = 50;
            }
        );
    });

    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(60)));
    });

    // Add Standard Resilience Pipeline for any future HTTP Clients
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler();
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    // Register Global Garbage Collection / Cleanup Service
    builder.Services.AddHostedService<ArchitectureAI.Application.Services.DataCleanupService>();

    // Health Checks for Cloud Run / K8s
    builder.Services.AddHealthChecks();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Seed Data
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    app.UseHttpsRedirection();

    // Global Exception Handler
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Security Headers (Simple manual implementation for now, or use library)
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            await next();
        }
    );

    app.UseResponseCompression();
    app.UseRateLimiter();
    app.UseOutputCache();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers().RequireRateLimiting("fixed");
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
