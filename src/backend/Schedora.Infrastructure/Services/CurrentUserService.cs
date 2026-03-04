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
        // TODO: In the future this should come from user preferences or a geo-location service
        // based on the incoming request (IP, headers, etc.). For now, we default to UTC,
        // which is a valid system timezone identifier and keeps scheduling logic stable.
        return TimeZoneInfo.Utc.Id;
    }
}