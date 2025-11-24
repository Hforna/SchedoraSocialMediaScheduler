using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class Template : Entity
    {
        // Properties
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public string? Content { get; private set; }
        public string? Platforms { get; private set; } // JSON array
        public string? Category { get; private set; }
        public int UsageCount { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual ICollection<Post> PostsCreatedFromTemplate { get; private set; }

        // Private constructor for EF
        private Template()
        {
            PostsCreatedFromTemplate = new HashSet<Post>();
        }

        // Factory method
        public static Template Create(Guid userId, string name, string content, string? category = null)
        {
            return new Template
            {
                UserId = userId,
                Name = name,
                Content = content,
                Category = category,
                UsageCount = 0
            };
        }

        // Domain methods
        public void Update(string name, string content, string? category = null)
        {
            Name = name;
            Content = content;
            Category = category;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPlatforms(string platformsJson)
        {
            Platforms = platformsJson;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementUsage()
        {
            UsageCount++;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
