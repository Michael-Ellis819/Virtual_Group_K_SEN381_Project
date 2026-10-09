namespace CivicConnect.Data.Entities;

public sealed class RequestNoteEntity
{
    public Guid NoteId { get; set; }

    public Guid RequestId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsRequesterVisible { get; set; }

    public ServiceRequestEntity ServiceRequest { get; set; } = null!;
}