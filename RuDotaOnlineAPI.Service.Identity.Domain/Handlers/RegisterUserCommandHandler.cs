using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, TokenPairModel>
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITokenService _tokens;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> users,
        ITokenService tokens,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _users = users;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<TokenPairModel> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var existing = await _users.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _users.AddToRoleAsync(user, "User");

        _logger.LogInformation("User registered: {UserId} ({Email})", user.Id, user.Email);

        var roles = await _users.GetRolesAsync(user);
        return await _tokens.GenerateTokenPairAsync(user, roles, ct);
    }
}