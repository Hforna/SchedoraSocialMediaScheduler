using Microsoft.AspNetCore.Mvc.Testing;
using Schedora.Application.Requests;

namespace Schedora.IntegrationTests.Controllers;

public class AuthControllerTests
{
    private WebApplicationTests _httpClient;
    
    [Fact]
    public async Task RegisterUser_EmailExists_ShouldThrowRequestException()
    {
        var request = new UserRegisterRequest()
        {
            Email = 
        }
    }
}