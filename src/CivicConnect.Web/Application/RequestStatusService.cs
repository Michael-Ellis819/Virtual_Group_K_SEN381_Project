using System.Security.Claims;
using CivicConnect.Security;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Application;

public sealed record StatusChange(Guid RequestId, string PreviousStatus, string NewStatus,
    string ActorId, string Feedback, long ExpectedVersion);

public interface IRequestStatusWriter
{
    Task SaveAsync(StatusChange change, CancellationToken cancellationToken);
}

public enum StatusChangeResult { Saved, NotFound, Invalid, Conflict }

// Coordinates required effects explicitly; persistence owns the transaction.
public sealed class RequestStatusService(IRequestReader reader, IRequestAccessPolicy access,
    StatusTransitionService transitions, IRequestStatusWriter writer)
{
    public async Task<StatusChangeResult> ChangeAsync(ClaimsPrincipal user, Guid id,
        string? expectedStatus, string? targetStatus, string? feedback,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty || user.Identity?.IsAuthenticated != true)
            return StatusChangeResult.NotFound;
        var request = await reader.FindAsync(id, cancellationToken);
        if (request is null || !access.CanChangeStatus(user, request))
            return StatusChangeResult.NotFound;
        if (string.IsNullOrWhiteSpace(feedback) || feedback.Length > 2000 ||
            string.IsNullOrWhiteSpace(expectedStatus) || string.IsNullOrWhiteSpace(targetStatus))
            return StatusChangeResult.Invalid;
        if (request.Status != expectedStatus) return StatusChangeResult.Conflict;
        if (!transitions.IsAllowed(request.Status, targetStatus)) return StatusChangeResult.Invalid;
        try
        {
            await writer.SaveAsync(new(id, request.Status, targetStatus,
                user.FindFirst(ClaimTypes.NameIdentifier)!.Value, feedback, request.Version), cancellationToken);
            return StatusChangeResult.Saved;
        }
        catch (DbUpdateConcurrencyException) { return StatusChangeResult.Conflict; }
    }
}
