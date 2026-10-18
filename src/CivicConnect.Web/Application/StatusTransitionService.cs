namespace CivicConnect.Application;

// Retains the M3 coordinator contract using Bianca's single lifecycle matrix.
public sealed class StatusTransitionService
{
    private readonly Lifecycle.IStatusTransitionService rules = new Lifecycle.StatusTransitionService();

    public bool IsAllowed(string current, string target)
        => rules.AllowedNextStatuses(current).Contains(target, StringComparer.Ordinal);
}
