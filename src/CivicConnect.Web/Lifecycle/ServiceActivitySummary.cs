namespace CivicConnect.Lifecycle;

// One request reduced to what the summary needs. DueAt is null when no due date is set.
public sealed record ActivityItem(string? Status, DateTimeOffset? DueAt);

// FR-013. Open = New, Assigned or In Progress (M1 rule). Overdue is a subset of Open.
// Rejected requests are in none of the four management counts and are reported
// separately so every request is accounted for.
public sealed record ServiceActivitySummary(int Open, int Overdue, int Resolved, int Closed, int Rejected);

public interface IServiceActivitySummaryCalculator
{
    ServiceActivitySummary Calculate(IEnumerable<ActivityItem> items, DateTimeOffset asOf);
}

public sealed class ServiceActivitySummaryCalculator : IServiceActivitySummaryCalculator
{
    public ServiceActivitySummary Calculate(IEnumerable<ActivityItem> items, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(items);

        int open = 0, overdue = 0, resolved = 0, closed = 0, rejected = 0;
        foreach (var item in items)
        {
            // An unrecognised status means corrupt data. Fail loudly rather than
            // under-count silently.
            var state = RequestStates.FromName(item.Status)
                ?? throw new ArgumentException($"Unrecognised request status '{item.Status}'.", nameof(items));

            if (state.IsOpen)
            {
                open++;
                // M1 rule: no due date means not overdue. Due exactly now is not yet overdue.
                if (item.DueAt is { } due && due < asOf) overdue++;
            }
            else if (state is ResolvedState) resolved++;
            else if (state is ClosedState) closed++;
            else rejected++;
        }

        return new ServiceActivitySummary(open, overdue, resolved, closed, rejected);
    }
}
