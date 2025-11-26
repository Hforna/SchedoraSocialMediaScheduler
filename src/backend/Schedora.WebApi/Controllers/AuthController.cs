using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;
using Schedora.WebApi.Extensions;

namespace Schedora.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, LinkGenerator linkGenerator, ILogger<AuthController> logger)
    {
        _authService = authService;
        _linkGenerator = linkGenerator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody]UserRegisterRequest request)
    {
        var confirmEmailEndpoint = _linkGenerator.GetPathByName(HttpContext, "ConfirmEmail");
        var uri = $"{HttpContext.GetBaseUri()}{confirmEmailEndpoint[1..]}";
        _logger.LogInformation("Email uri to confirm email {uri}", uri);
        var result = await _authService.RegisterUser(request, uri);

        return Created(string.Empty, result);
    }

    [HttpGet("confirm/email")]
    [EndpointName("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        await _authService.ConfirmEmail(email, token);

        return Ok();
    }
}