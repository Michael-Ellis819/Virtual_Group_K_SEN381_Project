using System.Security.Claims;
using CivicConnect.Domain;
using CivicConnect.Security;
namespace CivicConnect.Application;
public interface IRequestReader
{
    Task<RequestSnapshot?> FindAsync(Guid id, CancellationToken cancellationToken);
}
public sealed record RequestDetailsDto(Guid RequestId, string ReferenceNumber, string CategoryId,
    string Description, string Location, string Status, DateTimeOffset UpdatedAt, IReadOnlyList<string> Notes);
public sealed class RequestQueryService(IRequestReader reader, IRequestAccessPolicy access)
{
    public async Task<RequestDetailsDto?> ReadAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty || user.Identity?.IsAuthenticated != true) return null;
        var request = await reader.FindAsync(id, cancellationToken);
        if (request is null) return null;
        var permission = access.EvaluateRead(user, request);
        if (!permission.Allowed) return null;
        // Project after permission checking. Never serialise persistence entities.
        return new(request.RequestId, request.ReferenceNumber, request.CategoryId, request.Description,
            request.Location, request.Status, request.UpdatedAt,
            request.Notes.Where(n => n.IsRequesterVisible || permission.IncludeInternalNotes).Select(n => n.Content).ToArray());
    }
}
