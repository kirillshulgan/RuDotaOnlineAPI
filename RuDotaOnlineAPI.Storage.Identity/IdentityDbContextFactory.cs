using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RuDotaOnlineAPI.Storage.Identity;

// Для EF CLI: dotnet ef migrations add Init --project Storage.Identity
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityUnitOfWork>
{
    public IdentityUnitOfWork CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityUnitOfWork>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=myapp_identity;Username=myapp;Password=myapp_dev_password",
                npgsql => npgsql.MigrationsAssembly(typeof(IdentityUnitOfWork).Assembly.FullName))
            .Options;
        return new IdentityUnitOfWork(options);
    }
}