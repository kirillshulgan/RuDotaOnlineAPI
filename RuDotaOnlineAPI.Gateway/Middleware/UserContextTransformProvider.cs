using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace RuDotaOnlineAPI.Gateway.Middleware;

/// <summary>
/// YARP ITransformProvider — добавляет заголовки пользовательского контекста
/// во все форвардируемые запросы. Применяется глобально ко всем маршрутам.
///
/// Downstream сервисы читают заголовки напрямую, JWT не валидируют повторно:
///   X-User-Id       — claim "sub" (GUID пользователя)
///   X-User-Role     — роли через запятую
///   X-Correlation-Id — сквозной ID из CorrelationIdMiddleware
/// </summary>
public sealed class UserContextTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            // CorrelationId — всегда, независимо от аутентификации
            if (httpContext.Items.TryGetValue("X-Correlation-Id", out var corrObj)
                && corrObj is string correlationId)
            {
                transformContext.ProxyRequest.Headers.Remove("X-Correlation-Id");
                transformContext.ProxyRequest.Headers
                    .TryAddWithoutValidation("X-Correlation-Id", correlationId);
            }

            if (httpContext.User.Identity?.IsAuthenticated != true)
                return ValueTask.CompletedTask;

            // X-User-Id
            var userId = httpContext.User.FindFirst("sub")?.Value
                      ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.Remove("X-User-Id");
                transformContext.ProxyRequest.Headers
                    .TryAddWithoutValidation("X-User-Id", userId);
            }

            // X-User-Role — все роли через запятую
            var roles = httpContext.User
                .FindAll("role")
                .Concat(httpContext.User.FindAll(ClaimTypes.Role))
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            if (roles.Length > 0)
            {
                transformContext.ProxyRequest.Headers.Remove("X-User-Role");
                transformContext.ProxyRequest.Headers
                    .TryAddWithoutValidation("X-User-Role", string.Join(",", roles));
            }

            return ValueTask.CompletedTask;
        });
    }
}