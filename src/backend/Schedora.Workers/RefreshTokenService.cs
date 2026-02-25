using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;

namespace Schedora.Workers;

public class RefreshTokenService
{
    public RefreshTokenService(ILogger<RefreshTokenService> logger, IUnitOfWork uow, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _uow = uow;
        _serviceProvider = serviceProvider;
    }

    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IServiceProvider _serviceProvider;

    public async Task RegenerateTwitterTokens()
    {
        using var scope = _serviceProvider.CreateScope();

        var twitterTokensService = scope.ServiceProvider
            .GetRequiredService<IOAuthTokenService>();

        var cryptographyService = scope.ServiceProvider.GetRequiredService<ITokensCryptographyService>();
        
        var socialAccounts = await _uow.SocialAccountRepository.GetAllTwitterSocialAccounts();

        if (!socialAccounts.Any())
            return;

        foreach (var socialAccount in socialAccounts)
        {
            if (!string.IsNullOrEmpty(socialAccount.RefreshToken))
            {
                var decrypt = cryptographyService.DecryptToken(socialAccount.RefreshToken);
                var tokens = await twitterTokensService.RefreshToken(decrypt);
                
                socialAccount.AccessToken = cryptographyService.EncryptToken(tokens.AccessToken);
                socialAccount.RefreshToken = cryptographyService.EncryptToken(tokens.RefreshToken);
                socialAccount.LastTokenRefreshAt = DateTime.UtcNow;
            }
        }
        _uow.GenericRepository.UpdateRange<SocialAccount>(socialAccounts);
        await _uow.Commit();
    }
    
    public async Task RegenerateTokens()
    {
        using var scope = _serviceProvider.CreateScope();

        var tokensService = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IOAuthTokenService>>();

        var cryptographyService = scope.ServiceProvider.GetRequiredService<ITokensCryptographyService>();
        
        var socialAccounts = await _uow.SocialAccountRepository.GetAllSocialAccounts();

        if (!socialAccounts.Any())
            return;

        foreach (var socialPlatforms in socialAccounts)
        {
            var platformTokenService = tokensService.FirstOrDefault(d => d.Platform.Equals(socialPlatforms.Key));
            foreach (var socialAccount in socialPlatforms.Value)
            {
                if (!string.IsNullOrEmpty(socialAccount.RefreshToken))
                {
                    var decrypt = cryptographyService.DecryptToken(socialAccount.RefreshToken);
                    var tokens = await platformTokenService.RefreshToken(decrypt);
                
                    socialAccount.AccessToken = cryptographyService.EncryptToken(tokens.AccessToken);
                    socialAccount.RefreshToken = cryptographyService.EncryptToken(tokens.RefreshToken);
                    socialAccount.LastTokenRefreshAt = DateTime.UtcNow;
                    
                    _uow.GenericRepository.Update<SocialAccount>(socialAccount);
                }   
            }
        }
        await _uow.Commit();
    }
}