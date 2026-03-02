using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class AiConversation : TenantEntity
{
    public Guid UserId { get; set; }
    public required string Provider { get; set; }
    public required string Model { get; set; }
    public required string ContextJson { get; set; }
    public required string MessagesJson { get; set; }
}
