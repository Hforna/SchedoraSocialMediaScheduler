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
public class SocialAccountsController : ControllerBase
{
    private readonly ISocialAccountService _socialAccountService;
    
    public SocialAccountsController(ISocialAccountService socialAccountService)
    {
        _socialAccountService = socialAccountService;
    }

    [HttpGet("connect/{platform}")]
    [Authorize]
    public async Task<IActionResult> ConnectPlatform([FromRoute]string platform, [FromQuery]string redirectUrl, 
        [FromServices]IEnumerable<IExternalOAuthAuthenticationService> externalAuthenticationService)
    {
        var service = externalAuthenticationService.
            FirstOrDefault(d => d.Platform.Equals(platform, StringComparison.InvariantCultureIgnoreCase));

        if (service is null)
            throw new RequestException("Invalid platform name");
        
        var result = await service.GetOAuthRedirectUrl(redirectUrl);

        return Ok(result);
    }

    [HttpGet("linkedin/callback")]
    public async Task<IActionResult> LinkedInCallback([FromQuery]string state, [FromQuery]string code,
        [FromServices]IEnumerable<IExternalOAuthAuthenticationService> externalAuthenticationService)
    {
        var externalService = externalAuthenticationService
            .FirstOrDefault(d => d.Platform.Equals("linkedin", StringComparison.InvariantCultureIgnoreCase));
        
        var tokensResult = await externalService!.RequestAccessFromOAuthPlatform(code, state);
        //TODO: validate state storaged on session
        
    }

    [HttpGet("twitter/callback")]
    public async Task<IActionResult> TwitterCallback()
    {
        return Ok();
    }
}