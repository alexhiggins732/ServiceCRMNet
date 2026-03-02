using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public required string Domain { get; set; }
}
