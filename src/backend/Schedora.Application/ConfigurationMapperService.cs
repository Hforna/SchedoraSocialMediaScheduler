global using AutoMapper;
global using Schedora.Application.Requests;
global using Schedora.Application.Responses;
global using Schedora.Domain.Entities;
using Schedora.Domain.Dtos;

namespace Schedora.Application;

public class ConfigurationMapperService : Profile
{
    public ConfigurationMapperService()
    {
        RequestToEntity();
        EntityToResponse();
        DtoMaps();
    }

    void RequestToEntity()
    {
        CreateMap<UserRegisterRequest, User>()
            .ForMember(d => d.PasswordHash, f => f.Ignore());

        CreateMap<UpdateAddressRequest, Address>();
    }

    void DtoMaps()
    {
        CreateMap<Address, UserAddressDto>();
    }

    void EntityToResponse()
    {
        CreateMap<User, UserResponse>();
        CreateMap<SocialAccount, SocialAccountResponse>();
        CreateMap<Address, AddressResponse>();
    }
}