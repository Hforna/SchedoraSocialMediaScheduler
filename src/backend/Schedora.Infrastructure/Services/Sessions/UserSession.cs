using Microsoft.AspNetCore.Http;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services.Session;

namespace Schedora.Infrastructure.Services.Sessions;

public class UserSession : IUserSession
{
    private readonly IHttpContextAccessor _httpContext;

    public UserSession(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public void AddUserId(long userId)
    {
        var session = _httpContext.HttpContext.Session;
        
        session.SetString("UserId", userId.ToString());
    }

    public long GetUserId()
    {
        var session = _httpContext.HttpContext.Session;
        
        var userIdSession =  session.GetString("UserId");
        
        if(string.IsNullOrEmpty(userIdSession))
            throw new RequestException("It was not possible to get the user id from the session");
        
        return long.Parse(userIdSession);
    }
}