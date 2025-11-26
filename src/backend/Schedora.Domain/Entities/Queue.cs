using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class Queue : Entity
    {
        // Properties
        public long UserId { get; private set; }
        public string Name { get; private set; }
        public string? Platforms { get; private set; } // JSON array of social account IDs
        public string Schedule { get; private set; } // JSON schedule definition
        public string? Timezone { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual ICollection<Post> Posts { get; private set; }
        public virtual ICollection<QueuePost> QueuePosts { get; private set; }

        // Private constructor for EF
        private Queue()
        {
            Posts = new HashSet<Post>();
            QueuePosts = new HashSet<QueuePost>();
        }

        // Factory method
        public static Queue Create(long userId, string name, string schedule, string timezone)
        {
            return new Queue
            {
                UserId = userId,
                Name = name,
                Schedule = schedule,
                Timezone = timezone,
                IsActive = true
            };
        }

        // Domain methods
        public void UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSchedule(string schedule, string timezone)
        {
            Schedule = schedule;
            Timezone = timezone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPlatforms(string platformsJson)
        {
            Platforms = platformsJson;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
