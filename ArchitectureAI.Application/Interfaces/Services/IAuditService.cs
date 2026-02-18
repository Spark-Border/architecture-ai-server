using ArchitectureAI.Domain.Audit;

namespace ArchitectureAI.Application.Interfaces.Services
{
    public interface IAuditService
    {
        ValueTask EnqueueAuditLogAsync(AuditTrail auditLog);
        ValueTask LogSecurityEventAsync(string action, string description, string tenantId, string userEmail, string type = "Security", string module = "System");
    }
}
