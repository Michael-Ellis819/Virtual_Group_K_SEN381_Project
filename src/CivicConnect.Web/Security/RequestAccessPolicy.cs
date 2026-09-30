using System.Security.Claims;
using CivicConnect.Domain;
namespace CivicConnect.Security;
public sealed record ReadPermission(bool Allowed, bool IncludeInternalNotes)
{
    public static readonly ReadPermission Denied = new(false, false);
}
public interface IRequestAccessPolicy
{
    ReadPermission EvaluateRead(ClaimsPrincipal user, RequestSnapshot request);
}
public sealed class RequestAccessPolicy : IRequestAccessPolicy
{
    public ReadPermission EvaluateRead(ClaimsPrincipal user, RequestSnapshot request)
    {
        // Trusted server claims only. Ambiguous identities fail closed.
        var identities = user.Identities.ToArray();
        if (identities.Length != 1 || !identities[0].IsAuthenticated) return ReadPermission.Denied;
        var identity = identities[0];
        var ids = identity.FindAll(ClaimTypes.NameIdentifier).ToArray();
        var roles = identity.FindAll(ClaimTypes.Role).ToArray();
        var active = identity.FindAll("active").ToArray();
        if (ids.Length != 1 || string.IsNullOrWhiteSpace(ids[0].Value) || roles.Length != 1 ||
            active.Length != 1 || active[0].Value != "true") return ReadPermission.Denied;
        return roles[0].Value switch
        {
            "Requester" when ids[0].Value == request.RequesterId => new(true, false),
            "Staff" when identity.HasClaim("category", request.CategoryId) => new(true, true),
            _ => ReadPermission.Denied
        };
    }
}
