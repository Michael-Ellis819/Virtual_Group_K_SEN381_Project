namespace CivicConnect.Lifecycle;

// ADR-M2-02: each request status is a State object that owns its legal next states.
// Legality lives here and nowhere else (FR-010). Access permission is a separate
// concern owned by RequestAccessPolicy (ADR-M2-05). Reopening is excluded by design.
public interface IRequestState
{
    string Name { get; }

    // M1 rule: open means New, Assigned or In Progress.
    bool IsOpen { get; }

    // True when no further transition is possible (Closed, Rejected).
    bool IsTerminal { get; }

    // FR-005 / FR-012: entering this state needs a reason or resolution summary.
    bool RequiresReasonOnEntry { get; }

    IReadOnlyList<IRequestState> AllowedNext { get; }

    bool CanTransitionTo(IRequestState next);
}

public abstract class RequestState : IRequestState
{
    // Internal constructor: the six states are fixed by ADR-M2-02, so code outside
    // this assembly cannot introduce a seventh status.
    internal RequestState(string name, bool isOpen, bool requiresReasonOnEntry)
    {
        Name = name;
        IsOpen = isOpen;
        RequiresReasonOnEntry = requiresReasonOnEntry;
    }

    public string Name { get; }

    public bool IsOpen { get; }

    public bool RequiresReasonOnEntry { get; }

    public bool IsTerminal => AllowedNext.Count == 0;

    public abstract IReadOnlyList<IRequestState> AllowedNext { get; }

    public bool CanTransitionTo(IRequestState next) =>
        next is not null && AllowedNext.Contains(next);

    public override string ToString() => Name;
}

public sealed class NewState : RequestState
{
    public static readonly NewState Instance = new();

    private NewState() : base("New", isOpen: true, requiresReasonOnEntry: false) { }

    public override IReadOnlyList<IRequestState> AllowedNext =>
        [AssignedState.Instance, RejectedState.Instance];
}

public sealed class AssignedState : RequestState
{
    public static readonly AssignedState Instance = new();

    private AssignedState() : base("Assigned", isOpen: true, requiresReasonOnEntry: false) { }

    public override IReadOnlyList<IRequestState> AllowedNext => [InProgressState.Instance];
}

public sealed class InProgressState : RequestState
{
    public static readonly InProgressState Instance = new();

    private InProgressState() : base("In Progress", isOpen: true, requiresReasonOnEntry: false) { }

    public override IReadOnlyList<IRequestState> AllowedNext => [ResolvedState.Instance];
}

public sealed class ResolvedState : RequestState
{
    public static readonly ResolvedState Instance = new();

    // AC-FR-012-1: resolving requires a resolution summary.
    private ResolvedState() : base("Resolved", isOpen: false, requiresReasonOnEntry: true) { }

    public override IReadOnlyList<IRequestState> AllowedNext => [ClosedState.Instance];
}

public sealed class ClosedState : RequestState
{
    public static readonly ClosedState Instance = new();

    private ClosedState() : base("Closed", isOpen: false, requiresReasonOnEntry: false) { }

    public override IReadOnlyList<IRequestState> AllowedNext => [];
}

public sealed class RejectedState : RequestState
{
    public static readonly RejectedState Instance = new();

    // AC-FR-005-1: rejection includes a reason.
    private RejectedState() : base("Rejected", isOpen: false, requiresReasonOnEntry: true) { }

    public override IReadOnlyList<IRequestState> AllowedNext => [];
}

// Maps the status string stored by persistence to its State object.
public static class RequestStates
{
    public static IReadOnlyList<IRequestState> All { get; } =
    [
        NewState.Instance, AssignedState.Instance, InProgressState.Instance,
        ResolvedState.Instance, ClosedState.Instance, RejectedState.Instance
    ];

    // Exact, case-sensitive match on the stored value. Unknown or blank returns null
    // so callers fail closed rather than guessing.
    public static IRequestState? FromName(string? name) => name switch
    {
        "New" => NewState.Instance,
        "Assigned" => AssignedState.Instance,
        "In Progress" => InProgressState.Instance,
        "Resolved" => ResolvedState.Instance,
        "Closed" => ClosedState.Instance,
        "Rejected" => RejectedState.Instance,
        _ => null
    };
}
