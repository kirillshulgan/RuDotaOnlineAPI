using Microsoft.AspNetCore.RateLimiting;
using RuDotaOnlineAPI.Gateway.Configuration;
using System.Net;
using System.Threading.RateLimiting;

namespace RuDotaOnlineAPI.Gateway.Extensions;

public static class RateLimitingExtensions
{
    public const string AnonymousPolicy = "anonymous-sliding";
    public const string AuthenticatedPolicy = "authenticated-sliding";

    public static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiterOptions =>
        {
            // Анонимные запросы — лимит по IP-адресу
            limiterOptions.AddSlidingWindowLimiter(AnonymousPolicy, policy =>
            {
                policy.PermitLimit = options.AnonymousSlidingWindow.PermitLimit;
                policy.Window = TimeSpan.FromSeconds(options.AnonymousSlidingWindow.WindowSeconds);
                policy.SegmentsPerWindow = options.AnonymousSlidingWindow.SegmentsPerWindow;
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                policy.QueueLimit = options.AnonymousSlidingWindow.QueueLimit;
            });

            // Авторизованные запросы — отдельный bucket на каждого пользователя по sub
            limiterOptions.AddPolicy(AuthenticatedPolicy, context =>
            {
                var userId = context.User.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: $"user:{userId}",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = options.AuthenticatedSlidingWindow.PermitLimit,
                            Window = TimeSpan.FromSeconds(options.AuthenticatedSlidingWindow.WindowSeconds),
                            SegmentsPerWindow = options.AuthenticatedSlidingWindow.SegmentsPerWindow,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = options.AuthenticatedSlidingWindow.QueueLimit
                        });
                }

                // Fallback на IP если claim отсутствует
                var ip = context.Connection.RemoteIpAddress ?? IPAddress.Loopback;
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: $"ip:{ip}",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = options.AnonymousSlidingWindow.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.AnonymousSlidingWindow.WindowSeconds),
                        SegmentsPerWindow = options.AnonymousSlidingWindow.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiterOptions.OnRejected = async (ctx, cancellationToken) =>
            {
                ctx.HttpContext.Response.Headers.RetryAfter = "60";
                await ctx.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "Too many requests. Please retry after 60 seconds." },
                    cancellationToken);
            };
        });

        return services;
    }
}