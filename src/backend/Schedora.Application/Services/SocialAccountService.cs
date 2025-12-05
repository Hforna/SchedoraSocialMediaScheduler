using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;

namespace Schedora.Application.Services;

public interface ISocialAccountService
{
    public Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto);
}

public class SocialAccountService : ISocialAccountService
{
    public SocialAccountService(ILogger<ISocialAccountService> logger, ITokenService tokenService, 
        IMapper mapper, IUnitOfWork uow, 
        ILinkedInService linkedInService, ISocialAccountProducer socialAccountProducer)
    {
        _logger = logger;
        _tokenService = tokenService;
        _mapper = mapper;
        _uow = uow;
        _linkedInService = linkedInService;
        _socialAccountProducer = socialAccountProducer; 
    }

    private readonly ILogger<ISocialAccountService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ILinkedInService  _linkedInService;
    private readonly ISocialAccountProducer _socialAccountProducer;
    
    public async Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto)
    {
        var socialUserInfos = await _linkedInService.GetSocialAccountInfos(dto.AccessToken, "Bearer");
        
        var user = await _uow.UserRepository.UserByEmail(socialUserInfos.Email) 
                   ?? throw new NotFoundException("The email provided by external service was not found in application");
        
        var socialAccount = new SocialAccount()
        {
            UserId = user.Id,
            AccessToken = dto.AccessToken,
            RefreshToken = dto.RefreshToken,
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(dto.ExpiresIn),
            Scopes = dto.Scopes,
            TokenType = "Bearer",
            IsActive = true,
            ConnectedAt = DateTime.UtcNow,
            LastSyncAt = DateTime.UtcNow,
            Platform = SocialPlatformsNames.LinkedIn,
            PlatformUserId = socialUserInfos.UserId,
            UserName = socialUserInfos.UserName,
            ProfileImageUrl = socialUserInfos.ProfileImageUrl,
            FollowerCount = socialUserInfos.FollowerCount,
        };
        if (dto.RefreshTokenExpiresIn != 0)
            socialAccount.LastTokenRefreshAt = DateTime.UtcNow.AddMinutes(dto.RefreshTokenExpiresIn);
        
        await _uow.GenericRepository.Add<SocialAccount>(socialAccount);
        await _uow.Commit();

        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, user.Id);
        await _socialAccountProducer.SendAccountConnected(producerDto);
    }
}