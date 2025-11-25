namespace Schedora.WebApi.Extensions;

public static class HttpContextExtensions
{
    public static string GetBaseUri(this HttpContext httpContext)
    {
        var uriBuilder = new UriBuilder(httpContext.Request.Scheme, httpContext.Request.Host.Host, httpContext.Request.Host.Port ?? -1);
        return uriBuilder.Uri.AbsoluteUri;
    }
}