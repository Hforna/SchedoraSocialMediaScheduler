using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;

namespace Schedora.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetUserAuthenticatedInfos()
    {
        var result = await _userService.GetUserAuthenticatedInfos();
        return Ok(result);
    }

    [HttpPatch("address")]
    public async Task<IActionResult> UpdateAddress([FromBody]UpdateAddressRequest request)
    {
        var result = await _userService.UpdateAddress(request);
        
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserInfos([FromBody]UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserInfos(request);

        return Ok(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody]UpdatePasswordRequest request)
    {
        await _userService.UpdatePassword(request);

        return Ok();
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetUserSubscription()
    {
        var result = await _userService.GetUserSubscription();

        return Ok(result);
    }
}