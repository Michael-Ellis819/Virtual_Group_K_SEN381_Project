
using CivicConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public sealed class EfRequestStatusWriter(
    CivicConnectDbContext dbContext)
{
    public async Task SaveStatusChangeAsync(
        Guid requestId,
        string expectedCurrentStatus,
        string newStatus,
        string changedByUserId,
        string feedback,
        CancellationToken cancellationToken = default)
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

        if (request.Status != expectedCurrentStatus)
        {
            throw new DbUpdateConcurrencyException(
                "The request status has changed.");
        }

        request.Status = newStatus;
        request.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.RequestStatusHistory.Add(
            new RequestStatusHistoryEntity
            {
                HistoryId = Guid.NewGuid(),
                RequestId = requestId,
                PreviousStatus = expectedCurrentStatus,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            });

        dbContext.RequestNotes.Add(
            new RequestNoteEntity
            {
                NoteId = Guid.NewGuid(),
                RequestId = requestId,
                Content = feedback,
                IsRequesterVisible = true
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
