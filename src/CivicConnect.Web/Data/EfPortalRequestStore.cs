using CivicConnect.Application;
using CivicConnect.Data.Entities;
using CivicConnect.Lifecycle;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CivicConnect.Data;

public sealed class EfPortalRequestStore : IPortalRequestStore
{
    private readonly CivicConnectDbContext _db;

    private readonly IOwnershipRule _ownershipRule;
    private readonly EfRequestStatusWriter _statusWriter;

    public EfPortalRequestStore(
        CivicConnectDbContext db,
        IOwnershipRule ownershipRule,
        EfRequestStatusWriter statusWriter)
    {
        _db = db;
        _ownershipRule = ownershipRule;
        _statusWriter = statusWriter;
    }
    
    public async Task<IReadOnlyList<PortalRow>> ListAsync(
        PortalActor actor,
        PortalFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _db.ServiceRequests.AsNoTracking().AsQueryable();

        if (actor.Role == "Requester" && !string.IsNullOrWhiteSpace(actor.Id))
        {
            query = query.Where(r => r.RequesterId == actor.Id);
        }
        else if (actor.Role == "Staff" && !string.IsNullOrWhiteSpace(actor.Id))
        {
            query = query.Where(r => actor.Categories.Contains(r.CategoryId));
        }
        else
        {
            return Array.Empty<PortalRow>();
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(r =>
                r.ReferenceNumber.Contains(search) ||
                r.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(r => r.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(r => r.CategoryId == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.Priority))
            query = query.Where(r => r.Priority == filter.Priority);

        if (filter.From.HasValue)
            query = query.Where(r => r.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(r => r.CreatedAt <= filter.To.Value);

        query = filter.Sort switch
        {
            "date-asc" => query.OrderBy(r => r.CreatedAt),
            "priority-desc" => query.OrderByDescending(r =>
                r.Priority == "High" ? 3 :
                r.Priority == "Normal" ? 2 : 1).ThenByDescending(r => r.CreatedAt).ThenBy(r => r.RequestId),
            "priority-asc" => query.OrderBy(r =>
                r.Priority == "High" ? 3 :
                r.Priority == "Normal" ? 2 : 1).ThenBy(r => r.CreatedAt).ThenBy(r => r.RequestId),
            _ => query.OrderByDescending(r => r.CreatedAt).ThenBy(r => r.RequestId)
        };

        return await query
            .Select(r => new PortalRow(
                r.RequestId,
                r.ReferenceNumber,
                r.CategoryId,
                r.CreatedAt,
                r.Status,
                r.Priority,
                r.OwnerId,
                r.DueAt))
            .ToListAsync(cancellationToken);
        
        
    }
    public async Task<PortalMetadata?> MetadataAsync(
        PortalActor actor,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor.Id))
            return null;

        var query = _db.ServiceRequests
            .AsNoTracking()
            .Where(r => r.RequestId == requestId);

        if (actor.Role == "Requester")
        {
            query = query.Where(r => r.RequesterId == actor.Id);
        }
        else if (actor.Role == "Staff")
        {
            query = query.Where(r => actor.Categories.Contains(r.CategoryId));
        }
        else
        {
            return null;
        }

        var request = await query.FirstOrDefaultAsync(cancellationToken);

        if (request is null)
            return null;

        var notesQuery = _db.RequestNotes
            .AsNoTracking()
            .Where(n => n.RequestId == requestId);

        if (actor.Role == "Requester")
            notesQuery = notesQuery.Where(n => n.IsRequesterVisible);

        var notes = await notesQuery
            .OrderBy(n => n.CreatedAt)
            .Select(n => new PortalNote(
                n.Content,
                n.AuthorId,
                n.CreatedAt,
                n.IsRequesterVisible))
            .ToListAsync(cancellationToken);

        var history = await _db.RequestStatusHistory
            .AsNoTracking()
            .Where(h => h.RequestId == requestId)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new PortalHistory(
                h.PreviousStatus + " → " + h.NewStatus,
                h.ChangedByUserId,
                h.ChangedAt,
                true))
            .ToListAsync(cancellationToken);

        if (actor.Role == "Staff")
        {
            var schedules = await _db.RequestScheduleAudits.AsNoTracking()
                .Where(a => a.RequestId == requestId).ToListAsync(cancellationToken);
            history.AddRange(schedules.Select(a => new PortalHistory(
                $"Priority {a.PreviousPriority} → {a.NewPriority}; due {FormatDue(a.PreviousDueAt)} → {FormatDue(a.NewDueAt)}",
                a.ChangedByUserId, a.ChangedAt)));
            history.AddRange(notes.Select(n => new PortalHistory(
                n.IsPublic ? "Public update added" : "Internal note added", n.Author, n.At)));
        }

        return new PortalMetadata(
            request.RequesterId,
            request.Priority,
            request.OwnerId,
            request.DueAt,
            notes,
            history.OrderBy(h => h.At).ToArray(),
            request.CreatedAt);
    }

    private static string FormatDue(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'") ?? "none";
    
    public async Task<PortalWriteResult> SubmitAsync(
    PortalActor actor,
    PortalSubmission submission,
    CancellationToken cancellationToken)
{
    if (actor.Role != "Requester" ||
        string.IsNullOrWhiteSpace(actor.Id) ||
        actor.Id.Length > 100 ||
        submission.SubmissionKey == Guid.Empty ||
        !CivicConnect.Models.PortalOptions.Categories.Contains(submission.Category) ||
        string.IsNullOrWhiteSpace(submission.Description) ||
        submission.Description.Length > 2000 ||
        string.IsNullOrWhiteSpace(submission.Location) ||
        submission.Location.Length > 250)
    {
        return new PortalWriteResult(PortalWriteOutcome.Invalid);
    }

    var existing = await _db.ServiceRequests
        .AsNoTracking()
        .FirstOrDefaultAsync(r =>
            r.RequesterId == actor.Id &&
            r.SubmissionKey == submission.SubmissionKey,
            cancellationToken);

    if (existing != null)
    {
        bool sameSubmission =
            existing.CategoryId == submission.Category &&
            existing.Description == submission.Description &&
            existing.Location == submission.Location;

        return sameSubmission
            ? new PortalWriteResult(PortalWriteOutcome.Saved, existing.RequestId)
            : new PortalWriteResult(PortalWriteOutcome.Conflict);
    }

    var now = DateTimeOffset.UtcNow;
    var requestId = Guid.NewGuid();

    var request = new ServiceRequestEntity
    {
        RequestId = requestId,
        ReferenceNumber = $"CC-{requestId:N}",
        RequesterId = actor.Id,
        CategoryId = submission.Category,
        Description = submission.Description,
        Location = submission.Location,
        CreatedAt = now,
        UpdatedAt = now,
        Version = 1,
        Priority = "Normal",
        SubmissionKey = submission.SubmissionKey
    };

    _db.ServiceRequests.Add(request);

    _db.RequestStatusHistory.Add(new RequestStatusHistoryEntity
    {
        HistoryId = Guid.NewGuid(),
        RequestId = requestId,
        PreviousStatus = "",
        NewStatus = "New",
        ChangedByUserId = actor.Id,
        ChangedAt = now
    });

    _db.RequestNotes.Add(new RequestNoteEntity
    {
        NoteId = Guid.NewGuid(),
        RequestId = requestId,
        Content = "Your request has been submitted.",
        IsRequesterVisible = true,
        AuthorId = actor.Id,
        CreatedAt = now
    });

    try
    {
        await _db.SaveChangesAsync(cancellationToken);

        return new PortalWriteResult(
            PortalWriteOutcome.Saved, requestId);
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is PostgresException pg &&
              pg.SqlState == PostgresErrorCodes.UniqueViolation &&
              pg.ConstraintName == "IX_ServiceRequests_RequesterId_SubmissionKey")
    {
        _db.ChangeTracker.Clear();

        var duplicate = await _db.ServiceRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                    r.RequesterId == actor.Id &&
                    r.SubmissionKey == submission.SubmissionKey,
                cancellationToken);

        if (duplicate is null)
            throw;

        bool samePayload =
            duplicate.CategoryId == submission.Category &&
            duplicate.Description == submission.Description &&
            duplicate.Location == submission.Location;

        return samePayload
            ? new PortalWriteResult(
                PortalWriteOutcome.Saved, duplicate.RequestId)
            : new PortalWriteResult(PortalWriteOutcome.Conflict);
    }
}
    
    public async Task<PortalWriteResult> AssignAsync(
    PortalActor actor,
    Guid requestId,
    string ownerId,
    long expectedVersion,
    ValidatedStatusChange change,
    CancellationToken cancellationToken)
{
    if (actor.Role != "Staff" ||
        string.IsNullOrWhiteSpace(actor.Id) ||
        requestId == Guid.Empty ||
        string.IsNullOrWhiteSpace(ownerId) ||
        expectedVersion < 1 ||
        change is null ||
        change.ActorId != actor.Id ||
        change.PreviousStatus != "New" ||
        change.NewStatus != "Assigned")
    {
        return new PortalWriteResult(PortalWriteOutcome.Invalid);
    }

    var request = await _db.ServiceRequests
        .AsNoTracking()
        .SingleOrDefaultAsync(
            r => r.RequestId == requestId,
            cancellationToken);

    if (request is null ||
        !actor.Categories.Contains(request.CategoryId))
        return new PortalWriteResult(PortalWriteOutcome.NotFound);

    if (request.Version != expectedVersion)
        return new PortalWriteResult(PortalWriteOutcome.Conflict);

    var owner = await _db.PortalAccounts
        .AsNoTracking()
        .Include(a => a.Categories)
        .SingleOrDefaultAsync(
            a => a.Id == ownerId,
            cancellationToken);

    if (owner is null)
        return new PortalWriteResult(PortalWriteOutcome.Invalid);

    var decision = _ownershipRule.Evaluate(
        new OwnerCandidate(
            owner.Id,
            owner.Role,
            owner.IsActive,
            owner.Categories.Select(c => c.CategoryId).ToArray()),
        new AssignmentTarget(
            request.Status,
            request.CategoryId,
            request.OwnerId));

    if (!decision.ShouldAssign ||
        request.Status != change.PreviousStatus)
        return new PortalWriteResult(PortalWriteOutcome.Conflict);

    try
    {
        await _statusWriter.SaveAssignmentAsync(
            requestId,
            owner.Id,
            expectedVersion,
            change,
            cancellationToken,
            actor.Id);

        return new PortalWriteResult(
            PortalWriteOutcome.Saved,
            requestId);
    }
    catch (DbUpdateConcurrencyException)
    {
        return new PortalWriteResult(PortalWriteOutcome.Conflict);
    }
}
    
    public async Task<PortalWriteResult> AddNoteAsync(
        PortalActor actor,
        Guid requestId,
        string content,
        bool isPublic,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        if (actor.Role != "Staff" ||
            string.IsNullOrWhiteSpace(actor.Id) ||
            actor.Id.Length > 100 ||
            requestId == Guid.Empty ||
            string.IsNullOrWhiteSpace(content) ||
            content.Length > 2000 ||
            expectedVersion < 1)
        {
            return new PortalWriteResult(PortalWriteOutcome.Invalid);
        }

        var request = await _db.ServiceRequests
            .SingleOrDefaultAsync(
                r => r.RequestId == requestId &&
                     actor.Categories.Contains(r.CategoryId),
                cancellationToken);

        if (request is null)
            return new PortalWriteResult(PortalWriteOutcome.NotFound);

        if (request.Version != expectedVersion)
            return new PortalWriteResult(PortalWriteOutcome.Conflict);

        var now = DateTimeOffset.UtcNow;

        // Mark the request as modified so EF Core checks its version.
        request.UpdatedAt = now;
        _db.Entry(request).Property(r => r.UpdatedAt).IsModified = true;

        _db.RequestNotes.Add(new RequestNoteEntity
        {
            NoteId = Guid.NewGuid(),
            RequestId = requestId,
            Content = content.Trim(),
            IsRequesterVisible = isPublic,
            AuthorId = actor.Id,
            CreatedAt = now
        });

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            return new PortalWriteResult(
                PortalWriteOutcome.Saved,
                requestId);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new PortalWriteResult(PortalWriteOutcome.Conflict);
        }
    }
    
    public async Task<IReadOnlyList<PortalActivity>> ActivityAsync(
        PortalActor actor,
        CancellationToken cancellationToken)
    {
        if (actor.Role != "Management" ||
            string.IsNullOrWhiteSpace(actor.Id))
        {
            return Array.Empty<PortalActivity>();
        }

        return await _db.ServiceRequests
            .AsNoTracking()
            .Select(r => new PortalActivity(
                r.CategoryId,
                r.Status,
                r.DueAt,
                r.RequestId,
                r.ReferenceNumber))
            .ToListAsync(cancellationToken);
    }
    
    public async Task<PortalWriteResult> SetScheduleAsync(
        PortalActor actor,
        Guid requestId,
        string priority,
        DateTimeOffset? dueAt,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        if (actor.Role != "Staff" ||
            string.IsNullOrWhiteSpace(actor.Id) ||
            actor.Id.Length > 100 ||
            requestId == Guid.Empty ||
            !CivicConnect.Models.PortalOptions.Priorities.Contains(priority) ||
            expectedVersion < 1)
        {
            return new PortalWriteResult(PortalWriteOutcome.Invalid);
        }

        var request = await _db.ServiceRequests
            .SingleOrDefaultAsync(
                r => r.RequestId == requestId &&
                     actor.Categories.Contains(r.CategoryId),
                cancellationToken);

        if (request is null)
            return new PortalWriteResult(PortalWriteOutcome.NotFound);

        if (request.Version != expectedVersion)
            return new PortalWriteResult(PortalWriteOutcome.Conflict);

        var now = DateTimeOffset.UtcNow;
        var newDueAt = dueAt?.ToUniversalTime();

        var previousPriority = request.Priority;
        var previousDueAt = request.DueAt;

        request.Priority = priority;
        request.DueAt = newDueAt;
        request.UpdatedAt = now;

        if (previousPriority != priority || previousDueAt != newDueAt)
        {
            _db.RequestScheduleAudits.Add(new RequestScheduleAuditEntity
            {
                AuditId = Guid.NewGuid(),
                RequestId = requestId,
                ChangedByUserId = actor.Id,
                ChangedAt = now,
                PreviousPriority = previousPriority,
                NewPriority = priority,
                PreviousDueAt = previousDueAt,
                NewDueAt = newDueAt
            });
        }

        // Ensure the request version changes even if the values are unchanged.
        _db.Entry(request).Property(r => r.UpdatedAt).IsModified = true;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            return new PortalWriteResult(
                PortalWriteOutcome.Saved,
                requestId);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new PortalWriteResult(PortalWriteOutcome.Conflict);
        }
    }
    
    
}
