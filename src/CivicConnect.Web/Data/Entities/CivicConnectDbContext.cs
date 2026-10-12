
using CivicConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public sealed class CivicConnectDbContext : DbContext
{
    public CivicConnectDbContext(
        DbContextOptions<CivicConnectDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceRequestEntity> ServiceRequests =>
        Set<ServiceRequestEntity>();

    public DbSet<RequestNoteEntity> RequestNotes =>
        Set<RequestNoteEntity>();
    public DbSet<RequestStatusHistoryEntity> RequestStatusHistory =>
        Set<RequestStatusHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequestEntity>(entity =>
        {
            entity.HasKey(r => r.RequestId);

            entity.Property(r => r.ReferenceNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(r => r.ReferenceNumber)
                .IsUnique();

            entity.Property(r => r.RequesterId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(r => r.CategoryId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(r => r.Description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(r => r.Location)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(r => r.UpdatedAt)
                .IsRequired();
            
            // Detect when another operation has updated this request.
            entity.Property(r => r.Version)
                .IsRequired()
                .IsConcurrencyToken()
                .HasDefaultValue(1L);

            entity.HasMany(r => r.Notes)
                .WithOne(n => n.ServiceRequest)
                .HasForeignKey(n => n.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestNoteEntity>(entity =>
        {
            entity.HasKey(n => n.NoteId);

            entity.Property(n => n.Content)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(n => n.IsRequesterVisible)
                .IsRequired();
        });
        
        
        modelBuilder.Entity<RequestStatusHistoryEntity>(entity =>
        {
            entity.HasKey(h => h.HistoryId);

            entity.Property(h => h.PreviousStatus)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(h => h.NewStatus)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(h => h.ChangedByUserId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(h => h.ChangedAt)
                .IsRequired();

            entity.HasOne(h => h.ServiceRequest)
                .WithMany()
                .HasForeignKey(h => h.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new
            {
                h.RequestId,
                h.ChangedAt
            });
        });

    }
    
    public override int SaveChanges()
    {
        IncrementRequestVersions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        IncrementRequestVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void IncrementRequestVersions()
    {
        foreach (var entry in ChangeTracker
                     .Entries<ServiceRequestEntity>()
                     .Where(e => e.State == EntityState.Modified))
        {
            var originalVersion = entry.Property(r => r.Version).OriginalValue;

            entry.Property(r => r.Version).CurrentValue =
                checked(originalVersion + 1);
        }
    }

}
