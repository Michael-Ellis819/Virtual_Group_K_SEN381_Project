using System.Security.Claims;
using CivicConnect.Lifecycle;

namespace CivicConnect.Application;

public sealed record PortalActor(string Id, string Role, IReadOnlyCollection<string> Categories)
{
    public static PortalActor? From(ClaimsPrincipal user)
    {
        var identities = user.Identities.ToArray();
        if (identities.Length != 1 || !identities[0].IsAuthenticated) return null;
        var ids = user.FindAll(ClaimTypes.NameIdentifier).ToArray();
        var roles = user.FindAll(ClaimTypes.Role).ToArray();
        var active = user.FindAll("active").ToArray();
        if (ids.Length != 1 || string.IsNullOrWhiteSpace(ids[0].Value) || ids[0].Value.Length > 100 ||
            roles.Length != 1 || roles[0].Value is not ("Requester" or "Staff" or "Management") ||
            active.Length != 1 || active[0].Value != "true") return null;
        return new(ids[0].Value, roles[0].Value, user.FindAll("category").Select(c => c.Value).Distinct().ToArray());
    }
}

public sealed record PortalAccount(string Id, string DisplayName, string Role, bool IsActive,
    IReadOnlyCollection<string> Categories, string SecurityStamp);
public enum SignInOutcome { Authenticated, InvalidCredentials, Unavailable }
public sealed record SignInResult(SignInOutcome Outcome, PortalAccount? Account = null);

// Michael's account adapter must validate hashed credentials and server-owned permissions.
// No application/demo account store is supplied by the presentation layer.
public interface IPortalAccounts
{
    Task<SignInResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
    Task<PortalAccount?> FindAsync(string id, CancellationToken cancellationToken);
}

public sealed record PortalFilter(string? Search, string? Status, string? Category,
    DateTimeOffset? From = null, DateTimeOffset? To = null, string? Priority = null, string Sort = "date-desc");
public sealed record PortalRow(Guid Id, string Reference, string Category, DateTimeOffset CreatedAt,
    string Status, string Priority, string? OwnerId, DateTimeOffset? DueAt);
public sealed record PortalMetadata(string? Requester, string Priority, string? OwnerId, DateTimeOffset? DueAt,
    IReadOnlyList<PortalNote> Notes, IReadOnlyList<PortalHistory> History);
public sealed record PortalNote(string Content, string Author, DateTimeOffset At, bool IsPublic);
public sealed record PortalHistory(string Action, string Actor, DateTimeOffset At);
public sealed record PortalActivity(string Category, string Status, DateTimeOffset? DueAt);
public enum PortalWriteOutcome { Saved, Conflict, Invalid, NotFound, Unavailable }
public sealed record PortalWriteResult(PortalWriteOutcome Outcome, Guid? RequestId = null);
public sealed record PortalSubmission(string Category, string Description, string Location, Guid SubmissionKey);

// Integration contract proposed by Cameron on 18 October. Team adapter agreement is pending.
// All methods must independently enforce actor permissions. Queries scope BEFORE filtering/counting.
// Writes atomically save request, audit and feedback; expectedVersion guards claims and other edits.
public interface IPortalRequestStore
{
    Task<IReadOnlyList<PortalRow>> ListAsync(PortalActor actor, PortalFilter filter, CancellationToken cancellationToken);
    Task<PortalMetadata?> MetadataAsync(PortalActor actor, Guid requestId, CancellationToken cancellationToken);
    Task<PortalWriteResult> SubmitAsync(PortalActor actor, PortalSubmission submission, CancellationToken cancellationToken);
    Task<PortalWriteResult> AssignAsync(PortalActor actor, Guid requestId, string ownerId, long expectedVersion,
        ValidatedStatusChange change, CancellationToken cancellationToken);
    Task<PortalWriteResult> AddNoteAsync(PortalActor actor, Guid requestId, string content, bool isPublic,
        long expectedVersion, CancellationToken cancellationToken);
    Task<PortalWriteResult> SetScheduleAsync(PortalActor actor, Guid requestId, string priority,
        DateTimeOffset? dueAt, long expectedVersion, CancellationToken cancellationToken);
    Task<IReadOnlyList<PortalActivity>> ActivityAsync(PortalActor actor, CancellationToken cancellationToken);
}
