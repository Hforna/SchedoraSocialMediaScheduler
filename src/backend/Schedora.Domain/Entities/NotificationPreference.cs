using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class NotificationPreference : Entity
    {
        // Properties
        public Guid UserId { get; private set; }
        public bool EmailOnPublish { get; private set; }
        public bool EmailOnFailure { get; private set; }
        public bool EmailDailySummary { get; private set; }
        public bool EmailWeeklySummary { get; private set; }
        public bool InAppNotifications { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }

        // Private constructor for EF
        private NotificationPreference() { }

        // Factory method
        public static NotificationPreference CreateDefault(Guid userId)
        {
            return new NotificationPreference
            {
                UserId = userId,
                EmailOnPublish = true,
                EmailOnFailure = true,
                EmailDailySummary = false,
                EmailWeeklySummary = true,
                InAppNotifications = true
            };
        }

        // Domain methods
        public void UpdatePreferences(
            bool emailOnPublish,
            bool emailOnFailure,
            bool emailDailySummary,
            bool emailWeeklySummary,
            bool inAppNotifications)
        {
            EmailOnPublish = emailOnPublish;
            EmailOnFailure = emailOnFailure;
            EmailDailySummary = emailDailySummary;
            EmailWeeklySummary = emailWeeklySummary;
            InAppNotifications = inAppNotifications;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
