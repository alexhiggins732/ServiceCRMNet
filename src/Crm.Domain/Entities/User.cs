using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class User : TenantEntity
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }
}
