using MediatR;
using Microsoft.AspNetCore.Identity;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Queries;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserModel>
{
    private readonly UserManager<ApplicationUser> _users;

    public GetCurrentUserQueryHandler(UserManager<ApplicationUser> users) => _users = users;

    public async Task<UserModel> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(request.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var roles = await _users.GetRolesAsync(user);

        return new UserModel(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.SteamId,
            user.AvatarUrl,
            (IReadOnlyList<string>)roles,
            user.CreatedAt,
            user.LastLoginAt);
    }
}