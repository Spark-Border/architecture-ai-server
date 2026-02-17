using ArchitectureAI.Domain.Audit;

namespace ArchitectureAI.Application.Interfaces.Services
{
    public interface IAuditService
    {
        ValueTask EnqueueAuditLogAsync(AuditTrail auditLog);
    }
}
