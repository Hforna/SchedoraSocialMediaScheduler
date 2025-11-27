global using AutoMapper;
global using Schedora.Application.Requests;
global using Schedora.Application.Responses;
global using Schedora.Domain.Entities;

namespace Schedora.Application;

public class ConfigurationMapperService : Profile
{
    public ConfigurationMapperService()
    {
        RequestToEntity();
        EntityToResponse();
    }

    void RequestToEntity()
    {
        CreateMap<UserRegisterRequest, User>()
            .ForMember(d => d.PasswordHash, f => f.Ignore());
    }

    void EntityToResponse()
    {
        CreateMap<User, UserResponse>();
    }
}