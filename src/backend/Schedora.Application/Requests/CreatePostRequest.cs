using Microsoft.AspNetCore.Http;

namespace Schedora.Application.Requests;

public class CreatePostRequest
{
    public string Content { get; set; }
    public long? TemplateId { get; set; }
    public string? Notes { get; set; }
    public List<MediaPostRequest> Medias  { get; set; }
    public List<long> SocialAccountsIds { get; set; }
    public bool TeamContext { get; set; }
}

public class MediaPostRequest
{
    public long MediaId { get; set; }
    public int OrderIndex { get; set; }
    public string AltText { get; set; }
}

public class ValidatePostRequest
{
    public string Content { get; set; }
    public List<MediaPostRequest> Medias  { get; set; }
    public List<long> SocialAccountsIds { get; set; }
}