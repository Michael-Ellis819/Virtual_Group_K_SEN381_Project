
using CivicConnect.Data.Entities;
using CivicConnect.Application;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Lifecycle;

namespace CivicConnect.Data;

public sealed class EfRequestStatusWriter(
    CivicConnectDbContext dbContext,
    Lifecycle.IOwnershipRule ownershipRule) : IRequestStatusWriter
{
    public Task SaveAsync(StatusChange change, CancellationToken cancellationToken)
        => SaveStatusChangeAsync(change.RequestId, change.PreviousStatus, change.NewStatus,
            change.ActorId, change.Feedback, cancellationToken, change.ExpectedVersion);

    public async Task SaveStatusChangeAsync(
        Guid requestId,
        string expectedCurrentStatus,
        string newStatus,
        string changedByUserId,
        string feedback,
        CancellationToken cancellationToken = default,
        long? expectedVersion = null)
    {
        if (requestId == Guid.Empty ||
            string.IsNullOrWhiteSpace(expectedCurrentStatus) ||
            string.IsNullOrWhiteSpace(newStatus) ||
            string.IsNullOrWhiteSpace(changedByUserId) ||
            string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException(
                "Status change information is incomplete.");
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var request = await dbContext.ServiceRequests
            .SingleOrDefaultAsync(
                r => r.RequestId == requestId,
                cancellationToken);

        if (request is null)
        {
            throw new InvalidOperationException(
                "Request was not found.");
        }

        if (request.Status != expectedCurrentStatus ||
            (expectedVersion.HasValue && request.Version != expectedVersion.Value))
        {
            throw new DbUpdateConcurrencyException(
                "The request status has changed.");
        }

        var decision = new CivicConnect.Lifecycle.StatusTransitionService().Evaluate(
            new(expectedCurrentStatus, newStatus, changedByUserId, feedback, request.OwnerId));
        if (!decision.Allowed || newStatus == "Assigned")
            throw new ArgumentException("Use an allowed transition; first ownership requires the assignment command.");
        var now = DateTimeOffset.UtcNow;
        request.Status = decision.Change!.NewStatus;
        request.UpdatedAt = now;

        dbContext.RequestStatusHistory.Add(
            new RequestStatusHistoryEntity
            {
                HistoryId = Guid.NewGuid(),
                RequestId = requestId,
                PreviousStatus = expectedCurrentStatus,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                ChangedAt = now
            });

        dbContext.RequestNotes.Add(
            new RequestNoteEntity
            {
                NoteId = Guid.NewGuid(),
                RequestId = requestId,
                Content = feedback,
                IsRequesterVisible = true,
                AuthorId = changedByUserId,
                CreatedAt = now
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    
    public async Task SaveAssignmentAsync(
        Guid requestId,
        string ownerId,
        long expectedVersion,
        Lifecycle.ValidatedStatusChange change,
        CancellationToken cancellationToken,
        string actorId)
    {
        if (requestId == Guid.Empty ||
            string.IsNullOrWhiteSpace(ownerId) ||
            expectedVersion < 1 ||
            change.PreviousStatus != "New" ||
            change.NewStatus != "Assigned" ||
            string.IsNullOrWhiteSpace(change.ActorId))
        {
            throw new ArgumentException("Invalid assignment.");
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var request = await dbContext.ServiceRequests
            .SingleOrDefaultAsync(
                r => r.RequestId == requestId,
                cancellationToken);

        if (request is null ||
            request.Version != expectedVersion ||
            request.Status != change.PreviousStatus ||
            !string.IsNullOrEmpty(request.OwnerId))
        {
            throw new DbUpdateConcurrencyException(
                "The request has changed or is already assigned.");
        }
        
        if (actorId != change.ActorId)
        {
            throw new DbUpdateConcurrencyException(
                "The assignment actor does not match the approved change.");
        }

        var actor = await dbContext.PortalAccounts
            .AsNoTracking()
            .Include(a => a.Categories)
            .SingleOrDefaultAsync(a => a.Id == actorId, cancellationToken);

        if (actor is null ||
            !actor.IsActive ||
            actor.Role != "Staff" ||
            !actor.Categories.Any(c => c.CategoryId == request.CategoryId))
        {
            throw new DbUpdateConcurrencyException(
                "The acting staff member is no longer authorised.");
        }

        var owner = await dbContext.PortalAccounts
            .AsNoTracking()
            .Include(a => a.Categories)
            .SingleOrDefaultAsync(a => a.Id == ownerId, cancellationToken);

        if (owner is null)
        {
            throw new DbUpdateConcurrencyException(
                "The proposed owner no longer exists.");
        }

        var decision = ownershipRule.Evaluate(
            new Lifecycle.OwnerCandidate(
                owner.Id,
                owner.Role,
                owner.IsActive,
                owner.Categories.Select(c => c.CategoryId).ToArray()),
            new Lifecycle.AssignmentTarget(
                request.Status,
                request.CategoryId,
                request.OwnerId));

        if (!decision.ShouldAssign)
        {
            throw new DbUpdateConcurrencyException(
                "The proposed owner is no longer eligible.");
        }

        var now = DateTimeOffset.UtcNow;

        request.OwnerId = ownerId;
        request.Status = change.NewStatus;
        request.UpdatedAt = now;

        dbContext.RequestStatusHistory.Add(new RequestStatusHistoryEntity
        {
            HistoryId = Guid.NewGuid(),
            RequestId = requestId,
            PreviousStatus = change.PreviousStatus,
            NewStatus = change.NewStatus,
            ChangedByUserId = change.ActorId,
            ChangedAt = now
        });

        dbContext.RequestNotes.Add(new RequestNoteEntity
        {
            NoteId = Guid.NewGuid(),
            RequestId = requestId,
            Content = change.Feedback ?? "Request assigned.",
            IsRequesterVisible = true,
            AuthorId = change.ActorId,
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
