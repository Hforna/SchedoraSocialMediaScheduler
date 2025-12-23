namespace Schedora.Application.Responses;

public class BaseResponse
{
    public List<LinkResponse> Links { get; set; }
}

public class LinkResponse
{
    public required string Rel { get; set; }
    public string Href { get; set; }
    public string Method { get; set; }
}