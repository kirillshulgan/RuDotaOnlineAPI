using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace RuDotaOnlineAPI.Gateway.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwksUri = configuration["Jwt:JwksUri"]
            ?? throw new InvalidOperationException("Jwt:JwksUri is not configured.");

        // Derive readiness URL from JWKS URI
        var identityBaseUrl = jwksUri.Replace("/.well-known/jwks.json", string.Empty);

        services
            .AddHealthChecks()
            .AddUrlGroup(
                uri: new Uri($"{identityBaseUrl}/health/ready"),
                name: "identity-api",
                failureStatus: HealthStatus.Degraded,
                tags: ["ready"]);

        return services;
    }

    public static void MapGatewayHealthChecks(this WebApplication app)
    {
        // Liveness — только живой ли процесс, без внешних зависимостей
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = _ => false,
            ResponseWriter = WriteJsonResponse
        });

        // Readiness — готов ли принимать трафик (проверяет downstream)
        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse
        });

        // Полный статус
        app.MapHealthChecks("/health", new()
        {
            ResponseWriter = WriteJsonResponse
        });
    }

    private static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        return context.Response.WriteAsync(result);
    }
}