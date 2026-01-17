using Microsoft.AspNetCore.Http;

namespace Schedora.Application.Requests;

public class UploadMediaRequest
{
    public IFormFile Media { get; set; }
    public IFormFile? Thumbnail { get; set; }
    public long? FolderId { get; set; }
    public string? Description { get; set; }
    public string MediaName { get; set; }
}