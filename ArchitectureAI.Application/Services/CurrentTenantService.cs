using ArchitectureAI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace ArchitectureAI.Application.Services
{
    public class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private string? _tenantId;

        public string? TenantId
        {
            get
            {
                if (!string.IsNullOrEmpty(_tenantId))
                    return _tenantId;

                // 1. Try to get from Claims (Best for Auth)
                var claimTenant = _httpContextAccessor
                    .HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "tenant_id")
                    ?.Value;
                if (!string.IsNullOrEmpty(claimTenant))
                    return claimTenant;

                // 2. Try to get from Header (Best for flexibility/testing)
                if (
                    _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(
                        "X-Tenant-ID",
                        out var headerTenant
                    ) == true
                )
                {
                    return headerTenant.ToString();
                }

                return null; // Or throw if tenant is mandatory
            }
        }

        public void SetTenant(string tenantId)
        {
            _tenantId = tenantId;
        }
    }
}
