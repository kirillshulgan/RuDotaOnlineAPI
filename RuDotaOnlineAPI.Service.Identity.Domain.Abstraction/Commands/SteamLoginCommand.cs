using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

public sealed record SteamLoginCommand(
    IReadOnlyDictionary<string, string> CallbackParams) : IRequest<TokenPairModel>;