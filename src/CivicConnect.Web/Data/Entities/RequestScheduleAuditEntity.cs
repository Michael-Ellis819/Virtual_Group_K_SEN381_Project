namespace CivicConnect.Data.Entities;

public sealed class RequestScheduleAuditEntity
{
    public Guid AuditId { get; set; }

    public Guid RequestId { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }

    public string PreviousPriority { get; set; } = string.Empty;

    public string NewPriority { get; set; } = string.Empty;

    public DateTimeOffset? PreviousDueAt { get; set; }

    public DateTimeOffset? NewDueAt { get; set; }

    public ServiceRequestEntity ServiceRequest { get; set; } = null!;
}