using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenPairModel>
{
    private readonly ITokenService _tokens;

    public RefreshTokenCommandHandler(ITokenService tokens) => _tokens = tokens;

    public Task<TokenPairModel> Handle(RefreshTokenCommand request, CancellationToken ct)
        => _tokens.RefreshAsync(request.RefreshToken, ct);
}