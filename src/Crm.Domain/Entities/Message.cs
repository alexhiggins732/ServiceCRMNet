using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

public class Message : TenantEntity
{
    public MessageChannel Channel { get; set; }
    public MessageDirection Direction { get; set; }
    public required string ExternalId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public required string Body { get; set; }
    public Uri? MediaUrl { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
