using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Services;

public sealed class JwtService : IJwtService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _ttlMinutes;

    public JwtService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"] ?? "dev-issuer";
        _audience = configuration["Jwt:Audience"] ?? "dev-audience";
        _ttlMinutes = int.Parse(configuration["Jwt:AccessTokenTtlMinutes"] ?? "15");

        var privatePem = configuration["Jwt:PrivateKeyPem"];
        var publicPem = configuration["Jwt:PublicKeyPem"];

        if (!IsValidPem(privatePem) || !IsValidPem(publicPem))
        {
            var rsa = RSA.Create(2048);
            _privateKey = new RsaSecurityKey(rsa) { KeyId = "dev-key" };
            _publicKey = new RsaSecurityKey(rsa) { KeyId = "dev-key" };
            return;
        }

        privatePem = NormalizePem(privatePem!);
        publicPem = NormalizePem(publicPem!);

        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(privatePem);
        _privateKey = new RsaSecurityKey(privateRsa) { KeyId = "identity-rsa-1" };

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(publicPem);
        _publicKey = new RsaSecurityKey(publicRsa) { KeyId = "identity-rsa-1" };
    }

    public string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName ?? string.Empty),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        if (user.SteamId is not null)
            claims.Add(new Claim("steam_id", user.SteamId));

        var credentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_ttlMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            IssuerSigningKey = _publicKey,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        try
        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public string GetJwks()
    {
        var parameters = _publicKey.Rsa.ExportParameters(false);
        var jwk = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = _publicKey.KeyId,
                    alg = "RS256",
                    n   = Base64UrlEncoder.Encode(parameters.Modulus!),
                    e   = Base64UrlEncoder.Encode(parameters.Exponent!),
                }
            }
        };
        return JsonSerializer.Serialize(jwk);
    }

    private static string NormalizePem(string pem) => pem.Replace("\\n", "\n").Replace("\\r\\n", "\n").Trim();

    private static bool IsValidPem(string? pem) => !string.IsNullOrWhiteSpace(pem) && pem.Contains("-----BEGIN ") && pem.Contains("-----END ");
}