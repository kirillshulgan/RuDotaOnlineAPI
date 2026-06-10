using Scalar.AspNetCore;

namespace RuDotaOnlineAPI.Gateway.Extensions;

public static class OpenApiExtensions
{
    public static void MapGatewayOpenApi(this WebApplication app)
    {
        // Gateway не генерирует собственную схему.
        // Scalar подключается к схемам downstream сервисов,
        // проксируемым через YARP маршрут openapi-identity.
        app.MapScalarApiReference(options =>
        {
            options.Title = "RuDota.Online API";
            options.Theme = ScalarTheme.DeepSpace;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);

            // При добавлении нового сервиса — добавить строку здесь
            // и YARP-маршрут openapi-{service} в appsettings.json
            options.AddDocument("Identity API", "/openapi/identity/openapi/v1.json");
        });
    }
}