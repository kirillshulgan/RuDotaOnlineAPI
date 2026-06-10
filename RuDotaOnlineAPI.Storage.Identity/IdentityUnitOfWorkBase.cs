using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.Identity.Abstraction;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.Identity;

/// <summary>
/// Базовый write-контекст для Identity.
/// Наследует IdentityDbContext — содержит все ASP.NET Identity таблицы.
/// Наследует UnitOfWork (SDK) — содержит SaveChangesAsync + BeginTransactionAsync.
///
/// Двойное наследование разрешается через UnitOfWork → DbContext → IdentityDbContext:
/// IdentityDbContext сам наследует DbContext, поэтому цепочка:
///   IdentityUnitOfWorkBase → IdentityDbContext&lt;ApplicationUser, IdentityRole&lt;Guid&gt;, Guid&gt;
///                          → DbContext ← UnitOfWork тоже наследует DbContext.
/// Решение: UnitOfWork наследует DbContext, а здесь мы наследуем IdentityDbContext
/// и дублируем логику UnitOfWork через интерфейс IIdentityUnitOfWork.
/// </summary>
public abstract class IdentityUnitOfWorkBase
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
      IIdentityUnitOfWork
{
    protected IdentityUnitOfWorkBase(DbContextOptions options) : base(options) { }

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

        // MassTransit Outbox таблицы
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await Database.BeginTransactionAsync(ct);
        return new IdentityEfCoreTransaction(tx);
    }
}