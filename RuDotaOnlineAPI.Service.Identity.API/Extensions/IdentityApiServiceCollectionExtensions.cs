using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Cryptography;

namespace RuDotaOnlineAPI.Service.Identity.API.Extensions;

public static class IdentityApiServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Контроллеры
        services.AddControllers();

        // Versioning
        services.AddApiVersioning(opts =>
        {
            opts.DefaultApiVersion = new ApiVersion(1, 0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ReportApiVersions = true;
        });

        // JWT Bearer (для /users/me и /auth/revoke)
        var publicPem = configuration["Jwt:PublicKeyPem"];
        var rsa = RSA.Create(2048); // дефолтная пара если ключ не задан

        if (!string.IsNullOrWhiteSpace(publicPem)
            && publicPem.Contains("-----BEGIN ")
            && publicPem.Contains("-----END "))
        {
            rsa.ImportFromPem(publicPem.Replace("\\n", "\n").Trim());
        }
        // Иначе используем временный ключ — JWT валидация в dev без реального ключа не работает,
        // но приложение запустится и миграции пройдут

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidAlgorithms = ["RS256"],
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        // OpenAPI + Scalar
        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseIdentityApi(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}