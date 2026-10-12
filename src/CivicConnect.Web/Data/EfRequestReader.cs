using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public sealed class EfRequestReader : IRequestReader
{
    private readonly CivicConnectDbContext _dbContext;

    public EfRequestReader(CivicConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RequestSnapshot?> FindAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ServiceRequests
            .AsNoTracking()
            .Where(r => r.RequestId == id)
            .Select(r => new RequestSnapshot(
                r.RequestId,
                r.ReferenceNumber,
                r.RequesterId,
                r.CategoryId,
                r.Description,
                r.Location,
                r.Status,
                r.UpdatedAt,
                r.Notes
                    .Select(n => new RequestNote(
                        n.Content,
                        n.IsRequesterVisible))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}