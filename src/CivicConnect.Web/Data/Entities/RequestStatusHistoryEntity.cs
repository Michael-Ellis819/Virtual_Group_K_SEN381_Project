namespace CivicConnect.Data.Entities;

public sealed class RequestStatusHistoryEntity
{
    public Guid HistoryId { get; set; }

    public Guid RequestId { get; set; }

    public string PreviousStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }

    public ServiceRequestEntity ServiceRequest { get; set; } = null!;
}