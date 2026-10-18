
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
    
    public DbSet<PortalAccountEntity> PortalAccounts =>
        Set<PortalAccountEntity>();

    public DbSet<PortalAccountCategoryEntity> PortalAccountCategories =>
        Set<PortalAccountCategoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortalAccountEntity>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id).HasMaxLength(100);
            entity.Property(a => a.Username).HasMaxLength(100).IsRequired();
            entity.Property(a => a.DisplayName).HasMaxLength(150).IsRequired();
            entity.Property(a => a.PasswordHash).IsRequired();
            entity.Property(a => a.Role).HasMaxLength(30).IsRequired();
            entity.Property(a => a.SecurityStamp).HasMaxLength(100).IsRequired();

            entity.HasIndex(a => a.Username).IsUnique();
        });

        modelBuilder.Entity<PortalAccountCategoryEntity>(entity =>
        {
            entity.HasKey(c => new { c.AccountId, c.CategoryId });

            entity.Property(c => c.AccountId).HasMaxLength(100);
            entity.Property(c => c.CategoryId).HasMaxLength(100);

            entity.HasOne(c => c.Account)
                .WithMany(a => a.Categories)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
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
            
            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.Property(r => r.Priority)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Normal");

            entity.Property(r => r.OwnerId)
                .HasMaxLength(100);

            entity.Property(r => r.DueAt);

            entity.Property(r => r.SubmissionKey);

            entity.HasIndex(r => new { r.RequesterId, r.SubmissionKey })
                .IsUnique()
                .HasFilter("\"SubmissionKey\" IS NOT NULL");

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
            
            entity.Property(n => n.AuthorId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(n => n.CreatedAt)
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
