using CivicConnect.Application;
using CivicConnect.Lifecycle;
using CivicConnect.Models;
using CivicConnect.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicConnect.Controllers;

[Authorize, Route("portal")]
public sealed class PortalController(IRequestReader reader, RequestQueryService queries, RequestStatusService statuses,
    IRequestAccessPolicy access, IStatusTransitionService transitions, IOwnershipRule ownership,
    IServiceActivitySummaryCalculator summaries, IEnumerable<IPortalRequestStore> stores,
    IEnumerable<IPortalAccounts> accountAdapters) : Controller
{
    private IPortalRequestStore? Store => stores.SingleOrDefault();
    private PortalActor? Actor => PortalActor.From(User);

    [Authorize(Roles = "Requester,Staff"), HttpGet("requests")]
    public async Task<IActionResult> Requests([FromQuery] ListInput input, CancellationToken cancellationToken)
    {
        var actor = Actor;
        if (actor is null) return Forbid();
        if (!ValidFilter(input)) ModelState.AddModelError("", "Check the filter values and date range.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View(new ListPage(input, [], Store is not null)); }
        if (Store is not { } store) { Response.StatusCode = 503; return View(new ListPage(input, [], false)); }
        var filter = new PortalFilter(input.Search?.Trim(), input.Status, input.Category,
            input.From is { } from ? new DateTimeOffset(DateTime.SpecifyKind(from.Date, DateTimeKind.Utc)) : null,
            input.To is { } to ? new DateTimeOffset(DateTime.SpecifyKind(to.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)) : null,
            input.Priority, input.Sort);
        return View(new ListPage(input, await store.ListAsync(actor, filter, cancellationToken), true));
    }

    [Authorize(Roles = "Requester"), HttpGet("requests/new")]
    public IActionResult Create() => View(new SubmissionInput());

    [Authorize(Roles = "Requester"), HttpPost("requests/new"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubmissionInput input, CancellationToken cancellationToken)
    {
        var actor = Actor;
        if (actor is null) return Forbid();
        if (!PortalOptions.Categories.Contains(input.Category) || string.IsNullOrWhiteSpace(input.Description) ||
            string.IsNullOrWhiteSpace(input.Location) || input.SubmissionKey == Guid.Empty)
            ModelState.AddModelError("", "Choose a listed category and enter a description and location.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View(input); }
        if (Store is not { } store) { Response.StatusCode = 503; ModelState.AddModelError("", "Submission is temporarily unavailable. Your request has not been saved."); return View(input); }
        var result = await store.SubmitAsync(actor, new(input.Category, input.Description.Trim(), input.Location.Trim(), input.SubmissionKey), cancellationToken);
        if (result.Outcome == PortalWriteOutcome.Saved && result.RequestId is { } id) return Redirect($"/portal/requests/{id}");
        Response.StatusCode = result.Outcome == PortalWriteOutcome.Conflict ? 409 : result.Outcome == PortalWriteOutcome.Unavailable ? 503 : 400;
        ModelState.AddModelError("", "The request could not be saved. Check your entries before trying again.");
        return View(input);
    }

    [HttpGet("requests/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Requester" or "Staff" } actor) return NotFound();
        var request = await queries.ReadAsync(User, id, cancellationToken);
        if (request is null) return NotFound();
        var snapshot = await reader.FindAsync(id, cancellationToken);
        if (snapshot is null || !access.EvaluateRead(User, snapshot).Allowed) return NotFound();
        var metadata = Store is { } store ? await store.MetadataAsync(actor, id, cancellationToken) : null;
        if (metadata is not null && actor.Role == "Requester") metadata = metadata with
        {
            Requester = null,
            Notes = metadata.Notes.Where(n => n.IsPublic).ToArray(),
            History = [] // A separate reviewed public history projection is required before exposing audit entries.
        };
        return View(new DetailPage(request, metadata, snapshot.Version, access.CanChangeStatus(User, snapshot),
            transitions.AllowedNextStatuses(snapshot.Status).Where(s => s != "Assigned").ToArray(), Store is not null));
    }

    [Authorize(Roles = "Staff"), HttpPost("requests/{id:guid}/status"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(Guid id, StatusInput input, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Staff" }) return Forbid();
        if (!ModelState.IsValid) return ActionError(400, "Enter a status and feedback of at most 2,000 characters.", id);
        // Owner assignment is a separate atomic command, never a status-only UI shortcut.
        if (input.TargetStatus == "Assigned") return ActionError(400, "Assign an eligible owner using the assignment form.", id);
        var result = await statuses.ChangeAsync(User, id, input.ExpectedStatus, input.TargetStatus, input.Feedback, cancellationToken);
        return result switch
        {
            StatusChangeResult.Saved => Redirect($"/portal/requests/{id}"),
            StatusChangeResult.NotFound => NotFound(),
            StatusChangeResult.Conflict => ActionError(409, "This request changed. Refresh it before retrying.", id),
            _ => ActionError(400, "The status change or feedback is invalid.", id)
        };
    }

    [Authorize(Roles = "Staff"), HttpPost("requests/{id:guid}/assign"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid id, AssignmentInput input, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Staff" } actor) return Forbid();
        var request = await reader.FindAsync(id, cancellationToken);
        if (request is null || !access.CanChangeStatus(User, request)) return NotFound();
        if (!ModelState.IsValid || input.ExpectedVersion < 1) return ActionError(400, "Enter an eligible staff account.", id);
        if (Store is not { } store || accountAdapters.SingleOrDefault() is not { } accounts)
            return ActionError(503, "Assignment is temporarily unavailable. No owner has been changed.", id);
        if (request.Version != input.ExpectedVersion) return ActionError(409, "This request changed. Refresh it before retrying.", id);
        var metadata = await store.MetadataAsync(actor, id, cancellationToken);
        if (metadata is null) return NotFound();
        var owner = await accounts.FindAsync(input.OwnerId.Trim(), cancellationToken);
        if (owner is null) return ActionError(400, "The selected owner is not eligible.", id);
        var decision = ownership.Evaluate(new(owner.Id, owner.Role, owner.IsActive, owner.Categories),
            new(request.Status, request.CategoryId, metadata.OwnerId));
        var transition = transitions.Evaluate(new(request.Status, "Assigned", actor.Id, "Request assigned.", owner.Id));
        if (!decision.ShouldAssign || !transition.Allowed) return ActionError(400, "The selected owner is not eligible or the request is already assigned.", id);
        return WriteResult(await store.AssignAsync(actor, id, owner.Id, input.ExpectedVersion, transition.Change!, cancellationToken), id);
    }

    [Authorize(Roles = "Staff"), HttpPost("requests/{id:guid}/notes"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Note(Guid id, NoteInput input, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Staff" } actor) return Forbid();
        var request = await reader.FindAsync(id, cancellationToken);
        if (request is null || !access.CanChangeStatus(User, request)) return NotFound();
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(input.Content) || input.ExpectedVersion < 1)
            return ActionError(400, "Enter a note of 1 to 2,000 characters.", id);
        if (Store is not { } store) return ActionError(503, "Notes are temporarily unavailable. Your note has not been saved.", id);
        return WriteResult(await store.AddNoteAsync(actor, id, input.Content.Trim(), input.IsPublic, input.ExpectedVersion, cancellationToken), id);
    }

    [Authorize(Roles = "Staff"), HttpPost("requests/{id:guid}/schedule"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(Guid id, ScheduleInput input, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Staff" } actor) return Forbid();
        var request = await reader.FindAsync(id, cancellationToken);
        if (request is null || !access.CanChangeStatus(User, request)) return NotFound();
        if (!ModelState.IsValid || !PortalOptions.Priorities.Contains(input.Priority) || input.ExpectedVersion < 1)
            return ActionError(400, "Choose a listed priority and a valid due date.", id);
        if (Store is not { } store) return ActionError(503, "Priority and due-date updates are temporarily unavailable.", id);
        var dueAt = input.DueAt is { } due ? new DateTimeOffset(DateTime.SpecifyKind(due, DateTimeKind.Utc)) : (DateTimeOffset?)null;
        return WriteResult(await store.SetScheduleAsync(actor, id, input.Priority, dueAt, input.ExpectedVersion, cancellationToken), id);
    }

    [Authorize(Roles = "Management"), HttpGet("management")]
    public async Task<IActionResult> Management(string? category, string? status, CancellationToken cancellationToken)
    {
        if (Actor is not { Role: "Management" } actor) return Forbid();
        if ((!string.IsNullOrEmpty(category) && !PortalOptions.Categories.Contains(category)) ||
            (!string.IsNullOrEmpty(status) && !PortalOptions.Statuses.Contains(status))) return BadRequest();
        if (Store is not { } store) { Response.StatusCode = 503; return View(new ManagementPage(category, status, new(0, 0, 0, 0, 0), [], false)); }
        var rows = (await store.ActivityAsync(actor, cancellationToken)).Where(r =>
            (string.IsNullOrEmpty(category) || r.Category == category) && (string.IsNullOrEmpty(status) || r.Status == status)).ToArray();
        return View(new ManagementPage(category, status,
            summaries.Calculate(rows.Select(r => new ActivityItem(r.Status, r.DueAt)), DateTimeOffset.UtcNow), rows, true));
    }

    private bool ValidFilter(ListInput input) =>
        (string.IsNullOrEmpty(input.Category) || PortalOptions.Categories.Contains(input.Category)) &&
        (string.IsNullOrEmpty(input.Status) || PortalOptions.Statuses.Contains(input.Status)) &&
        (string.IsNullOrEmpty(input.Priority) || PortalOptions.Priorities.Contains(input.Priority)) &&
        input.Sort is "date-desc" or "date-asc" or "priority-desc" or "priority-asc" &&
        (input.From is null || input.To is null || input.From <= input.To) && input.To != DateTime.MaxValue.Date;

    private IActionResult WriteResult(PortalWriteResult result, Guid id) => result.Outcome switch
    {
        PortalWriteOutcome.Saved => Redirect($"/portal/requests/{id}"),
        PortalWriteOutcome.NotFound => NotFound(),
        PortalWriteOutcome.Conflict => ActionError(409, "This request changed. Refresh it before retrying.", id),
        PortalWriteOutcome.Unavailable => ActionError(503, "This action is temporarily unavailable. No change has been saved.", id),
        _ => ActionError(400, "The change is invalid. Check the request and try again.", id)
    };
    private IActionResult ActionError(int status, string message, Guid id)
    {
        Response.StatusCode = status;
        ViewData["Message"] = message;
        ViewData["RequestId"] = id;
        return View("ActionError");
    }
}
