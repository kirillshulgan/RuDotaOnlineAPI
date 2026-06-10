using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? DisplayName) : IRequest<TokenPairModel>;