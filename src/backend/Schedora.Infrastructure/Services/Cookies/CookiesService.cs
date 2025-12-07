using Microsoft.AspNetCore.Http;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services.Cookies;

public class CookiesService : ICookiesService
{
    public CookiesService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    private readonly IHttpContextAccessor _httpContext;
    
    public long? GetUserId()
    {
        return _httpContext.HttpContext.Request.Cookies.TryGetValue("UserId", out var value) ? long.Parse(value) : null;
    }
}