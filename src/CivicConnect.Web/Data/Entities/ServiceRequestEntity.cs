namespace CivicConnect.Data.Entities;

public sealed class ServiceRequestEntity
{
    public Guid RequestId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string RequesterId { get; set; } = string.Empty;

    public string CategoryId { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = "New";

    public DateTimeOffset UpdatedAt { get; set; }

    // Used to detect competing updates to the same request.
    public long Version { get; set; } = 1;
    
    public DateTimeOffset CreatedAt { get; set; }

    public string Priority { get; set; } = "Normal";

    public string? OwnerId { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    public Guid? SubmissionKey { get; set; }

    public List<RequestNoteEntity> Notes { get; set; } = new();
}