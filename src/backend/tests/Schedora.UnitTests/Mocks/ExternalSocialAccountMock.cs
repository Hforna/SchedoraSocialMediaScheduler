using Bogus;
using Moq;
using Schedora.Domain.Services;

namespace Schedora.UnitTests.Mocks;

public static class ExternalSocialAccountMock
{
    public static Mock<ISocialOAuthAuthenticationService> GenerateMock()
    {
        return new Mock<ISocialOAuthAuthenticationService>();
    }
}