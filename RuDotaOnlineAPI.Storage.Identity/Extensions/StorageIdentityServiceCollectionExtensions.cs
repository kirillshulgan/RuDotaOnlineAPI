using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RuDotaOnlineAPI.Storage.Identity.Abstraction;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Aggregators;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Repositories;
using RuDotaOnlineAPI.Storage.Identity.Aggregators;
using RuDotaOnlineAPI.Storage.Identity.Repositories;

namespace RuDotaOnlineAPI.Storage.Identity.Extensions;

public static class StorageIdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var leader = configuration["ConnectionStrings:Leader"]
                     ?? configuration["ConnectionStrings:Postgres"]
                     ?? throw new InvalidOperationException("ConnectionStrings:Leader is missing.");
        var syncRead = configuration["ConnectionStrings:SyncRead"] ?? leader;
        var asyncRead = configuration["ConnectionStrings:AsyncRead"] ?? leader;

        // Leader — запись
        services.AddDbContext<IdentityUnitOfWork>(opts =>
            opts.UseNpgsql(leader,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(IdentityUnitOfWork).Assembly.FullName)));

        // SyncRead replica
        services.AddDbContext<IdentityReadUnitOfWork>(opts =>
            opts.UseNpgsql(syncRead));

        // AsyncRead replica
        services.AddDbContext<IdentityAsyncReadUnitOfWork>(opts =>
            opts.UseNpgsql(asyncRead));

        // Интерфейсы UoW
        services.AddScoped<IIdentityUnitOfWork>(sp =>
            sp.GetRequiredService<IdentityUnitOfWork>());
        services.AddScoped<IIdentityReadUnitOfWork>(sp =>
            sp.GetRequiredService<IdentityReadUnitOfWork>());
        services.AddScoped<IIdentityAsyncReadUnitOfWork>(sp =>
            sp.GetRequiredService<IdentityAsyncReadUnitOfWork>());

        // Репозитории
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

        // Агрегаторы
        services.AddScoped<IApplicationUserAggregator, ApplicationUserAggregator>();

        return services;
    }
}