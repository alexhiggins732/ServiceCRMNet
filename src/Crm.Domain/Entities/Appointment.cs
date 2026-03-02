using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Appointment : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public Guid? JobId { get; set; }
    public required string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
