using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedora.Application.Services;
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
    private Mock<IUserSession> _userSession;
    private Mock<IEnumerable<IExternalOAuthAuthenticationService>> _externalOAuthAuthenticationServices;

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
        _externalOAuthAuthenticationServices = new Mock<IEnumerable<IExternalOAuthAuthenticationService>>();
        _oauthStateService = new Mock<IOAuthStateService>();
        _twitterService = new Mock<ITwitterService>();
        _cookiesService = new Mock<ICookiesService>();
        _tokensCryptography = new Mock<ITokensCryptographyService>();

        _service = new SocialAccountService(
            _logger.Object,
            _tokenService.Object,
            _mapper.Object,
            _uow.Object,
            _linkedInService.Object,
            _socialAccountProducer.Object,
            _oauthStateService.Object,
            _externalOAuthAuthenticationServices.Object,
            _userSession.Object,
            _twitterService.Object,
            _cookiesService.Object,
            _accountCache.Object,
            _tokensCryptography.Object
        );
    }

    [Fact]
    public async Task ConfigureOAuthLinkedIn_StateIsNull_ThrowsUnauthorizedException()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        
        var result = async () => await _service.ConfigureOAuthTokensFromLinkedin(tokensDto, state);
        
        await result.Should().ThrowAsync<UnauthorizedException>("Invalid state from query");
        _oauthStateService.Verify(d => d.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state));
    }

    [Fact]
    public async Task ConfigureOAuthLinkedIn_FailedToParseTokenResponse_ThrowsExternalServiceException()
    {
        var tokensDto = ExternalServicesTokensDtoFaker.Generate();
        var state = Guid.NewGuid().ToString();
        long id = 1;

        _oauthStateService.Setup(d => d.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state)).ReturnsAsync(1);

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

        _oauthStateService.Setup(d => d.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state)).ReturnsAsync(1);

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

        _oauthStateService.Setup(d => d.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state)).ReturnsAsync(1);

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

        _oauthStateService.Setup(d => d.GetUserIdByStateStoraged(SocialPlatformsNames.LinkedIn, state))
            .ReturnsAsync(userId);
    
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
}