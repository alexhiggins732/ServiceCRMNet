using Crm.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Crm.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Role : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
}
