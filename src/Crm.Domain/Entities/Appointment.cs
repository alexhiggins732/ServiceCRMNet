using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Appointment : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public Guid? JobId { get; set; }
    public required string Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
