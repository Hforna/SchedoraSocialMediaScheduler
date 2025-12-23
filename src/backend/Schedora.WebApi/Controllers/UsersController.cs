using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Application.Services;
using Schedora.WebApi.Helpers;

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
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILinkHelper _linkHelper;

    public UsersController(IUserService userService, ILinkHelper linkHelper)
    {
        _userService = userService;
        _linkHelper = linkHelper;
    }

    /// <summary>
    /// Gets information about the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Returns the authenticated user's profile information.
    /// </returns>
    [HttpGet("me")]
    [EndpointName("GetUserInfos")]
    public async Task<IActionResult> GetUserAuthenticatedInfos()
    {
        var result = await _userService.GetUserAuthenticatedInfos();
        result.Links = new List<LinkResponse>()
        {
            _linkHelper.GenerateLinkResponse("UpdateUserInfos", "update", HttpMethods.Put),
            _linkHelper.GenerateLinkResponse("UpdateAddress", "update-address", HttpMethods.Patch),
            _linkHelper.GenerateLinkResponse("GetSubscription", "subscription", HttpMethods.Get),
            _linkHelper.GenerateLinkResponse("GetUserInfos", "self", HttpMethods.Get),
        };
        
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
    [EndpointName("UpdateAddress")]
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
    [EndpointName("UpdateUserInfos")]
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
}