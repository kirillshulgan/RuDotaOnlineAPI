using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<TokenPairModel>;