using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class IdempotencyKey : BaseEntity
{
    public required string Key { get; set; }
    public required string RequestPath { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime ExpiresAt { get; set; }
}
