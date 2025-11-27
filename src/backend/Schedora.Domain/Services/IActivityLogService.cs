using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Services
{
    public interface IActivityLogService
    {
        public Task LogAsync(long userId, string action, string entityType, long entityId, object? details, bool commit = true);
    }
}
