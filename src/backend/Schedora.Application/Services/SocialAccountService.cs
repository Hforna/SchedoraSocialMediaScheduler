using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services.Cache;
using Schedora.Domain.Services.Session;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface ISocialAccountService
{
    public Task<string> ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string state);
    public Task<string> ConfigureOAuthTokensFromOAuthTwitter(string state, string code, string callbackUri);
    public Task<StateResponseDto> GetStateResponse(string state, string platform);
    public Task<bool> UserCanConnectSocialAccount(string platform);
}

public class SocialAccountService : ISocialAccountService
{
    public SocialAccountService(ILogger<ISocialAccountService> logger, ITokenService tokenService, 
        IMapper mapper, IUnitOfWork uow, 
        ILinkedInService linkedInService, ISocialAccountProducer socialAccountProducer, 
        IOAuthStateService oauthStateService, IEnumerable<IExternalOAuthAuthenticationService>  externalOAuthAuthenticationService, 
        IUserSession userSession, ITwitterService twitterService, ICookiesService cookies, 
        ISocialAccountCache socialAccountCache, ITokensCryptographyService tokensCryptography, 
        ISocialAccountDomainService  socialAccountDomainService, IActivityLogService activityLogService)
    {
        _logger = logger;
        _cookiesService = cookies;
        _activityLogService = activityLogService;
        _userSession = userSession;
        _socialAccountCache = socialAccountCache;
        _socialAccountDomainService = socialAccountDomainService;
        _externalOAuthAuthenticationService = externalOAuthAuthenticationService;
        _oauthStateService = oauthStateService;
        _tokenService = tokenService;
        _mapper = mapper;
        _uow = uow;
        _tokensCryptography = tokensCryptography;
        _linkedInService = linkedInService;
        _socialAccountProducer = socialAccountProducer; 
        _twitterService = twitterService;
    }

    private readonly ILogger<ISocialAccountService> _logger;
    private readonly IActivityLogService _activityLogService;
    private readonly ICookiesService _cookiesService;
    private readonly ISocialAccountCache  _socialAccountCache;
    private readonly IUserSession _userSession;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ILinkedInService  _linkedInService;
    private readonly ITwitterService  _twitterService;
    private readonly IEnumerable<IExternalOAuthAuthenticationService> _externalOAuthAuthenticationService;
    private readonly ISocialAccountProducer _socialAccountProducer;
    private readonly IOAuthStateService _oauthStateService;
    private readonly ITokensCryptographyService  _tokensCryptography;
    private readonly ISocialAccountDomainService  _socialAccountDomainService;
    
    public async Task<string> ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string state)
    {
        var stateResponse = await GetStateResponse(state, SocialPlatformsNames.LinkedIn);
        var userId = stateResponse.UserId;
        
        var socialUserInfos = await _linkedInService.GetSocialAccountInfos(dto.AccessToken, "Bearer");
        
        var socialAccountExists = await _uow.SocialAccountRepository.SocialAccountLinkedToUserExists(userId, 
                        socialUserInfos.UserId,
                SocialPlatformsNames.LinkedIn);

        if (socialAccountExists)
            throw new UnauthorizedException("This account is already linked to this user");

        dto = SecureTokens(dto);
        var socialAccount = CreateSocialAccount(dto, socialUserInfos, userId, SocialPlatformsNames.LinkedIn);

        await _uow.GenericRepository.Add<SocialAccount>(socialAccount);
        await _uow.Commit();

        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, userId);
        await _socialAccountProducer.SendAccountConnected(producerDto);

        return stateResponse.RedirectUrl;
    }

    public async Task<string> ConfigureOAuthTokensFromOAuthTwitter(string state,  string code, string callbackUrl)
    {
        var oauthService =  _externalOAuthAuthenticationService
            .FirstOrDefault(d => d.Platform.Equals(SocialPlatformsNames.Twitter));

        if (oauthService is null)
        {
            _logger.LogError($"Oauth service {nameof(oauthService)} not found");
            
            throw new InternalServiceException($"Oauth service {nameof(oauthService)} not found");
        }

        var stateResponse = await GetStateResponse(state, SocialPlatformsNames.Twitter);
        var userId = stateResponse.UserId;
        
        var codeChallenge = await _socialAccountCache.GetCodeChallenge(userId, SocialPlatformsNames.Twitter);

        if (string.IsNullOrEmpty(codeChallenge))
            throw new UnauthorizedException("Code challenge is null");
        
        var tokensDto = await oauthService.RequestTokensFromOAuthPlatform(code, callbackUrl, codeChallenge);
        
        var socialInfos = await _twitterService.GetUserSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType);
        
        var socialAccountExists = await _uow.SocialAccountRepository.SocialAccountLinkedToUserExists(userId, 
            socialInfos.UserId,
            SocialPlatformsNames.Twitter);

        if (socialAccountExists)
            throw new UnauthorizedException("This account is already linked to this user");

        tokensDto = SecureTokens(tokensDto);
        var socialAccount = CreateSocialAccount(tokensDto, socialInfos, userId, SocialPlatformsNames.Twitter);
        
        await _uow.GenericRepository.Add<SocialAccount>(socialAccount);
        await _uow.Commit();
        
        await _activityLogService.LogAsync(userId,
            ActivityActions.SOCIAL_ACCOUNT_CONNECTED,
            nameof(SocialAccount),
            socialAccount.Id, new
            {
                Platform = $"{socialAccount.Platform}"
            }, true);
        
        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, userId);
        await _socialAccountProducer.SendAccountConnected(producerDto);

        return stateResponse.RedirectUrl;
    }

    public async Task<StateResponseDto> GetStateResponse(string state, string platform)
    {
        var stateDto = await _oauthStateService.GetStateStoraged(platform, state);

        if (stateDto is null)
            throw new UnauthorizedException("Invalid state from query");
        
        return stateDto;
    }

    public async Task<bool> UserCanConnectSocialAccount(string platform)
    {
        platform = SocialPlatformsNames.NormalizePlatform(platform);
        
        var user = await _tokenService.GetUserByToken();
        
        var userCanConnect = await _socialAccountDomainService.UserAbleToConnectAccount(user, platform);
        
        return userCanConnect;
    }

    private SocialAccount CreateSocialAccount(ExternalServicesTokensDto tokensDto,
        SocialAccountInfosDto accountInfosDto,
        long userId, string platform)
    {
        var socialAccount = SocialAccount.Create(userId, platform, accountInfosDto.UserId, accountInfosDto.UserName,
            tokensDto.TokenType, tokensDto.Scopes, tokensDto.AccessToken, 
            tokensDto.RefreshToken, DateTime.UtcNow.AddSeconds(tokensDto.ExpiresIn));
        
        socialAccount.FollowerCount = accountInfosDto.FollowersCount;
        socialAccount.ProfileImageUrl = !string.IsNullOrEmpty(accountInfosDto.PictureUrl) 
            ? accountInfosDto.PictureUrl
            : string.Empty;

        return socialAccount;
    }
    
    private ExternalServicesTokensDto SecureTokens(ExternalServicesTokensDto dto)
    {
        dto.AccessToken = _tokensCryptography.HashToken(dto.AccessToken);
        if(!string.IsNullOrEmpty(dto.RefreshToken))
            dto.RefreshToken = _tokensCryptography.HashToken(dto.RefreshToken);
        
        return dto;
    }
}