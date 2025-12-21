using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Schedora.Application.Services;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Domain.Services.Session;
using Schedora.WebApi.Extensions;

namespace Schedora.WebApi.Controllers;

[Route("api/social-accounts")]
[ApiController]
public class SocialAccountsController : ControllerBase
{
    private readonly ISocialAccountService _socialAccountService;
    private readonly ILogger<SocialAccountsController> _logger;
    private readonly LinkGenerator _linkGenerator;
    private readonly IUserSession _userSession;
    
    public SocialAccountsController(ISocialAccountService socialAccountService, LinkGenerator linkGenerator, 
        ILogger<SocialAccountsController> logger, IUserSession  userSession)
    {
        _userSession = userSession;
        _socialAccountService = socialAccountService;
        _logger = logger;
        _linkGenerator = linkGenerator;
    }
    
    /// <summary>
    /// Returns a uri to twitter oauth authorization requesting user permissions for app access on behalf
    /// </summary>
    /// <param name="platform">the platform to connect the social account, meanwhile linkedin or twitter</param>
    /// <param name="redirectUrl">redirect url after the user authorize the platform access</param>
    /// <returns>returns the url to platform oauth authorization page</returns>
    [HttpGet("connect/{platform}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConnectPlatform([FromRoute]string platform, [FromQuery]string redirectUrl, 
        [FromServices]IEnumerable<IExternalOAuthAuthenticationService> externalAuthenticationService)
    {
        await _socialAccountService.UserCanConnectSocialAccount(platform);
        
        var service = externalAuthenticationService.
            FirstOrDefault(d => d.Platform.Equals(platform, StringComparison.InvariantCultureIgnoreCase));

        if (service is null)
            throw new RequestException("Invalid platform name");
        
        var baseUri = HttpContext.GetBaseUri();
        baseUri = "https://4fdc66fbbe74.ngrok-free.app/";
        var endpointName = "";
        endpointName = service.Platform switch
        {
            SocialPlatformsNames.LinkedIn => "LinkedInCallback",
            SocialPlatformsNames.Twitter => "TwitterCallback",
            _ => throw new InternalServiceException("Error while trying to get the callback endpoint")
        };
        var callbackEndpoint = _linkGenerator.GetPathByName(HttpContext, endpointName);
        var callbackUrl = $"{baseUri}{callbackEndpoint![1..]}";
        
        var result = await service.GetOAuthRedirectUrl(redirectUrl, callbackUrl);

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
        
        var stateResponse = await _socialAccountService.GetStateResponse(state, SocialPlatformsNames.LinkedIn);

        var tokensResult = await externalService!.RequestTokensFromOAuthPlatform(code, stateResponse.RedirectUrl);
        await _socialAccountService.ConfigureOAuthTokensFromLinkedin(tokensResult, state);
        
        return Ok(stateResponse.RedirectUrl);
     }
    
    [HttpGet("twitter/callback")]
    [EndpointName("TwitterCallback")]
    public async Task<IActionResult> TwitterCallback([FromQuery]string state, [FromQuery]string code)
    {
        var baseUri = HttpContext.GetBaseUri();
        baseUri = "https://4fdc66fbbe74.ngrok-free.app/";
        var callbackEndpoint = _linkGenerator.GetPathByName(HttpContext, "TwitterCallback");
        var redirectUri = $"{baseUri}{callbackEndpoint![1..]}";

        await _socialAccountService.ConfigureOAuthTokensFromOAuthTwitter(state, code, redirectUri);
        
        var stateResponse = await _socialAccountService.GetStateResponse(state, SocialPlatformsNames.Twitter);
        
        return Ok(stateResponse.RedirectUrl);
    }
}