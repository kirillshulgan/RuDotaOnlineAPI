using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Service.Identity.Domain.Behaviours;
using RuDotaOnlineAPI.Service.Identity.Domain.Mapping;
using RuDotaOnlineAPI.Service.Identity.Domain.Services;
using RuDotaOnlineAPI.Storage.Identity;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using StackExchange.Redis;

namespace RuDotaOnlineAPI.Service.Identity.Domain;

public static class IdentityDomainServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityDomain(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ASP.NET Core Identity поверх IdentityUnitOfWork
        services
            .AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireNonAlphanumeric = false;
                opts.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityUnitOfWork>()
            .AddDefaultTokenProviders();

        // MediatR + pipeline
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IdentityDomainServiceCollectionExtensions).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(
            typeof(IdentityDomainServiceCollectionExtensions).Assembly);

        // AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<IdentityMappingProfile>();
        });

        // Domain Services
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenCacheService, RefreshTokenCacheService>();
        services.AddScoped<ISteamAuthService, SteamAuthService>();

        // Redis
        var redisConnectionString = configuration["ConnectionStrings:Redis"]
            ?? throw new InvalidOperationException("ConnectionStrings:Redis is missing.");
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));

        // HttpClient для Steam
        services.AddHttpClient("steam");

        return services;
    }
}