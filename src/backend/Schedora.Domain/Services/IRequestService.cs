using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Services
{
    public interface IRequestService
    {
        public string GetAuthenticationHeaderToken();
        public string? GetRequestIpAddress();
        public string GetUserAgent();
    }
}
