using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedora.Application.Services;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services;
using Schedora.Domain.Services.Cache;
using Schedora.Domain.Services.Session;
using Schedora.UnitTests.Fakers.Dtos;
using Schedora.UnitTests.Fakers.Entities;
using Schedora.UnitTests.Mocks;

namespace Schedora.UnitTests.Services;

public class SocialAccountServiceTests
{
    private ISocialAccountService _service;
    private Mock<ILogger<ISocialAccountService>> _logger;
    private Mock<ITokenService> _tokenService;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _uow;
    private Mock<ILinkedInService> _linkedInService;
    private Mock<ISocialAccountProducer> _socialAccountProducer;
    private Mock<ISocialAccountCache> _accountCache;
    private Mock<IOAuthStateService> _oauthStateService;
    private Mock<ITwitterService> _twitterService;
    private Mock<ICookiesService> _cookiesService;
    private Mock<ITokensCryptographyService> _tokensCryptography;
    private Mock<ISocialAccountDomainService> _socialAccountDomainService;
    private Mock<IUserSession> _userSession;
    private List<IExternalOAuthAuthenticationService> _externalOAuthAuthenticationServices;
    private Mock<IExternalOAuthAuthenticationService> _externalOAuthAuthenticationService;
    private Mock<IActivityLogService> _activityLogService;

    public SocialAccountServiceTests()
    {
        _logger = new Mock<ILogger<ISocialAccountService>>();
        _tokenService = new Mock<ITokenService>();
        _mapper = new Mock<IMapper>();
        _uow = new UnitOfWorkMock().GetMock();
        _linkedInService = new Mock<ILinkedInService>();
        _socialAccountProducer = new Mock<ISocialAccountProducer>();
        _accountCache = new Mock<ISocialAccountCache>();
        _userSession = new Mock<IUserSession>();
        _oauthStateService = new Mock<IOAuthStateService>();
        _twitterService = new Mock<ITwitterService>();
        _cookiesService = new Mock<ICookiesService>();
        _tokensCryptography = new Mock<ITokensCryptographyService>();
        _socialAccountDomainService = new Mock<ISocialAccountDomainService>();
        _externalOAuthAuthenticationService = new Mock<IExternalOAuthAuthenticationService>();
        _activityLogService = new Mock<IActivityLogService>();
        
        _externalOAuthAuthenticationServices = new List<IExternalOAuthAuthenticationService>() { _externalOAuthAuthenticationService.Object };

        _service = new SocialAccountService(
            _logger.Object,
            _tokenService.Object,
            _mapper.Object,
            _uow.Object,
            _linkedInService.Object,
            _socialAccountProducer.Object,
            _oauthStateService.Object,
            _externalOAuthAuthenticationServices,
            _userSession.Object,
            _twitterService.Object,
            _cookiesService.Object,
            _accountCache.Object,
            _tokensCryptography.Object,
            _socialAccountDomainService.Object,
            _activityLogService.Object
        );
    }

    #region ConfigureOAuthTokensFromLinkedIn
    [Fact]
    public async Task ConfigureOAuthLinkedIn_StateIsNull_ThrowsUnauthorizedException()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        
        var result = async () => await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        await result.Should().ThrowAsync<UnauthorizedException>("Invalid state from query");
        _oauthStateService.Verify(d => d.GetStateStoraged(SocialPlatformsNames.LinkedIn, state));
    }

    [Fact]
    public async Task ConfigureOAuthLinkedIn_FailedToParseTokenResponse_ThrowsExternalServiceException()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        long id = 1;

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.LinkedIn, state))
            .ReturnsAsync(new StateResponseDto() { UserId = id, RedirectUrl = "https://localhost"});

        _linkedInService
            .Setup(d => d.GetSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ThrowsAsync(new ExternalServiceException("Failed to parse token response"));
        
        var result = async () => await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        await result.Should().ThrowAsync<ExternalServiceException>("Failed to parse token response");
    }

    [Fact]
    public async Task ConfigureOAuthLinkedIn_SocialAccount_LinkedToUser_ThrowsUnauthorizedException()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        long id = 1;
        var socialAccountInfos = SocialAccountInfosDtoFaker.Generate();

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.LinkedIn, state))
            .ReturnsAsync(new StateResponseDto() { UserId = id, RedirectUrl = "https://localhost"});

        _linkedInService
            .Setup(d => d.GetSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ReturnsAsync(socialAccountInfos);

        _uow.Setup(d =>
                d.SocialAccountRepository.SocialAccountLinkedToUserExists(id, socialAccountInfos.UserId,
                    SocialPlatformsNames.LinkedIn))
            .ReturnsAsync(true);
        
        var result = async () => await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        await result.Should().ThrowAsync<UnauthorizedException>("This account is already linked to this user");
    }

    [Fact]
    public async Task ConfigureOAuthLinkedIn_ValidTokens_ShouldCreateSocialAccountSuccessfully()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        long id = 1;
        var socialAccountInfos = SocialAccountInfosDtoFaker.Generate();

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.LinkedIn, state))
            .ReturnsAsync(new StateResponseDto() { UserId = id, RedirectUrl = "https://localhost"});

        _linkedInService
            .Setup(d => d.GetSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ReturnsAsync(socialAccountInfos);

        _uow.Setup(d =>
                d.SocialAccountRepository.SocialAccountLinkedToUserExists(id, socialAccountInfos.UserId,
                    SocialPlatformsNames.LinkedIn))
            .ReturnsAsync(false);
        
        await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        _uow.Verify(d => d.Commit(), Times.Once);
        _socialAccountProducer.Verify(d => d.SendAccountConnected(It.IsAny<SocialAccountConnectedDto>()), Times.Once);
    }
    
    [Fact]
    public async Task ConfigureOAuthLinkedIn_ValidTokens_ShouldCreateSocialAccountWithCorrectData()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        long userId = 1;
        var socialAccountInfos = SocialAccountInfosDtoFaker.Generate();
        SocialAccount capturedAccount = null;

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.LinkedIn, state))
            .ReturnsAsync(new StateResponseDto() { UserId =  userId, RedirectUrl = "https://localhost"});
    
        _linkedInService.Setup(d => d.GetSocialAccountInfos(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(socialAccountInfos);
    
        _uow.Setup(d => d.SocialAccountRepository.SocialAccountLinkedToUserExists(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
    
        _uow.Setup(d => d.GenericRepository.Add<SocialAccount>(It.IsAny<SocialAccount>()))
            .Callback<SocialAccount>(account => capturedAccount = account);
        
        await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        capturedAccount.Should().NotBeNull();
        capturedAccount.UserId.Should().Be(userId);
        capturedAccount.Platform.Should().Be(SocialPlatformsNames.LinkedIn);
    }
    #endregion

    #region ConfigureOAuthTokensFromTwitter
    [Theory]
    [InlineData(" ")]
    [InlineData("invalid token")]
    public async Task ConfigureOAuthTokensFromTwitter_NullOrInvalidState_ShouldThrowUnauthrorizedException(string state)
    {
        var redirectUrl = "https://google.com";
        var code = "random code";
        
        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);
        
        var result = async () => await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);
        
        await result.Should().ThrowAsync<UnauthorizedException>("Invalid state from query");
        _oauthStateService.Verify(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state));
    }

    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_NullOAuthService_ShouldThrowInternalServiceException()
    {
        var redirectUrl = "https://google.com";
        var code = "invalid code";
        var state = "invalid state";
        
        _externalOAuthAuthenticationService
            .Setup(d => d.Platform)
            .Returns("SomeOtherPlatform");
        
        var result = async () => await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);

        await result.Should().ThrowAsync<InternalServiceException>($"Oauth service {SocialPlatformsNames.Twitter} not found");
        
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_InvalidCodeChallenge_ShouldThrowUnauthorizedException()
    {
        var redirectUrl = "https://google.com";
        var code = "invalid code";
        var state = "valid state";
        var userId = 1;

        var stateResponse = new StateResponseDto()
        {
            RedirectUrl = redirectUrl,
            UserId = userId
        };
        
        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state))
            .ReturnsAsync(stateResponse);
        
        var result = async () => await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);
        
        await result.Should().ThrowAsync<UnauthorizedException>("Invalid code challenge");

        _accountCache.Verify(d => d.GetCodeChallenge(userId, SocialPlatformsNames.Twitter));
    }
    
    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_ExternalServiceThrowsException_ShouldPropagateException()
    {
        var redirectUrl = "https://google.com";
        var code = "valid code";
        var state = "valid state";
        var userId = 1;

        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state))
            .ReturnsAsync(new StateResponseDto { RedirectUrl = redirectUrl, UserId = userId });

        _accountCache.Setup(d => d.GetCodeChallenge(userId, SocialPlatformsNames.Twitter))
            .ReturnsAsync("challenge");

        _externalOAuthAuthenticationService
            .Setup(d => d.RequestTokensFromOAuthPlatform(code, redirectUrl, It.IsAny<string>()))
            .ThrowsAsync(new ExternalServiceException("twitter error"));

        var result = async () => await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);

        await result.Should().ThrowAsync<ExternalServiceException>("twitter error");
    }

    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_SocialAccountAlreadyLinked_ShouldThrowUnauthorizedException()
    {
        var redirectUrl = "https://google.com";
        var code = "valid code";
        var state = "valid state";
        var userId = 1;

        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var socialInfos = SocialAccountInfosDtoFaker.Generate();

        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state))
            .ReturnsAsync(new StateResponseDto { RedirectUrl = redirectUrl, UserId = userId });

        _accountCache.Setup(d => d.GetCodeChallenge(userId, SocialPlatformsNames.Twitter))
            .ReturnsAsync("challenge");

        _externalOAuthAuthenticationService
            .Setup(d => d.RequestTokensFromOAuthPlatform(code, redirectUrl, It.IsAny<string>()))
            .ReturnsAsync(tokensDto);

        _twitterService
            .Setup(d => d.GetUserSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ReturnsAsync(socialInfos);

        _uow.Setup(d =>
                d.SocialAccountRepository.SocialAccountLinkedToUserExists(userId, socialInfos.UserId, SocialPlatformsNames.Twitter))
            .ReturnsAsync(true);

        var result = async () => await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);

        await result.Should().ThrowAsync<UnauthorizedException>("This account is already linked to this user");
    }

    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_ValidTokens_ShouldCreateSocialAccountSuccessfully()
    {
        var redirectUrl = "https://google.com";
        var code = "valid code";
        var state = "valid state";
        var userId = 1;

        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var socialInfos = SocialAccountInfosDtoFaker.Generate();

        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state))
            .ReturnsAsync(new StateResponseDto { RedirectUrl = redirectUrl, UserId = userId });

        _accountCache.Setup(d => d.GetCodeChallenge(userId, SocialPlatformsNames.Twitter))
            .ReturnsAsync("challenge");

        _externalOAuthAuthenticationService
            .Setup(d => d.RequestTokensFromOAuthPlatform(code, redirectUrl, It.IsAny<string>()))
            .ReturnsAsync(tokensDto);

        _twitterService
            .Setup(d => d.GetUserSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ReturnsAsync(socialInfos);

        _uow.Setup(d =>
                d.SocialAccountRepository.SocialAccountLinkedToUserExists(userId, socialInfos.UserId, SocialPlatformsNames.Twitter))
            .ReturnsAsync(false);

        await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);

        _uow.Verify(d => d.Commit(), Times.Once);
        _socialAccountProducer.Verify(d => d.SendAccountConnected(It.IsAny<SocialAccountConnectedDto>()), Times.Once);
    }

    [Fact]
    public async Task ConfigureOAuthTokensFromTwitter_ValidTokens_ShouldCreateSocialAccountWithCorrectData()
    {
        var redirectUrl = "https://google.com";
        var code = "valid code";
        var state = "valid state";
        var userId = 1;

        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var socialInfos = SocialAccountInfosDtoFaker.Generate();

        SocialAccount capturedAccount = null;

        _externalOAuthAuthenticationService.Setup(d => d.Platform).Returns(SocialPlatformsNames.Twitter);

        _oauthStateService.Setup(d => d.GetStateStoraged(SocialPlatformsNames.Twitter, state))
            .ReturnsAsync(new StateResponseDto { RedirectUrl = redirectUrl, UserId = userId });

        _accountCache.Setup(d => d.GetCodeChallenge(userId, SocialPlatformsNames.Twitter))
            .ReturnsAsync("challenge");

        _externalOAuthAuthenticationService
            .Setup(d => d.RequestTokensFromOAuthPlatform(code, redirectUrl, It.IsAny<string>()))
            .ReturnsAsync(tokensDto);

        _twitterService
            .Setup(d => d.GetUserSocialAccountInfos(tokensDto.AccessToken, tokensDto.TokenType))
            .ReturnsAsync(socialInfos);

        _uow.Setup(d =>
                d.SocialAccountRepository.SocialAccountLinkedToUserExists(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _uow.Setup(d => d.GenericRepository.Add<SocialAccount>(It.IsAny<SocialAccount>()))
            .Callback<SocialAccount>(account => capturedAccount = account);

        await _service.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUrl);

        capturedAccount.Should().NotBeNull();
        capturedAccount.UserId.Should().Be(userId);
        capturedAccount.Platform.Should().Be(SocialPlatformsNames.Twitter);
    }
    #endregion
}