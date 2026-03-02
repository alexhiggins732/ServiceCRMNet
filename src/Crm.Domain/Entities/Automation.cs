using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Automation : TenantEntity
{
    public required string Name { get; set; }
    public bool IsEnabled { get; set; }
    public required string TriggerType { get; set; }
    public required string TriggerConfigJson { get; set; }
    public required string ActionType { get; set; }
    public required string ActionConfigJson { get; set; }
}
