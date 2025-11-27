using Microsoft.AspNetCore.Http;
using Schedora.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpContextAccessor _httpContext;

        public RequestService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public string GetAuthenticationHeaderToken()
        {
            var token = _httpContext.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(token))
                return "";

            return token["Bearer".Length..].Trim();
        }
        
        public string? GetRequestIpAddress()
        {
            var httpContext = _httpContext.HttpContext;

            // Try proxies headers first
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Ip connection fallback 
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }

        public string GetUserAgent()
        {
            return _httpContext.HttpContext.Request.Headers.UserAgent.ToString();
        }
    }
}
