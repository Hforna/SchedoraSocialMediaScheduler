using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services.Cache;
using Schedora.Domain.Services.Session;

namespace Schedora.Application.Services;

public interface ISocialAccountService
{
    public Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string state);
    public Task ConfigureOAuthTokensFromOAuthTwitter(string state, string code, string redirectUrl);
}

public class SocialAccountService : ISocialAccountService
{
    public SocialAccountService(ILogger<ISocialAccountService> logger, ITokenService tokenService, 
        IMapper mapper, IUnitOfWork uow, 
        ILinkedInService linkedInService, ISocialAccountProducer socialAccountProducer, 
        IOAuthStateService oauthStateService, IEnumerable<IExternalOAuthAuthenticationService>  externalOAuthAuthenticationService, 
        IUserSession userSession, ITwitterService twitterService, ICookiesService cookies, 
        ISocialAccountCache socialAccountCache, ITokensCryptographyService tokensCryptography)
    {
        _logger = logger;
        _cookiesService = cookies;
        _userSession = userSession;
        _socialAccountCache = socialAccountCache;
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
    
    public async Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string state)
    {
        var userId = await GetUserIdByState(state);
        
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
    }

    public async Task ConfigureOAuthTokensFromOAuthTwitter(string state,  string code, string redirectUrl)
    {
        var oauthService =  _externalOAuthAuthenticationService
            .FirstOrDefault(d => d.Platform.Equals(SocialPlatformsNames.Twitter));

        var userId = await GetUserIdByState(state);
        
        var codeChallenge = await _socialAccountCache.GetCodeChallenge(userId, SocialPlatformsNames.Twitter);

        if (string.IsNullOrEmpty(codeChallenge))
            throw new InternalServiceException("Code challenge is null");
        
        var tokensDto = await oauthService.RequestTokensFromOAuthPlatform(code, redirectUrl, codeChallenge);
        
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
        
        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, userId);
        await _socialAccountProducer.SendAccountConnected(producerDto);
    }

    private async Task<long> GetUserIdByState(string state)
    {
        var userIdByState = await _oauthStateService.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state);

        if (userIdByState is null)
            throw new UnauthorizedException("Invalid state from query");
        
        return (long)userIdByState;
    }

    private SocialAccount CreateSocialAccount(ExternalServicesTokensDto tokensDto,
        SocialAccountInfosDto accountInfosDto,
        long userId, string platform)
    {
        var socialAccount = SocialAccount.Create(userId, platform, accountInfosDto.UserId, accountInfosDto.UserName,
            tokensDto.TokenType, tokensDto.Scopes, tokensDto.AccessToken, 
            tokensDto.RefreshToken, DateTime.UtcNow.AddMinutes(tokensDto.ExpiresIn));
        
        socialAccount.FollowerCount = accountInfosDto.FollowersCount;
        socialAccount.ProfileImageUrl = !string.IsNullOrEmpty(accountInfosDto.PictureUrl) 
            ? accountInfosDto.PictureUrl
            : string.Empty;

        return socialAccount;
    }
    
    private ExternalServicesTokensDto SecureTokens(ExternalServicesTokensDto dto)
    {
        dto.AccessToken = _tokensCryptography.HashToken(dto.AccessToken);
        dto.RefreshToken = _tokensCryptography.HashToken(dto.RefreshToken);
        return dto;
    }
}