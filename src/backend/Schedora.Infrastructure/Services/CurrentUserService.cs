using Schedora.Domain.Entities;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ITokenService  _tokenService;

    public CurrentUserService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<User?> GetUser()
    {
        return await _tokenService.GetUserByToken();
    }
}