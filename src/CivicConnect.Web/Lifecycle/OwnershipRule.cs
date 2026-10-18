namespace CivicConnect.Lifecycle;

public enum AssignmentOutcome
{
    Assigned,
    AlreadyOwnedBySameUser,
    RejectedInvalidInput,
    RejectedIneligibleOwner,
    RejectedAlreadyOwned,
    RejectedStatusNotAssignable
}

// The person proposed as owner. Mirrors the claims RequestAccessPolicy already trusts:
// one user ID, one role, an active flag and the staff member's category grants.
public sealed record OwnerCandidate(
    string? UserId,
    string? Role,
    bool IsActive,
    IReadOnlyCollection<string> Categories);

// The request as it stands now. CurrentOwnerId is null when nobody owns it.
public sealed record AssignmentTarget(
    string? Status,
    string? CategoryId,
    string? CurrentOwnerId);

public sealed record AssignmentDecision(AssignmentOutcome Outcome, string Message)
{
    // True only when the caller should now save the owner and move New to Assigned.
    public bool ShouldAssign => Outcome == AssignmentOutcome.Assigned;
}

public interface IOwnershipRule
{
    AssignmentDecision Evaluate(OwnerCandidate candidate, AssignmentTarget target);
}

// FR-009: one eligible owner per request.
// Eligible means an active Staff member granted the request's category. Management and
// Requesters are never owners. The rule is a pure decision. It cannot stop two
// simultaneous claims by itself: persistence must save the owner only if none exists
// (conditional update or the Version concurrency token), so the loser re-evaluates and
// receives RejectedAlreadyOwned.
public sealed class OwnershipRule : IOwnershipRule
{
    public AssignmentDecision Evaluate(OwnerCandidate candidate, AssignmentTarget target)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(target);

        if (string.IsNullOrWhiteSpace(candidate.UserId) ||
            string.IsNullOrWhiteSpace(target.CategoryId) ||
            RequestStates.FromName(target.Status) is null)
            return Decide(AssignmentOutcome.RejectedInvalidInput, "The assignment information is incomplete or invalid.");

        if (candidate.Role != "Staff" || !candidate.IsActive ||
            candidate.Categories is null || !candidate.Categories.Contains(target.CategoryId))
            return Decide(AssignmentOutcome.RejectedIneligibleOwner,
                "Only an active staff member with access to this category can own the request.");

        if (!string.IsNullOrWhiteSpace(target.CurrentOwnerId))
            return target.CurrentOwnerId == candidate.UserId
                ? Decide(AssignmentOutcome.AlreadyOwnedBySameUser, "This staff member already owns the request.")
                : Decide(AssignmentOutcome.RejectedAlreadyOwned, "The request already has an owner.");

        // First assignment is the New to Assigned step; any other status cannot gain an owner.
        if (target.Status != NewState.Instance.Name)
            return Decide(AssignmentOutcome.RejectedStatusNotAssignable,
                $"A request in status {target.Status} cannot be assigned.");

        return Decide(AssignmentOutcome.Assigned, "The request can be assigned to this staff member.");
    }

    private static AssignmentDecision Decide(AssignmentOutcome outcome, string message) => new(outcome, message);
}
