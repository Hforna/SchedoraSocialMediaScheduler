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
        
        var socialAccounts = await _uow.SocialAccountRepository.GetAllTwitterSocialAccounts();

        if (!socialAccounts.Any())
            return;

        foreach (var socialAccount in socialAccounts)
        {
            if (string.IsNullOrEmpty(socialAccount.RefreshToken))
            {
                var tokens = await twitterTokensService.RefreshToken(socialAccount.RefreshToken!);
                
                socialAccount.AccessToken = tokens.AccessToken;
                socialAccount.RefreshToken = tokens.RefreshToken;
                socialAccount.LastTokenRefreshAt = DateTime.UtcNow;
            }
        }
        _uow.GenericRepository.UpdateRange<SocialAccount>(socialAccounts);
        await _uow.Commit();
    }
}