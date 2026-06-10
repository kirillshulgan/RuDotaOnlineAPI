using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<TokenPairModel>;