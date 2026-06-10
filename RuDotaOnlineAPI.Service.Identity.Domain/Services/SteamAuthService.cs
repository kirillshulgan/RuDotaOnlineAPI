using Microsoft.Extensions.Configuration;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using System.Text.RegularExpressions;
using System.Web;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Services;

public sealed partial class SteamAuthService : ISteamAuthService
{
    private const string SteamOpenIdUrl = "https://steamcommunity.com/openid/login";
    private readonly string _callbackUrl;
    private readonly string _apiKey;
    private readonly HttpClient _http;

    public SteamAuthService(IConfiguration configuration, IHttpClientFactory factory)
    {
        _callbackUrl = configuration["Steam:CallbackUrl"]
            ?? throw new InvalidOperationException("Steam:CallbackUrl missing.");
        _apiKey = configuration["Steam:ApiKey"]
            ?? throw new InvalidOperationException("Steam:ApiKey missing.");
        _http = factory.CreateClient("steam");
    }

    public string BuildRedirectUrl()
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        q["openid.ns"] = "http://specs.openid.net/auth/2.0";
        q["openid.mode"] = "checkid_setup";
        q["openid.return_to"] = _callbackUrl;
        q["openid.realm"] = new Uri(_callbackUrl).GetLeftPart(UriPartial.Authority);
        q["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select";
        q["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select";
        return $"{SteamOpenIdUrl}?{q}";
    }

    public async Task<SteamAuthModel> ValidateCallbackAsync(
        IReadOnlyDictionary<string, string> queryParams,
        CancellationToken ct = default)
    {
        // 1. Верифицируем подпись Steam
        var verifyParams = new Dictionary<string, string>(queryParams)
        {
            ["openid.mode"] = "check_authentication"
        };

        var response = await _http.PostAsync(
            SteamOpenIdUrl,
            new FormUrlEncodedContent(verifyParams), ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!body.Contains("is_valid:true"))
            throw new UnauthorizedAccessException("Steam OpenID validation failed.");

        // 2. Извлекаем SteamID из claimed_id
        var claimedId = queryParams["openid.claimed_id"];
        var steamId = SteamIdRegex().Match(claimedId).Groups[1].Value;

        if (string.IsNullOrEmpty(steamId))
            throw new UnauthorizedAccessException("Could not extract SteamID.");

        // 3. Запрашиваем профиль
        var profileUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/" +
                         $"?key={_apiKey}&steamids={steamId}";
        var profileJson = await _http.GetStringAsync(profileUrl, ct);

        using var doc = System.Text.Json.JsonDocument.Parse(profileJson);
        var player = doc.RootElement
            .GetProperty("response")
            .GetProperty("players")[0];

        return new SteamAuthModel(
            steamId,
            player.GetProperty("personaname").GetString() ?? steamId,
            player.TryGetProperty("avatarfull", out var av) ? av.GetString() : null);
    }

    [GeneratedRegex(@"\/id\/(\d+)$")]
    private static partial Regex SteamIdRegex();
}