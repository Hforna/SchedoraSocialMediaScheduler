using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Schedora.Application.Requests;

namespace Schedora.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationTests>
{
    private WebApplicationTests _httpClient;
    
    public AuthControllerTests(WebApplicationTests httpClient) => _httpClient = httpClient;
    
    [Fact]
    public async Task RegisterUser_EmailExists_ShouldThrowRequestException()
    {
        var request = new UserRegisterRequest()
        {
            Email = "henriqueflashpingo@gmail.com",
            Password = "password123",
            FirstName = "Henrique",
            LastName = "Flash",
            UserName = "henriqueflashingo",
        };

        var client = _httpClient.CreateClient();
        
        var response = await client.PostAsJsonAsync("/api/auth/RegisterUser", request);
        
        
    }
}