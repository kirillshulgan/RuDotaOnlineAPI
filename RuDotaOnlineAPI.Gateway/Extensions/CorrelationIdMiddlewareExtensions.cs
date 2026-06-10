using RuDotaOnlineAPI.Gateway.Middleware;

namespace RuDotaOnlineAPI.Gateway.Extensions;

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}