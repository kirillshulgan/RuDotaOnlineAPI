using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RuDotaOnlineAPI.Storage.Identity.Abstraction;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Storage.Identity;

public abstract class IdentityReadUnitOfWorkBase
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
      IIdentityReadUnitOfWork
{
    protected IdentityReadUnitOfWorkBase(DbContextOptions options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.DisplayName).HasMaxLength(100);
            entity.Property(u => u.SteamId).HasMaxLength(50);
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            entity.HasIndex(u => u.SteamId)
                .IsUnique()
                .HasFilter("\"SteamId\" IS NOT NULL");
        });

        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}

public sealed class IdentityReadUnitOfWork(DbContextOptions<IdentityReadUnitOfWork> options)
    : IdentityReadUnitOfWorkBase(options), IIdentityReadUnitOfWork;

public sealed class IdentityAsyncReadUnitOfWork(DbContextOptions<IdentityAsyncReadUnitOfWork> options)
    : IdentityReadUnitOfWorkBase(options), IIdentityAsyncReadUnitOfWork;