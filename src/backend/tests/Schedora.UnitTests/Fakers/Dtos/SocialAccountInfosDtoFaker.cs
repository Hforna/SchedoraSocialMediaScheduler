using Bogus;
using Schedora.Domain.Dtos;

namespace Schedora.UnitTests.Fakers.Dtos;

public static class SocialAccountInfosDtoFaker
{
    public static SocialAccountInfosDto Generate()
    {
        return new Faker<SocialAccountInfosDto>()
            .RuleFor(d => d.Email, f => f.Internet.Email())
            .RuleFor(d => d.FirstName, f => f.Name.FirstName())
            .RuleFor(d => d.LastName, f => f.Name.LastName())
            .RuleFor(d => d.UserName, f => f.Internet.UserName())
            .RuleFor(d => d.UserId, d => Guid.NewGuid().ToString());
    }
}