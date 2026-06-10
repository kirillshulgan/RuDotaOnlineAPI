using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RuDotaOnlineAPI.Service.Identity.API.Extensions;
using RuDotaOnlineAPI.Service.Identity.Domain;
using RuDotaOnlineAPI.Shared.Observability;
using RuDotaOnlineAPI.Storage.Identity;
using RuDotaOnlineAPI.Storage.Identity.Extensions;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.WithMachineName()
       .Enrich.WithEnvironmentName()
       .WriteTo.Console(outputTemplate:
           "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ── Storage ───────────────────────────────────────────────────────────────────
builder.Services.AddIdentityStorage(builder.Configuration);

// ── Domain ────────────────────────────────────────────────────────────────────
builder.Services.AddIdentityDomain(builder.Configuration);

// ── API (Controllers, Auth, OpenAPI) ─────────────────────────────────────────
builder.Services.AddIdentityApi(builder.Configuration);

// ── Observability ─────────────────────────────────────────────────────────────
builder.Services.AddObservability(builder.Configuration);

// ── MassTransit + Outbox ──────────────────────────────────────────────────────
builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();

    cfg.AddEntityFrameworkOutbox<IdentityUnitOfWork>(outbox =>
    {
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    });

    cfg.UsingRabbitMq((ctx, rmq) =>
    {
        rmq.Host(
            builder.Configuration["RabbitMq:Host"] ?? "localhost",
            builder.Configuration["RabbitMq:VirtualHost"] ?? "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
        rmq.ConfigureEndpoints(ctx);
    });
});

// ── Health Checks ─────────────────────────────────────────────────────────────
var rabbitUri =
    $"amqp://{builder.Configuration["RabbitMq:Username"]}:" +
    $"{builder.Configuration["RabbitMq:Password"]}" +
    $"@{builder.Configuration["RabbitMq:Host"]}" +
    $"/{Uri.EscapeDataString(builder.Configuration["RabbitMq:VirtualHost"] ?? "/")}";

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration["ConnectionStrings:Leader"]
            ?? builder.Configuration["ConnectionStrings:Postgres"]!,
        tags: ["ready"])
    .AddRedis(
        builder.Configuration["ConnectionStrings:Redis"]!,
        tags: ["ready"])
    .AddRabbitMQ(
        factory: sp => sp.GetRequiredService<IConnection>(),
        tags: ["ready"]);

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityUnitOfWork>();
    await db.Database.MigrateAsync();
}

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseIdentityApi();

// ── Metrics endpoint (Prometheus scrape) ─────────────────────────────────────
app.MapPrometheusScrapingEndpoint("/metrics");

// ── Health ────────────────────────────────────────────────────────────────────
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteJson
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready"),
    ResponseWriter = WriteJson
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteJson
});

app.Run();

static Task WriteJson(HttpContext ctx, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    return ctx.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.TotalMilliseconds,
        })
    }));
}