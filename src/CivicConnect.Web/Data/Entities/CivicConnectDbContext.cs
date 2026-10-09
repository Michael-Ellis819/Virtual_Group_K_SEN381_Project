
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
    }
}
