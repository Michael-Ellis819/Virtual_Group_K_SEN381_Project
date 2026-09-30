using CivicConnect.Application;
using CivicConnect.Domain;
namespace CivicConnect.Data;
// Synthetic fixture. No durable storage or write guarantee.
public sealed class DevelopmentRequestReader : IRequestReader
{
    public static readonly Guid RequestId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Task<RequestSnapshot?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequestSnapshot? result = id == RequestId ? new(RequestId, "CC-DEMO-001", "requester-1",
            "IT Support", "Demo network fault", "Demo lab", "New", DateTimeOffset.Parse("2026-09-30T08:00:00Z"),
            new[] { new RequestNote("Request received.", true), new RequestNote("Private diagnostic note.", false) }) : null;
        return Task.FromResult(result);
    }
}
