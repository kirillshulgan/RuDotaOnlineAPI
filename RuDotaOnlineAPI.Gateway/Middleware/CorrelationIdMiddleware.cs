namespace RuDotaOnlineAPI.Gateway.Middleware;

/// <summary>
/// Генерирует или прокидывает X-Correlation-Id через всю цепочку.
/// Если клиент передал заголовок — используем его значение.
/// Иначе генерируем новый GUID. Кладём в HttpContext.Items —
/// UserContextTransformProvider достаёт оттуда и добавляет в downstream-запрос.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items[Header] = correlationId;

        // Возвращаем ID клиенту в ответе
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[Header] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}