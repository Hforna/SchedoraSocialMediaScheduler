using Bogus;
using Schedora.Domain.Dtos;

namespace Schedora.UnitTests.Fakers.Dtos;

public static class ExternalServicesTokensDtoFaker
{
    public static ExternalServicesTokensDto Generate()
    {
        return new Faker<ExternalServicesTokensDto>()
            .RuleFor(d => d.AccessToken, f => f.Random.Word())
            .RuleFor(d => d.ExpiresIn, f => 60)
            .RuleFor(d => d.Scopes, f => f.Random.Words(5))
            .RuleFor(d => d.TokenType, "Bearer");
    }
}