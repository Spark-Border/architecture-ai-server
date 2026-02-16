using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Audit;
using ArchitectureAI.Persistence.Context;
using Microsoft.AspNetCore.Mvc.Filters;
using Google.Cloud.Firestore;

namespace ArchitectureAI.Api.Filters;

public class AuditLogActionFilter : IAsyncActionFilter
{
    private readonly FirestoreDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly ILogger<AuditLogActionFilter> _logger;

    public AuditLogActionFilter(FirestoreDbContext dbContext, ITenantService tenantService, ILogger<AuditLogActionFilter> logger)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the Action
        var resultContext = await next();

        try
        {
            // Gather contextual info
            var httpContext = context.HttpContext;
            var user = httpContext.User?.Identity?.Name ?? httpContext.User?.Claims.FirstOrDefault(c => c.Type == "user_id" || c.Type == "sub")?.Value ?? "Anonymous";
            var path = httpContext.Request.Path;
            var method = httpContext.Request.Method;
            var statusCode = resultContext.HttpContext.Response.StatusCode;
            var tenantId = _tenantService.TenantId ?? "No-Tenant";
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Create Audit Record
            var audit = new AuditTrail
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                ActionName = $"{method} {path}",
                ActionDescription = $"API Request: {method} {path} returned {statusCode}",
                Type = "Pipeline",
                Module = "API",
                LoggedInUser = user,
                CreatedBy = user,
                Origin = ip,
                ActionTime = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow
            };

            // Fire and forget (save to Firestore)
            var collection = _dbContext.Collection(nameof(AuditTrail));
            await collection.Document(audit.Id).SetAsync(audit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API Audit Trail.");
        }
    }
}
