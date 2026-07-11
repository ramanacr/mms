using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Data;

public sealed class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PendingAdminConsent> PendingAdminConsents => Set<PendingAdminConsent>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
    public DbSet<BookingSeries> BookingSeries => Set<BookingSeries>();
    public DbSet<BookingInstance> BookingInstances => Set<BookingInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.MicrosoftTenantId)
            .IsUnique();

        modelBuilder.Entity<PendingAdminConsent>()
            .HasKey(c => c.State);

        modelBuilder.Entity<PendingAdminConsent>()
            .HasIndex(c => new { c.ExpectedMicrosoftTenantId, c.UsedAt });

        modelBuilder.Entity<MeetingRoom>()
            .HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();

        modelBuilder.Entity<MeetingRoom>()
            .HasQueryFilter(r => r.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<BookingSeries>()
            .HasQueryFilter(b => b.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<BookingInstance>()
            .HasQueryFilter(b => b.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<BookingInstance>()
            .HasIndex(b => new { b.TenantId, b.RoomId, b.StartAt, b.EndAt });

        modelBuilder.Entity<BookingInstance>()
            .HasOne(b => b.Series)
            .WithMany(s => s.Instances)
            .HasForeignKey(b => b.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingSeries>()
            .HasOne(s => s.Room)
            .WithMany(r => r.BookingSeries)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BookingInstance>()
            .HasOne(i => i.Room)
            .WithMany(r => r.BookingInstances)
            .HasForeignKey(i => i.RoomId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIds();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTenantIds();
        return base.SaveChanges();
    }

    private void ApplyTenantIds()
    {
        if (!_tenantProvider.HasTenant)
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantProvider.TenantId;
            }
        }
    }
}
