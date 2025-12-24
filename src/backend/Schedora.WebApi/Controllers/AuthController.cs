using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Application.Services;
using Schedora.WebApi.Extensions;
using Schedora.WebApi.Helpers;
using Schedora.WebApi.RequestExamples;
using Swashbuckle.AspNetCore.Filters;

namespace Schedora.WebApi.Controllers;

/// <summary>
/// Handles authentication and user identity lifecycle operations.
/// </summary>
/// <remarks>
/// Exposes endpoints for registering users, confirming email, logging in,
/// requesting password reset, resetting password, refreshing tokens, and logging out.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ILinkHelper _linkHelper;

    public AuthController(IAuthService authService, IConfiguration configuration, 
        LinkGenerator linkGenerator, ILogger<AuthController> logger, ILinkHelper  linkHelper)
    {
        _authService = authService;
        _linkHelper = linkHelper;
        _linkGenerator = linkGenerator;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user and sends an email confirmation link.
    /// </summary>
    /// <param name="request">User registration payload.</param>
    /// <returns>Created result with registration output.</returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Invalid registration payload.</response>
    [HttpPost("register")]
    [SwaggerRequestExample(typeof(UserRegisterRequest), typeof(UserRegisterRequestExample))]
    public async Task<IActionResult> RegisterUser([FromBody]UserRegisterRequest request)
    {
        var confirmEmailEndpoint = _linkGenerator.GetPathByName(HttpContext, "ConfirmEmail");
        var uri = $"{_configuration.GetValue<string>("appConfigs:appUrl")}{confirmEmailEndpoint![1..]}";
        _logger.LogInformation("Email uri to confirm email {uri}", uri);
        var result = await _authService.RegisterUser(request, uri);
        result.Links = new List<LinkResponse>()
        {
            _linkHelper.GenerateLinkResponse("UpdateUserInfos", "update", HttpMethods.Put),
            _linkHelper.GenerateLinkResponse("GetUserInfos", "self", HttpMethods.Get),
        };

        return Created(string.Empty, result);
    }

    /// <summary>
    /// Confirms a user's email using the confirmation token.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="token">Email confirmation token.</param>
    /// <returns>OK if confirmation succeeds.</returns>
    /// <response code="200">Email confirmed.</response>
    /// <response code="400">Invalid token or email.</response>
    [HttpGet("confirm/email")]
    [EndpointName("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        await _authService.ConfirmEmail(email, token);

        return Ok();
    }

    /// <summary>
    /// Authenticates a user using email and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Authentication tokens and expiration.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    public async Task<IActionResult> LoginByApplication([FromBody]LoginRequest request)
    {
        var result = await _authService.LoginByApplication(request);

        return Ok(result);
    }

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <param name="request">Email address used for password recovery.</param>
    /// <returns>OK if reset request is accepted.</returns>
    /// <response code="200">Reset email sent.</response>
    /// <response code="400">Invalid email.</response>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> UserForgotPasswordRequest([FromBody]ForgotPasswordRequest request)
    {
        var resetEndpoint = _linkGenerator.GetPathByName(HttpContext, "ResetPassword");
        
        await _authService.ResetPasswordRequest(request.Email,  resetEndpoint!);

        return Ok();
    }

    /// <summary>
    /// Resets the user's password using a reset token.
    /// </summary>
    /// <param name="token">Password reset token.</param>
    /// <param name="email">User email address.</param>
    /// <param name="request">New password payload.</param>
    /// <returns>OK if password is reset.</returns>
    /// <response code="200">Password updated.</response>
    /// <response code="400">Invalid token or email.</response>
    [HttpPost("reset-password")]
    [EndpointName("ResetPassword")]
    public async Task<IActionResult> ResetUserPassword([FromQuery]string token, [FromQuery]string email, [FromBody]ResetPasswordRequest request)
    {
        await _authService.ResetUserPassword(email, token, request.Password);

        return Ok();
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token string.</param>
    /// <returns>New access and refresh tokens.</returns>
    /// <response code="200">Token refreshed.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody]string refreshToken)
    {
        var result = await _authService.RefreshToken(refreshToken);

        return Ok(result);
    }

    /// <summary>
    /// Logs out the current user and revokes authentication tokens.
    /// </summary>
    /// <returns>No content.</returns>
    /// <response code="204">Logout successful.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutUser()
    {
        await _authService.RevokeToken();
        await HttpContext.SignOutAsync();

        return NoContent();
    }
}