using Schedora.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace Schedora.WebApi.RequestExamples;

public class UserRegisterRequestExample : IExamplesProvider<UserRegisterRequest>
{
    public UserRegisterRequest GetExamples()
    {
        return new UserRegisterRequest()
        {
            UserName =  "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
            Password = "StrongPassword123!"
        };
    }
}

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest()
        {
            Email = "john.doe@email.com",
            Password = "StrongPassword123!"
        };
    }
}