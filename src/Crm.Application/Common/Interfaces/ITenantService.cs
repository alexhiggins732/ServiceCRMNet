namespace Crm.Application.Common.Interfaces;

public interface ITenantService
{
    Guid? GetCurrentTenantId();
    void SetCurrentTenantId(Guid tenantId);
}
