using MediatR;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

public sealed record RevokeTokenCommand(
    string RefreshToken) : IRequest;