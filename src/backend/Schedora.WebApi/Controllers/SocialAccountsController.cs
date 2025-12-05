using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Schedora.Application.Services;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.WebApi.Extensions;

namespace Schedora.WebApi.Controllers;

[Route("api/social-accounts")]
[ApiController]
public class SocialAccountsController : ControllerBase
{
    private readonly ISocialAccountService _socialAccountService;
    private readonly ILogger<SocialAccountsController> _logger;
    private readonly LinkGenerator _linkGenerator;
    
    public SocialAccountsController(ISocialAccountService socialAccountService, LinkGenerator linkGenerator, 
        ILogger<SocialAccountsController> logger)
    {
        _socialAccountService = socialAccountService;
        _logger = logger;
        _linkGenerator = linkGenerator;
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
    [EndpointName("LinkedInCallback")]
    public async Task<IActionResult> LinkedInCallback([FromQuery]string state, [FromQuery]string code,
        [FromServices]IEnumerable<IExternalOAuthAuthenticationService> externalAuthenticationService)
    {
        var externalService = externalAuthenticationService
            .FirstOrDefault(d => d.Platform.Equals(SocialPlatformsNames.LinkedIn,
                StringComparison.InvariantCultureIgnoreCase));

        var baseUri = HttpContext.GetBaseUri();
        var callbackEndpoint = _linkGenerator.GetPathByName(HttpContext, "LinkedInCallback");
        var redirectUri = $"{HttpContext.GetBaseUri()}{callbackEndpoint![1..]}";
        
        var tokensResult = await externalService!.RequestAccessFromOAuthPlatform(code, redirectUri);
        
        await _socialAccountService.ConfigureOAuthTokensFromLinkedin(tokensResult, state);

        return Ok();
    }

    [HttpGet("twitter/callback")]
    public async Task<IActionResult> TwitterCallback()
    {
        return Ok();
    }
}