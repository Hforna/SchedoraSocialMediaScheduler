using Bogus;
using Schedora.Domain.Entities;

namespace Schedora.UnitTests.Fakers.Entities;

public static class UserEntityFaker
{
    public static User Generate()
    {
        return new Faker<User>()
            .RuleFor(d => d.FirstName, f => f.Name.FirstName())
            .RuleFor(d => d.LastName, f => f.Name.LastName())
            .RuleFor(d => d.Email, f => f.Internet.Email())
            .RuleFor(d => d.EmailConfirmed, f => f.Random.Bool())
            .RuleFor(d => d.PasswordHash, f => f.Internet.Password())
            .RuleFor(d => d.UserName, f => f.Person.FullName);
    }
}