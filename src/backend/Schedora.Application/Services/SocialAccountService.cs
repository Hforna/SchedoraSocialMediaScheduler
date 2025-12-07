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
        ISocialAccountCache accountCache, IEnumerable<IExternalOAuthAuthenticationService>  externalOAuthAuthenticationService, 
        IUserSession userSession, ITwitterService twitterService, ICookiesService cookies)
    {
        _logger = logger;
        _cookiesService = cookies;
        _userSession = userSession;
        _externalOAuthAuthenticationService = externalOAuthAuthenticationService;
        _accountCache = accountCache;
        _tokenService = tokenService;
        _mapper = mapper;
        _uow = uow;
        _linkedInService = linkedInService;
        _socialAccountProducer = socialAccountProducer; 
        _twitterService = twitterService;
    }

    private readonly ILogger<ISocialAccountService> _logger;
    private readonly ICookiesService _cookiesService;
    private readonly IUserSession _userSession;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ILinkedInService  _linkedInService;
    private readonly ITwitterService  _twitterService;
    private readonly IEnumerable<IExternalOAuthAuthenticationService> _externalOAuthAuthenticationService;
    private readonly ISocialAccountProducer _socialAccountProducer;
    private readonly ISocialAccountCache _accountCache;
    
    public async Task ConfigureOAuthTokensFromLinkedin(ExternalServicesTokensDto dto, string state)
    {
        var socialUserInfos = await _linkedInService.GetSocialAccountInfos(dto.AccessToken, "Bearer");
        
        var user = await _uow.UserRepository.UserByEmail(socialUserInfos.Email) 
                   ?? throw new NotFoundException("The email provided by external service was not found in application");
        
        var cacheState = await _accountCache.GetStateAuthorization(user.Id, SocialPlatformsNames.LinkedIn);

        if (string.IsNullOrEmpty(cacheState) || cacheState != state)
            throw new UnauthorizedException("Invalid state from query");

        var socialAccountExists = await _uow.SocialAccountRepository.SocialAccountLinkedToUserExists(user.Id, 
                        socialUserInfos.UserId,
                SocialPlatformsNames.LinkedIn);

        if (socialAccountExists)
            throw new UnauthorizedException("This account is already linked to this user");
        
        var socialAccount = CreateSocialAccount(dto, socialUserInfos, user.Id, SocialPlatformsNames.LinkedIn);

        await _uow.GenericRepository.Add<SocialAccount>(socialAccount);
        await _uow.Commit();

        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, user.Id);
        await _socialAccountProducer.SendAccountConnected(producerDto);
    }

    public async Task ConfigureOAuthTokensFromOAuthTwitter(string state,  string code, string redirectUrl)
    {
        var oauthService =  _externalOAuthAuthenticationService
            .FirstOrDefault(d => d.Platform.Equals(SocialPlatformsNames.Twitter));

        var userId = _cookiesService.GetUserId() ?? throw new RequestException("It was not possible to get user id");
        
        var cacheState = await _accountCache.GetStateAuthorization(userId,  SocialPlatformsNames.Twitter);
        if(string.IsNullOrEmpty(cacheState) || cacheState != state)
            throw new UnauthorizedException("Invalid state from query");
        
        var codeChallenge = await _accountCache.GetCodeChallenge(userId, SocialPlatformsNames.Twitter);
        
        var tokensDto = await oauthService.RequestTokensFromOAuthPlatform(code, redirectUrl, codeChallenge);

        var socialInfos = await _twitterService.GetUserSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType);
        
        var socialAccountExists = await _uow.SocialAccountRepository.SocialAccountLinkedToUserExists(userId, 
            socialInfos.UserId,
            SocialPlatformsNames.LinkedIn);

        if (socialAccountExists)
            throw new UnauthorizedException("This account is already linked to this user");
        
        var socialAccount = CreateSocialAccount(tokensDto, socialInfos, userId, SocialPlatformsNames.Twitter);
        
        await _uow.GenericRepository.Add<SocialAccount>(socialAccount);
        await _uow.Commit();
        
        var producerDto = new SocialAccountConnectedDto(socialAccount.Id, userId);
        await _socialAccountProducer.SendAccountConnected(producerDto);
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
}