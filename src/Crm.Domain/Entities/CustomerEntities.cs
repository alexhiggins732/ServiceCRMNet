using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Customer : TenantEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public class Lead : TenantEntity
{
    public required string Name { get; set; }
    public string? ContactInfo { get; set; }
    public required string Source { get; set; }
    public string? Status { get; set; }
    public Guid? CustomerId { get; set; }
}

public class Job : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string? Notes { get; set; }
}
