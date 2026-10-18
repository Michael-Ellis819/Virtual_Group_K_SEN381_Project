using System.Text.RegularExpressions;
using CivicConnect.Lifecycle;

// Business-rule checks for ADR-M2-02 (status transitions), FR-009 (ownership) and
// FR-013 (activity summary). They need no database, login or HTTP, so they also
// demonstrate NFR-009: rules are testable in isolation. Run from Program.cs.
// IDs: LC = lifecycle, OW = ownership, SM = summary, AR = architecture (R-011).
// docs/evidence/business-rule-test-mapping.md maps each ID to its design case.
internal static class LifecycleChecks
{
    private static readonly string[] AllStatuses = ["New", "Assigned", "In Progress", "Resolved", "Closed", "Rejected"];

    private static readonly HashSet<(string From, string To)> Legal =
    [
        ("New", "Assigned"), ("New", "Rejected"), ("Assigned", "In Progress"),
        ("In Progress", "Resolved"), ("Resolved", "Closed")
    ];

    public static void Run(Action<string, bool> check, string? repoRoot)
    {
        RunTransitions(check);
        RunOwnership(check);
        RunSummary(check);
        RunArchitecture(check, repoRoot);
    }

    private static TransitionRequest Req(string from, string to, string? actor = "staff-1",
        string? feedback = "Note", string? owner = "staff-1") => new(from, to, actor, feedback, owner);

    private static void RunTransitions(Action<string, bool> check)
    {
        var svc = new StatusTransitionService();

        check("LC01 exactly the five agreed transitions are allowed, all 36 pairs checked [FR-010 AC-FR-010-1]",
            AllStatuses.SelectMany(f => AllStatuses.Select(t => (f, t)))
                .All(p => svc.Evaluate(Req(p.f, p.t)).Allowed == Legal.Contains(p)));

        var happy = svc.Evaluate(Req("New", "Assigned"));
        check("LC02 allowed change carries previous status, new status and actor [AC-FR-010-1]",
            happy.Allowed && happy.Change is { PreviousStatus: "New", NewStatus: "Assigned", ActorId: "staff-1" });

        check("LC03 skipping a step is rejected without a change object [AC-FR-010-2]",
            svc.Evaluate(Req("New", "In Progress")) is { Outcome: TransitionOutcome.InvalidTransition, Change: null });

        check("LC04 backward move is rejected",
            svc.Evaluate(Req("In Progress", "Assigned")).Outcome == TransitionOutcome.InvalidTransition);

        check("LC05 closed and rejected are terminal: no exit to any status",
            new[] { "Closed", "Rejected" }.All(f => AllStatuses.All(t => !svc.Evaluate(Req(f, t)).Allowed)));

        check("LC06 reopening a closed request is rejected (reopen excluded by ADR-M2-02)",
            svc.Evaluate(Req("Closed", "New")).Outcome == TransitionOutcome.InvalidTransition);

        check("LC07 staying in the same status is not a transition",
            AllStatuses.All(s => svc.Evaluate(Req(s, s)).Outcome == TransitionOutcome.InvalidTransition));

        check("LC08 rejecting a request that is no longer New is rejected",
            svc.Evaluate(Req("Assigned", "Rejected")).Outcome == TransitionOutcome.InvalidTransition);

        check("LC09 unknown, blank, null and wrong-case statuses fail closed",
            new[] { "Open", "", "  ", null, "new", "in progress", "Done" }.All(s =>
                svc.Evaluate(new TransitionRequest(s, "Assigned", "a", "n", "o")).Outcome == TransitionOutcome.UnknownStatus &&
                svc.Evaluate(new TransitionRequest("New", s, "a", "n", "o")).Outcome == TransitionOutcome.UnknownStatus));

        check("LC10 missing actor is rejected so history can always name who acted [FR-015]",
            new[] { null, "", "   " }.All(a => svc.Evaluate(Req("New", "Assigned", actor: a)).Outcome == TransitionOutcome.ActorRequired));

        check("LC11 resolving needs a resolution summary [AC-FR-012-1]",
            svc.Evaluate(Req("In Progress", "Resolved", feedback: null)).Outcome == TransitionOutcome.ReasonRequired &&
            svc.Evaluate(Req("In Progress", "Resolved", feedback: "   ")).Outcome == TransitionOutcome.ReasonRequired &&
            svc.Evaluate(Req("In Progress", "Resolved", feedback: "Replaced the switch")).Allowed);

        check("LC12 rejecting needs a reason [AC-FR-005-1]",
            svc.Evaluate(Req("New", "Rejected", feedback: "")).Outcome == TransitionOutcome.ReasonRequired &&
            svc.Evaluate(Req("New", "Rejected", feedback: "Duplicate of CC-100")).Allowed);

        check("LC13 closing a resolved request needs no reason but is legal [AC-FR-012-2]",
            svc.Evaluate(Req("Resolved", "Closed", feedback: null)).Allowed);

        check("LC14 assigning without an owner is rejected [FR-009]",
            svc.Evaluate(Req("New", "Assigned", owner: null)).Outcome == TransitionOutcome.OwnerRequired &&
            svc.Evaluate(Req("New", "Assigned", owner: " ")).Outcome == TransitionOutcome.OwnerRequired);

        check("LC15 feedback and actor are trimmed in the validated change",
            svc.Evaluate(Req("Resolved", "Closed", actor: "  staff-9 ", feedback: "  done  ")).Change is
            { ActorId: "staff-9", Feedback: "done" });

        check("LC16 an illegal pair is reported as illegal before any reason check",
            svc.Evaluate(Req("New", "Closed", feedback: null)).Outcome == TransitionOutcome.InvalidTransition);

        check("LC17 next-status listing matches the lifecycle and is empty for unknown or terminal",
            svc.AllowedNextStatuses("New").SequenceEqual(["Assigned", "Rejected"]) &&
            svc.AllowedNextStatuses("In Progress").SequenceEqual(["Resolved"]) &&
            svc.AllowedNextStatuses("Closed").Count == 0 && svc.AllowedNextStatuses("Rejected").Count == 0 &&
            svc.AllowedNextStatuses("bogus").Count == 0 && svc.AllowedNextStatuses(null).Count == 0);

        check("LC18 six states exist; only New, Assigned and In Progress are open [M1 rule]",
            RequestStates.All.Count == 6 &&
            RequestStates.All.Where(s => s.IsOpen).Select(s => s.Name).SequenceEqual(["New", "Assigned", "In Progress"]));

        check("LC19 evaluating twice gives the same answer and changes nothing",
            svc.Evaluate(Req("New", "Assigned")).Message == svc.Evaluate(Req("New", "Assigned")).Message);

        bool threw = false;
        try { svc.Evaluate(null!); } catch (ArgumentNullException) { threw = true; }
        check("LC20 null request is a programming error, not a silent pass", threw);
    }

    private static OwnerCandidate Staff(string id = "staff-1", bool active = true, params string[] cats) =>
        new(id, "Staff", active, cats.Length == 0 ? ["IT Support"] : cats);

    private static AssignmentTarget Target(string status = "New", string cat = "IT Support", string? owner = null) =>
        new(status, cat, owner);

    private static void RunOwnership(Action<string, bool> check)
    {
        var rule = new OwnershipRule();

        check("OW01 active staff with the category can own an unowned New request [AC-FR-009-1]",
            rule.Evaluate(Staff(), Target()) is { Outcome: AssignmentOutcome.Assigned, ShouldAssign: true });

        check("OW02 staff without the request's category are rejected [AC-FR-009-2]",
            rule.Evaluate(Staff(cats: "Maintenance"), Target()).Outcome == AssignmentOutcome.RejectedIneligibleOwner);

        check("OW03 inactive staff are rejected [AC-FR-009-2]",
            rule.Evaluate(Staff(active: false), Target()).Outcome == AssignmentOutcome.RejectedIneligibleOwner);

        check("OW04 requesters, management and unknown roles can never own",
            new[] { "Requester", "Management", "Admin", "staff", null }.All(r =>
                rule.Evaluate(new OwnerCandidate("u-1", r, true, ["IT Support"]), Target()).Outcome ==
                AssignmentOutcome.RejectedIneligibleOwner));

        check("OW05 a second person cannot take an owned request, so never two owners [AC-FR-009-2]",
            rule.Evaluate(Staff("staff-2"), Target("Assigned", owner: "staff-1")) is
            { Outcome: AssignmentOutcome.RejectedAlreadyOwned, ShouldAssign: false });

        check("OW06 the current owner claiming again is a harmless no-op",
            rule.Evaluate(Staff("staff-1"), Target("Assigned", owner: "staff-1")) is
            { Outcome: AssignmentOutcome.AlreadyOwnedBySameUser, ShouldAssign: false });

        check("OW07 closed, rejected and resolved requests cannot gain an owner",
            new[] { "Closed", "Rejected", "Resolved", "In Progress", "Assigned" }.All(s =>
                rule.Evaluate(Staff(), Target(s)).Outcome == AssignmentOutcome.RejectedStatusNotAssignable));

        check("OW08 blank candidate, blank category and unknown status are invalid input",
            rule.Evaluate(new OwnerCandidate(" ", "Staff", true, ["IT Support"]), Target()).Outcome == AssignmentOutcome.RejectedInvalidInput &&
            rule.Evaluate(Staff(), Target(cat: "")).Outcome == AssignmentOutcome.RejectedInvalidInput &&
            rule.Evaluate(Staff(), Target("Open")).Outcome == AssignmentOutcome.RejectedInvalidInput);

        check("OW09 category match is exact: a different case is not eligible",
            rule.Evaluate(Staff(cats: "it support"), Target()).Outcome == AssignmentOutcome.RejectedIneligibleOwner);

        check("OW10 a staff member with several categories is eligible for any of them",
            rule.Evaluate(Staff(cats: ["Maintenance", "IT Support"]), Target(cat: "Maintenance")).ShouldAssign);

        // Deciding Assigned must be consistent with the transition service: the
        // coordinator uses the owner to move New to Assigned.
        var decision = rule.Evaluate(Staff(), Target());
        check("OW11 an Assigned decision leads to a legal New to Assigned transition",
            decision.ShouldAssign && new StatusTransitionService().Evaluate(Req("New", "Assigned", owner: "staff-1")).Allowed);
    }

    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-13T10:00:00Z");

    private static ActivityItem Item(string status, double? dueHoursFromNow = null) =>
        new(status, dueHoursFromNow is null ? null : Now.AddHours(dueHoursFromNow.Value));

    private static void RunSummary(Action<string, bool> check)
    {
        var calc = new ServiceActivitySummaryCalculator();

        // Known dataset (AC-FR-013-1). Expected values worked out by hand:
        //   open = New(-2h), Assigned(-0.5h), In Progress(+5h), New(no date), Assigned(+24h) = 5
        //   overdue = open items with a due date strictly in the past = 2
        //   resolved = 2, closed = 1, rejected = 1
        var dataset = new[]
        {
            Item("New", -2), Item("Assigned", -0.5), Item("In Progress", 5), Item("New"), Item("Assigned", 24),
            Item("Resolved", -48), Item("Resolved"), Item("Closed", -72), Item("Rejected", -10)
        };
        var s = calc.Calculate(dataset, Now);
        check("SM01 known dataset gives the hand-calculated counts [AC-FR-013-1]",
            s == new ServiceActivitySummary(Open: 5, Overdue: 2, Resolved: 2, Closed: 1, Rejected: 1));

        check("SM02 every request is accounted for: open + resolved + closed + rejected = total",
            s.Open + s.Resolved + s.Closed + s.Rejected == dataset.Length);

        check("SM03 open means New, Assigned or In Progress only [M1 rule]",
            calc.Calculate(AllStatuses.Select(x => Item(x)), Now).Open == 3);

        check("SM04 a request without a due date is never overdue [M1 rule, AC-FR-018-2]",
            calc.Calculate([Item("New"), Item("In Progress")], Now).Overdue == 0);

        check("SM05 due exactly now is not overdue; one second earlier is",
            calc.Calculate([new ActivityItem("New", Now)], Now).Overdue == 0 &&
            calc.Calculate([new ActivityItem("New", Now.AddSeconds(-1))], Now).Overdue == 1);

        check("SM06 resolved, closed and rejected requests past due are not overdue",
            calc.Calculate([Item("Resolved", -5), Item("Closed", -5), Item("Rejected", -5)], Now).Overdue == 0);

        check("SM07 empty data returns zero counts, not an error [AC-FR-013-2]",
            calc.Calculate([], Now) == new ServiceActivitySummary(0, 0, 0, 0, 0));

        check("SM08 overdue never exceeds open",
            s.Overdue <= s.Open);

        var before = calc.Calculate(dataset, Now);
        var after = calc.Calculate(dataset.Append(Item("In Progress", -1)), Now);
        check("SM09 a newly saved request shows on refresh [AC-FR-013-2]",
            after.Open == before.Open + 1 && after.Overdue == before.Overdue + 1);

        check("SM10 the same data at a later time makes more requests overdue",
            calc.Calculate(dataset, Now.AddDays(2)).Overdue == 4);

        bool threw = false;
        try { calc.Calculate([new ActivityItem("Open", null)], Now); } catch (ArgumentException) { threw = true; }
        check("SM11 an unrecognised status is rejected instead of miscounted", threw);

        bool nullThrew = false;
        try { calc.Calculate(null!, Now); } catch (ArgumentNullException) { nullThrew = true; }
        check("SM12 null item list is a programming error", nullThrew);
    }

    // R-011: only StatusTransitionService decides status changes. Scan the application source
    // for any assignment to a Status property outside the lifecycle namespace.
    private static void RunArchitecture(Action<string, bool> check, string? repoRoot)
    {
        var web = repoRoot is null ? null : Path.Combine(repoRoot, "src", "CivicConnect.Web");
        if (web is null || !Directory.Exists(web))
        {
            check("AR01 application source located for the R-011 scan", false);
            return;
        }

        var assignment = new Regex(@"\bStatus\s*=(?![=>])", RegexOptions.Compiled);
        var offenders = Directory.EnumerateFiles(web, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "Migrations" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "Lifecycle" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Where(f => File.ReadAllLines(f).Any(line => assignment.IsMatch(line)))
            .Select(f => Path.GetFileName(f))
            .Order()
            .ToArray();

        // Known and accepted today: persistence writes the value it is given. Any NEW file
        // appearing here is a bypass of the service and must fail this check.
        string[] accepted = ["EfRequestStatusWriter.cs"];
        check("AR01 no code outside the lifecycle namespace assigns a status except the persistence writer [R-011]",
            offenders.All(o => accepted.Contains(o)));
    }
}
