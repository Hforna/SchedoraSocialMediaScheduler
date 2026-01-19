using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application;
using Schedora.Application.Requests;
using Schedora.Application.Services;

namespace Schedora.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediasController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediasController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// Upload media to use on social posts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadMedia([FromForm]UploadMediaRequest request)
    {
        var result = await _mediaService.UploadMedia(request);
        
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserMedias()
    {
        int? result = null;
        
        return Ok(result);
    }
}