using MediatR;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<UserModel>;