namespace CivicConnect.Domain;
// Read models only. Persistence entities and state transitions have separate owners.
public sealed record RequestNote(string Content, bool IsRequesterVisible);
public sealed record RequestSnapshot(Guid RequestId, string ReferenceNumber, string RequesterId,
    string CategoryId, string Description, string Location, string Status,
    DateTimeOffset UpdatedAt, IReadOnlyList<RequestNote> Notes);
