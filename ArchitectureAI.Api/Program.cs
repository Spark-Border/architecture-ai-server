using System.Text;
using ArchitectureAI.Api.Middlewares;
using ArchitectureAI.Application.Extensions;
using ArchitectureAI.Application.Services;
using ArchitectureAI.Infrastructure.Data;
using ArchitectureAI.Infrastructure.Extensions;
using ArchitectureAI.Persistence.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ArchitectureAI.Api.Filters;
using NLog;
using NLog.Web;
using Finbuckle.MultiTenant;

// Early init of NLog to allow logging "during" startup
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Load .env file manually to avoid dependency issues
    var root = Directory.GetCurrentDirectory();
    
    var dotenvPath = Path.Combine(root, ".env");
    if (!File.Exists(dotenvPath))
    {
         // Try checking ArchitectureAI.Api subfolder if running from root
         var apiEnv = Path.Combine(root, "ArchitectureAI.Api", ".env");
         if (File.Exists(apiEnv))
         {
             dotenvPath = apiEnv;
         }
         else
         {
             var parent = Directory.GetParent(root)?.FullName;
             if (parent != null) dotenvPath = Path.Combine(parent, ".env");
         }
    }

    if (File.Exists(dotenvPath))
    {
        foreach (var line in File.ReadAllLines(dotenvPath))
        {
            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) continue;
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            // Remove quotes if present
            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value.Substring(1, value.Length - 2);
            }
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    // Ensure Environment Variables are loaded (defaults to true in CreateBuilder, but good to be explicit for hierarchy)
    builder.Configuration.AddEnvironmentVariables();

    // Add services to the container.
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<AuditLogActionFilter>();
    });

    // 1. Security & Performance Services
    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
                };
            
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                }
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

    // Register Global Garbage Collection / Cleanup Service
    builder.Services.AddHostedService<DataCleanupService>();

    // Health Checks for Cloud Run / K8s
    builder.Services.AddHealthChecks();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.CustomSchemaIds(x => x.FullName); // Avoid "Conflicting schemaIds" error

        // Add Security Definition
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        // Add Security Requirement
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    });

    var app = builder.Build();

    // Global Exception Handler - Must be first to catch exceptions from downstream middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage(); // Detailed errors in Dev
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArchitectureAI API v1"));
    }

    // Seed Data
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    app.UseHttpsRedirection();

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

    app.UseMultiTenant();
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
    logger.Error($"Application startup failed: {exception.Message}");
    logger.Error(exception.StackTrace!);
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
