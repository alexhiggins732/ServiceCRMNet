using Crm.Application.Common.Interfaces;

namespace Crm.Infrastructure.Tenancy;

public class TenantService : ITenantService
{
    private Guid? _currentTenantId;

    public Guid? GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _currentTenantId = tenantId;
    }
}
