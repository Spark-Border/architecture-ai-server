namespace ArchitectureAI.Application.Interfaces.Services;

public interface ITenantService
{
    string? TenantId { get; }
    void SetTenant(string tenantId);
}
