using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedora.Application.Services;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services;
using Schedora.Domain.Services.Cache;
using Schedora.UnitTests.Fakers.Dtos;
using Schedora.UnitTests.Mocks;

namespace Schedora.UnitTests.Services;

public class SocialAccountServiceTests
{
    private ISocialAccountService _socialAccountService;
    private Mock<ILogger<ISocialAccountService>> _logger;
    private Mock<ITokenService> _tokenService;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _uow;
    private Mock<ILinkedInService> _linkedInService;
    private Mock<ISocialAccountProducer> _socialAccountProducer;
    private Mock<ISocialAccountCache> _accountCache;

    public SocialAccountServiceTests()
    {
        _logger = new Mock<ILogger<ISocialAccountService>>();
        _tokenService = new Mock<ITokenService>();
        _mapper = new Mock<IMapper>();
        _uow = new UnitOfWorkMock().GetMock();
        _linkedInService = new Mock<ILinkedInService>();
        _socialAccountProducer = new Mock<ISocialAccountProducer>();
        _accountCache = new Mock<ISocialAccountCache>();

        _socialAccountService = new SocialAccountService(
            _logger.Object,
            _tokenService.Object,
            _mapper.Object,
            _uow.Object,
            _linkedInService.Object,
            _socialAccountProducer.Object,
            _accountCache.Object
        );
    }

    [Fact]
    public async Task UserEmailNotFound_Should_ThrowNotFoundException()
    {
        var socialTokensDto = ExternalServicesTokensDtoFaker.Generate();
        var socialInfos = SocialAccountInfosDtoFaker.Generate();
        
        _linkedInService.Setup(d => d.GetSocialAccountInfos(socialTokensDto.AccessToken,  socialTokensDto.TokenType))
            .ReturnsAsync(socialInfos);
        
        var state = Guid.NewGuid().ToString();
        var result = async () => await _socialAccountService.ConfigureOAuthTokensFromLinkedin(socialTokensDto, state);

        await result.Should().ThrowAsync<NotFoundException>("The email provided by external service was not found in application");
    }
}