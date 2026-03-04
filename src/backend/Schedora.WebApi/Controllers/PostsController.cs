using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;

namespace Schedora.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var result = await _postService.CreatePost(request);
        
        return Created(string.Empty, result);
    }

    [HttpGet("{id:long}/validation")]
    public async Task<IActionResult> GetPostValidation([FromRoute]long id)
    {
        var result = await _postService.GetPostValidation(id);
        
        return Ok(result);
    }

    [HttpPost("{id}/publish-now")]
    public async Task<IActionResult> PublishPostNow([FromRoute]long id)
    {
        var result = await _postService.PublishPost(id);
        
        return Ok(result);
    }

    [HttpPost("{id:long}/schedule")]
    public async Task<IActionResult> SchedulePost([FromRoute] long id, [FromBody] SchedulePostRequest request)
    {
        var result = await _postService.SchedulePost(id, request);

        return Ok(result);
    }

    [HttpPut("{id:long}/schedule")]
    public async Task<IActionResult> ReschedulePost([FromRoute] long id, [FromBody] ReschedulePostRequest request)
    {
        var result = await _postService.ReschedulePost(id, request);

        return Ok(result);
    }

    [HttpDelete("{id:long}/schedule")]
    public async Task<IActionResult> UnschedulePost([FromRoute] long id)
    {
        var result = await _postService.UnschedulePost(id);

        return Ok(result);
    }
}