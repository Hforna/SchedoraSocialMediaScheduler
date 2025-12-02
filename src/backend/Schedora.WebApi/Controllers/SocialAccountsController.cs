using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Schedora.Application.Services;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;

namespace Schedora.WebApi.Controllers;

[Route("api/social-accounts")]
[ApiController]
[Authorize]
public class SocialAccountsController : ControllerBase
{
    private readonly ISocialAccountService _socialAccountService;
    
    public SocialAccountsController(ISocialAccountService socialAccountService)
    {
        _socialAccountService = socialAccountService;
    }

    [HttpPost("connect/{platform}")]
    public async Task<IActionResult> ConnectPlatform([FromRoute]string platform, [FromQuery]string redirectUrl, 
        [FromServices]IEnumerable<IExternalAuthenticationService> externalAuthenticationService)
    {
        var service = externalAuthenticationService.
            FirstOrDefault(d => d.Platform.Equals(platform, StringComparison.InvariantCultureIgnoreCase));

        if (service is null)
            throw new RequestException("Invalid platform name");
        
        var result = await service.GetOAuthRedirectUrl(redirectUrl);

        return Challenge(new AuthenticationProperties() { RedirectUri = result });
    }
}