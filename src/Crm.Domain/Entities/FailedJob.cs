using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class FailedJob : BaseEntity
{
    public required string JobId { get; set; }
    public required string JobType { get; set; }
    public required string PayloadJson { get; set; }
    public required string ExceptionDetails { get; set; }
    public DateTime FailedAt { get; set; }
    public int RetryCount { get; set; }
}
