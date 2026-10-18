namespace CivicConnect.Lifecycle;

public enum TransitionOutcome
{
    Allowed,
    UnknownStatus,
    ActorRequired,
    InvalidTransition,
    ReasonRequired,
    OwnerRequired
}

// Everything the service needs to judge legality. It deliberately carries no
// identity or role: who may ask is decided by the access policy (ADR-M2-05).
public sealed record TransitionRequest(
    string? CurrentStatus,
    string? TargetStatus,
    string? ActorId,
    string? Feedback = null,
    string? OwnerId = null);

// Proof that StatusTransitionService approved a change. The constructor is internal,
// so only this assembly's service can create one. Persistence should accept this type
// rather than raw status strings (risk R-011, TD: EfRequestStatusWriter still takes strings).
public sealed class ValidatedStatusChange
{
    internal ValidatedStatusChange(string previousStatus, string newStatus, string actorId, string? feedback)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ActorId = actorId;
        Feedback = feedback;
    }

    public string PreviousStatus { get; }

    public string NewStatus { get; }

    public string ActorId { get; }

    public string? Feedback { get; }
}

public sealed record TransitionResult(TransitionOutcome Outcome, string Message, ValidatedStatusChange? Change)
{
    public bool Allowed => Outcome == TransitionOutcome.Allowed;
}

public interface IStatusTransitionService
{
    // Pure decision. Never changes anything; the caller persists a ValidatedStatusChange.
    TransitionResult Evaluate(TransitionRequest request);

    // Statuses legally reachable from the given status; empty when unknown or terminal.
    IReadOnlyList<string> AllowedNextStatuses(string? currentStatus);
}

// The single component allowed to decide whether a status may change (ADR-M2-02, R-011).
public sealed class StatusTransitionService : IStatusTransitionService
{
    public TransitionResult Evaluate(TransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var from = RequestStates.FromName(request.CurrentStatus);
        var to = RequestStates.FromName(request.TargetStatus);
        if (from is null || to is null)
            return Reject(TransitionOutcome.UnknownStatus, "The current or requested status is not recognised.");

        if (string.IsNullOrWhiteSpace(request.ActorId))
            return Reject(TransitionOutcome.ActorRequired, "A status change must record who made it.");

        if (!from.CanTransitionTo(to))
            return Reject(TransitionOutcome.InvalidTransition,
                $"A request cannot move from {from.Name} to {to.Name}.");

        var feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();
        if (to.RequiresReasonOnEntry && feedback is null)
            return Reject(TransitionOutcome.ReasonRequired,
                $"Moving a request to {to.Name} requires a reason or summary.");

        // A request cannot be Assigned without exactly one owner (FR-009).
        if (to is AssignedState && string.IsNullOrWhiteSpace(request.OwnerId))
            return Reject(TransitionOutcome.OwnerRequired, "A request cannot be Assigned without an owner.");

        return new TransitionResult(
            TransitionOutcome.Allowed,
            $"{from.Name} to {to.Name} is permitted.",
            new ValidatedStatusChange(from.Name, to.Name, request.ActorId.Trim(), feedback));
    }

    public IReadOnlyList<string> AllowedNextStatuses(string? currentStatus) =>
        RequestStates.FromName(currentStatus)?.AllowedNext.Select(s => s.Name).ToArray() ?? [];

    private static TransitionResult Reject(TransitionOutcome outcome, string message) =>
        new(outcome, message, null);
}
