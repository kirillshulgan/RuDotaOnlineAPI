using AutoMapper;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Mapping;

public sealed class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<ApplicationUser, UserModel>()
            .ForCtorParam("roles",
                opt => opt.MapFrom(_ => new List<string>()));
    }
}