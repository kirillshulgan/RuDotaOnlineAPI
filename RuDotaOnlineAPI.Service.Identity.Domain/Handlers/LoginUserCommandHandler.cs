using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, TokenPairModel>
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITokenService _tokens;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> users,
        ITokenService tokens,
        ILogger<LoginUserCommandHandler> logger)
    {
        _users = users;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<TokenPairModel> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var valid = await _users.CheckPasswordAsync(user, request.Password);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        var roles = await _users.GetRolesAsync(user);
        return await _tokens.GenerateTokenPairAsync(user, roles, ct);
    }
}