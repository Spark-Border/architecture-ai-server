using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Audit;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArchitectureAI.Api.Filters;

public class AuditLogActionFilter(
    IAuditService auditService,
    ITenantService tenantService,
    ILogger<AuditLogActionFilter> logger
) : IAsyncActionFilter
{
    private readonly IAuditService _auditService = auditService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly ILogger<AuditLogActionFilter> _logger = logger;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        // Execute the Action
        var resultContext = await next();

        try
        {
            // Gather contextual info
            var httpContext = context.HttpContext;
            var user =
                httpContext.User?.Identity?.Name
                ?? httpContext
                    .User?.Claims.FirstOrDefault(c => c.Type == "user_id" || c.Type == "sub")
                    ?.Value
                ?? "Anonymous";
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
                DateCreated = DateTime.UtcNow,
            };

            // Async Push
            await _auditService.EnqueueAuditLogAsync(audit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API Audit Trail.");
        }
    }
}
