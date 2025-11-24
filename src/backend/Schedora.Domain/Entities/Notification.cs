using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class Notification : Entity
    {
        // Properties
        public Guid UserId { get; private set; }
        public string Type { get; private set; }
        public string? Title { get; private set; }
        public string? Message { get; private set; }
        public bool IsRead { get; private set; }
        public string? RelatedEntityType { get; private set; }
        public Guid? RelatedEntityId { get; private set; }
        public string? ActionUrl { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }

        // Private constructor for EF
        private Notification() { }

        // Factory method
        public static Notification Create(
            Guid userId,
            string type,
            string title,
            string message,
            string? relatedEntityType = null,
            Guid? relatedEntityId = null,
            string? actionUrl = null)
        {
            return new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                ActionUrl = actionUrl
            };
        }

        // Domain methods
        public void MarkAsRead()
        {
            IsRead = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsUnread()
        {
            IsRead = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
