using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

namespace RuDotaOnlineAPI.Service.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISteamAuthService _steam;

    public AuthController(IMediator mediator, ISteamAuthService steam)
    {
        _mediator = mediator;
        _steam = steam;
    }

    /// <summary>Регистрация по email + password.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<TokenPairModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Вход по email + password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<TokenPairModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Обновление access токена по refresh токену.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<TokenPairModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Отзыв refresh токена (logout).</summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeTokenCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Редирект на Steam OpenID.</summary>
    [HttpGet("steam")]
    [AllowAnonymous]
    public IActionResult SteamLogin()
    {
        var url = _steam.BuildRedirectUrl();
        return Redirect(url);
    }

    /// <summary>Callback от Steam после авторизации.</summary>
    [HttpGet("steam/callback")]
    [AllowAnonymous]
    [ProducesResponseType<TokenPairModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SteamCallback(CancellationToken ct)
    {
        var queryParams = Request.Query
            .ToDictionary(k => k.Key, v => v.Value.ToString());

        var result = await _mediator.Send(
            new SteamLoginCommand(queryParams), ct);

        return Ok(result);
    }

    /// <summary>JWKS endpoint для Gateway — публичный ключ RSA.</summary>
    [HttpGet("/.well-known/jwks.json")]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600)]
    public IActionResult Jwks([FromServices] IJwtService jwt)
        => Content(jwt.GetJwks(), "application/json");
}