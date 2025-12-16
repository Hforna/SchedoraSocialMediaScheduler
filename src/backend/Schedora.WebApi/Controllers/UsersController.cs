using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;

namespace Schedora.WebApi.Controllers;

/// <summary>
/// Controller responsible for managing authenticated user operations.
/// </summary>
/// <remarks>
/// All endpoints in this controller require an authenticated user.
/// It provides endpoints to retrieve and update user profile data,
/// manage address and password changes, and access subscription information.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize]
/// <summary>
/// Users API controller.
/// </summary>
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">Service responsible for user-related business logic.</param>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets information about the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Returns the authenticated user's profile information.
    /// </returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetUserAuthenticatedInfos()
    {
        var result = await _userService.GetUserAuthenticatedInfos();
        return Ok(result);
    }

    /// <summary>
    /// Updates the address information of the authenticated user.
    /// </summary>
    /// <param name="request">Request containing the new address data.</param>
    /// <returns>
    /// Returns the updated address information.
    /// </returns>
    [HttpPatch("address")]
    public async Task<IActionResult> UpdateAddress([FromBody]UpdateAddressRequest request)
    {
        var result = await _userService.UpdateAddress(request);
        
        return Ok(result);
    }

    /// <summary>
    /// Updates general information of the authenticated user.
    /// </summary>
    /// <param name="request">Request containing the user information to be updated.</param>
    /// <returns>
    /// Returns the updated user information.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> UpdateUserInfos([FromBody]UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserInfos(request);

        return Ok(result);
    }

    /// <summary>
    /// Updates the password of the authenticated user.
    /// </summary>
    /// <param name="request">Request containing the current and new password.</param>
    /// <returns>
    /// Returns an empty response when the password update succeeds.
    /// </returns>
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody]UpdatePasswordRequest request)
    {
        await _userService.UpdatePassword(request);

        return Ok();
    }

    /// <summary>
    /// Gets the subscription information of the authenticated user.
    /// </summary>
    /// <returns>
    /// Returns the user's current subscription details.
    /// </returns>
    [HttpGet("subscription")]
    public async Task<IActionResult> GetUserSubscription()
    {
        var result = await _userService.GetUserSubscription();

        return Ok(result);
    }
}