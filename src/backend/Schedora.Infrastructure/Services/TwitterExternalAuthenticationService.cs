using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class TwitterExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TwitterExternalAuthenticationService> _logger;
    private readonly ICryptographyService _cryptographyService;
    private readonly string _secretKey;
    
    public TwitterExternalAuthenticationService(IServiceProvider serviceProvider, ILogger<TwitterExternalAuthenticationService> logger, 
        IConfiguration configuration, ICryptographyService cryptographyService)
    {

        _serviceProvider = serviceProvider;
        _logger = logger;
        _cryptographyService = cryptographyService;
        
        string consumerKey = configuration.GetValue<string>("Twitter:ConsumerKey");
        string consumerSecret = configuration.GetValue<string>("Twitter:ConsumerSecret");

        var concatenateKeys = $"{consumerKey}:{consumerSecret}";
        _secretKey = ReplaceSymbolsFromBase64(Convert.ToBase64String(Encoding.UTF8.GetBytes(concatenateKeys)));
    }


    public string Platform { get; } = "twitter";

    public async Task<string> GetOAuthRedirectUrl(string uri)
    {
        using var scope = _serviceProvider.CreateScope();
        using var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var state = GenerateRandomString();
        var codeChallenge = _cryptographyService.CryptographyPasswordAs256Hash(GenerateRandomString());
        
        var result = await client.GetAsync($"https://x.com/i/oauth2/authorize?" +
                                           $"response_type=code&client_id={_secretKey}&" +
                                           $"redirect_uri={uri}&" +
                                           $"scope=tweet.read%20users.read%20account.follows.read%20account.follows.write&" +
                                           $"state={state}&" +
                                           $"code_challenge={codeChallenge}&" +
                                           $"code_challenge_method=S256");

        try
        {
            result.EnsureSuccessStatusCode();
            
            var content = await result.Content.ReadAsStringAsync();
            var url = JsonSerializer.Deserialize<string>(content);
            
            if(string.IsNullOrEmpty(url))
                throw new ExternalServiceException("challenge url not provided on content");
            
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            throw;
        }
    }

    private string ReplaceSymbolsFromBase64(string base64)
    {
        var sb = new StringBuilder();
        sb.Append(base64);
        sb.Replace('+', '-');
        sb.Replace('/', '_');
        sb.Replace('=', '-');
        
        return sb.ToString();
    }

    private static string GenerateRandomString()
    {
        var acceptedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        Random random = new Random();
        StringBuilder sb = new StringBuilder(255);

        for (int i = 0; i < 255; i++)
        {
            int index = random.Next(0, acceptedChars.Length);
            sb.Append(acceptedChars[index]);
        }

        return sb.ToString();
    }
}