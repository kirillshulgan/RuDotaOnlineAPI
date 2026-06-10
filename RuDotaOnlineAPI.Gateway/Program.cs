using RuDotaOnlineAPI.Gateway.Extensions;
using RuDotaOnlineAPI.Gateway.Middleware;
using RuDotaOnlineAPI.Shared.Observability;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting RuDotaOnlineAPI.Gateway");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} " +
            "{Message:lj} {Properties:j}{NewLine}{Exception}"));

    var configuration = builder.Configuration;

    // ── OpenTelemetry ─────────────────────────────────────────
    builder.Services.AddObservability(configuration);

    // ── YARP ──────────────────────────────────────────────────
    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(configuration.GetSection("ReverseProxy"))
        .AddTransforms<UserContextTransformProvider>();

    // ── Auth ──────────────────────────────────────────────────
    builder.Services.AddGatewayAuthentication(configuration);

    // ── Rate Limiting ─────────────────────────────────────────
    builder.Services.AddGatewayRateLimiting(configuration);

    // ── Health Checks ─────────────────────────────────────────
    builder.Services.AddGatewayHealthChecks(configuration);

    // ── HttpClient (для JWKS ConfigurationManager) ────────────
    builder.Services.AddHttpClient();

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────
    // Порядок критичен: CorrelationId → Logging → RateLimiting → Auth → Endpoints → YARP

    app.UseCorrelationId();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} " +
            "in {Elapsed:0.0}ms [CorrId={CorrelationId}]";
        opts.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("CorrelationId", ctx.Items["X-Correlation-Id"]?.ToString() ?? "-");
            diag.Set("RemoteIp", ctx.Connection.RemoteIpAddress?.ToString() ?? "-");
        };
    });

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Эти endpoints Gateway обслуживает сам — до YARP
    app.MapGatewayHealthChecks();
    app.MapGatewayOpenApi();
    app.MapPrometheusScrapingEndpoint("/metrics");

    // YARP — последний, форвардирует всё что не поймали выше
    app.MapReverseProxy();

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}