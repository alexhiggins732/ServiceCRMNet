using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class AuditLog : TenantEntity
{
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? RequestId { get; set; }
    public string? ChangesJson { get; set; }
}
