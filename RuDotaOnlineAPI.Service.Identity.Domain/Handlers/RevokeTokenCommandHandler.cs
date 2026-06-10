using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly ITokenService _tokens;

    public RevokeTokenCommandHandler(ITokenService tokens) => _tokens = tokens;

    public Task Handle(RevokeTokenCommand request, CancellationToken ct)
        => _tokens.RevokeAsync(request.RefreshToken, ct);
}