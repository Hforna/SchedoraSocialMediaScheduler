using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class ActivityLog : Entity
    {
        // Properties
        public long UserId { get; private set; }
        public string Action { get; private set; }
        public string? EntityType { get; private set; }
        public long? EntityId { get; private set; }
        public string? Details { get; private set; } // JSON
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }

        // Private constructor for EF
        private ActivityLog() { }

        // Factory method
        public static ActivityLog Create(
            long userId,
            string action,
            string? entityType = null,
            long? entityId = null,
            string? details = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            return new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };
        }
    }
}
