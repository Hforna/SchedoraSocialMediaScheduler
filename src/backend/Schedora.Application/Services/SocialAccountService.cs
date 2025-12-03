using Schedora.Domain.Dtos;

namespace Schedora.Application.Services;

public interface ISocialAccountService
{
    public Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string userEmail);
}

public class SocialAccountService : ISocialAccountService
{
    private readonly ILogger<ISocialAccountService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    
    public async Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string userEmail)
    {
        var user = await _uow.UserRepository.UserByEmail(userEmail);
        
        var socialAccount = new SocialAccount()
        {
            UserId = user.Id,
            AccessToken =  dto.AccessToken,
            RefreshToken = dto.RefreshToken,
            Platform = SocialPlatformsNames.LinkedIn,
            
        }
    }
}