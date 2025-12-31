using Schedora.Domain.Entities;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ITokenService  _tokenService;
    private readonly IRequestService _requestService;

    public CurrentUserService(ITokenService tokenService, IRequestService requestService)
    {
        _tokenService = tokenService;
        _requestService = requestService;
    }

    public async Task<User?> GetUser()
    {
        return await _tokenService.GetUserByToken();
    }

    public string GetCurrentUserTimeZone()
    {
        //use a geo location api to get user timezone
        return "to be implemented";
    }
}