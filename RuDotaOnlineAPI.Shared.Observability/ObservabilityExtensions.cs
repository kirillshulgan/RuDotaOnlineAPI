using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RuDotaOnlineAPI.Shared.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["Observability:ServiceName"]
            ?? throw new InvalidOperationException(
                "Observability:ServiceName is not configured.");

        var serviceVersion = typeof(ObservabilityExtensions)
            .Assembly.GetName().Version?.ToString() ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)
                .AddAttributes([
                    KeyValuePair.Create<string, object>(
                        "deployment.environment",
                        configuration["ASPNETCORE_ENVIRONMENT"] ?? "production")
                ]))
            .WithTracing(tracing => tracing
                // TracerProviderBuilder — здесь всё корректно
                .AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health") &&
                        !ctx.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation(opts =>
                {
                    opts.RecordException = true;
                })
                .AddSource("MassTransit"))
            .WithMetrics(metrics => metrics
                // MeterProviderBuilder — методы из OpenTelemetry.Instrumentation.AspNetCore 1.15.x
                .AddAspNetCoreInstrumentation()
                // Метод из OpenTelemetry.Instrumentation.Http 1.15.x
                .AddHttpClientInstrumentation()
                // Метод из OpenTelemetry.Instrumentation.Runtime 1.12.x
                .AddRuntimeInstrumentation()
                // Prometheus scraping endpoint
                .AddPrometheusExporter());

        return services;
    }
}