namespace CivicConnect.Application;

// Integration dependency for ADR-M2-02; Michael remains the lifecycle owner.
public sealed class StatusTransitionService
{
    private static readonly IReadOnlyDictionary<string, IRequestState> States =
        new Dictionary<string, IRequestState>(StringComparer.Ordinal)
        {
            ["New"] = new NewState(), ["Assigned"] = new AssignedState(),
            ["In Progress"] = new InProgressState(), ["Resolved"] = new ResolvedState(),
            ["Closed"] = new ClosedState(), ["Rejected"] = new RejectedState()
        };

    public bool IsAllowed(string current, string target)
        => States.TryGetValue(current, out var state) && state.Allows(target);
}

internal interface IRequestState { bool Allows(string target); }
internal sealed class NewState : IRequestState
{
    public bool Allows(string target) => target is "Assigned" or "Rejected";
}
internal sealed class AssignedState : IRequestState
{
    public bool Allows(string target) => target == "In Progress";
}
internal sealed class InProgressState : IRequestState
{
    public bool Allows(string target) => target == "Resolved";
}
internal sealed class ResolvedState : IRequestState
{
    public bool Allows(string target) => target == "Closed";
}
internal sealed class ClosedState : IRequestState
{
    public bool Allows(string target) => false;
}
internal sealed class RejectedState : IRequestState
{
    public bool Allows(string target) => false;
}
