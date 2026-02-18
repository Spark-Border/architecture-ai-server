using ArchitectureAI.Application.Interfaces.Services;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace ArchitectureAI.Application.Services
{
    public class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private string? _manualTenantId;

        public string? TenantId
        {
            get
            {
                if (!string.IsNullOrEmpty(_manualTenantId))
                    return _manualTenantId;

                return _httpContextAccessor
                    .HttpContext?.GetMultiTenantContext<TenantInfo>()
                    ?.TenantInfo?.Id;
            }
        }

        public void SetTenant(string tenantId)
        {
            _manualTenantId = tenantId;
        }
    }
}
