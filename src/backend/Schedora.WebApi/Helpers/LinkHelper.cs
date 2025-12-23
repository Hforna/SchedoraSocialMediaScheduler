using Schedora.Application.Responses;

namespace Schedora.WebApi.Helpers;

public interface ILinkHelper
{
    public LinkResponse GenerateLinkResponse(string endpointName, string rel, string method);
}

public class LinkHelper : ILinkHelper
{
    public LinkHelper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public LinkResponse GenerateLinkResponse(string endpointName, string rel, string method)
    {
        return new LinkResponse()
        {
            Rel = rel,
            Method = method,
            Href = _linkGenerator.GetPathByName(_httpContextAccessor.HttpContext, endpointName)
        };
    }
}