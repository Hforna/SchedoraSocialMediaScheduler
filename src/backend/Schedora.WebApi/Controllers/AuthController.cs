using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration, LinkGenerator linkGenerator, ILogger<AuthController> logger)
    {
        _authService = authService;
        _linkGenerator = linkGenerator;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody]UserRegisterRequest request)
    {
        var confirmEmailEndpoint = _linkGenerator.GetPathByName(HttpContext, "ConfirmEmail");
        var uri = $"{_configuration.GetValue<string>("appConfigs:appUrl")}{confirmEmailEndpoint![1..]}";
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

    [HttpPost("login")]
    public async Task<IActionResult> LoginByApplication([FromBody]LoginRequest request)
    {
        var result = await _authService.LoginByApplication(request);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> UserForgotPasswordRequest([FromBody]ForgotPasswordRequest request)
    {
        var resetPasswordEndpoint = _linkGenerator.GetPathByName(HttpContext, "ResetPassword");
        var uri = $"{_configuration.GetValue<string>("appConfigs:appUrl")}{resetPasswordEndpoint![1..]}";
        _logger.LogInformation("Email uri to confirm email {uri}", uri);
        await _authService.ResetPasswordRequest(request.Email, uri);

        return Ok();
    }

    [HttpPost("reset-password")]
    [EndpointName("ResetPassword")]
    public async Task<IActionResult> ResetUserPassword([FromQuery]string token, [FromQuery]string email, [FromBody]ResetPasswordRequest request)
    {
        await _authService.ResetUserPassword(email, token, request.Password);

        return Ok();
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody]string refreshToken)
    {
        var result = await _authService.RefreshToken(refreshToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutUser()
    {
        await _authService.RevokeToken();
        await HttpContext.SignOutAsync();

        return NoContent();
    }
    
    
}