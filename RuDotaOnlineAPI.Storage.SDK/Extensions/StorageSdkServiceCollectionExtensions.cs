using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK.Extensions;

public static class StorageSdkServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует три DbContext-а (Leader, SyncRead, AsyncRead) по строкам подключения.
    /// Вызывается из сервис-специфичного IServiceCollectionExtension.
    ///
    /// Пример:
    ///   services.AddUnitOfWork&lt;IdentityUnitOfWork,
    ///                           IdentityReadUnitOfWork,
    ///                           IdentityAsyncReadUnitOfWork&gt;(configuration);
    /// </summary>
    public static IServiceCollection AddUnitOfWork<TWrite, TRead, TAsyncRead>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ConnectionStrings")
        where TWrite : DbContext, IUnitOfWork
        where TRead : DbContext, IReadUnitOfWork
        where TAsyncRead : DbContext, IAsyncReadUnitOfWork
    {
        var leader = configuration[$"{sectionName}:Leader"]
            ?? configuration[$"{sectionName}:Postgres"]
            ?? throw new InvalidOperationException($"Connection string '{sectionName}:Leader' is missing.");

        var syncRead = configuration[$"{sectionName}:SyncRead"] ?? leader;
        var asyncRead = configuration[$"{sectionName}:AsyncRead"] ?? leader;

        services.AddDbContext<TWrite>(opts =>
            opts.UseNpgsql(leader,
                npgsql => npgsql.MigrationsAssembly(typeof(TWrite).Assembly.FullName)));

        services.AddDbContext<TRead>(opts =>
            opts.UseNpgsql(syncRead));

        services.AddDbContext<TAsyncRead>(opts =>
            opts.UseNpgsql(asyncRead));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TWrite>());

        return services;
    }
}