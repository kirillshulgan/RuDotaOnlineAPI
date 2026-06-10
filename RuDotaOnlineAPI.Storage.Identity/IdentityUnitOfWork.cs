using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.Identity.Abstraction;

namespace RuDotaOnlineAPI.Storage.Identity;

public sealed class IdentityUnitOfWork(DbContextOptions<IdentityUnitOfWork> options)
    : IdentityUnitOfWorkBase(options), IIdentityUnitOfWork;