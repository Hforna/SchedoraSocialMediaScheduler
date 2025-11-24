using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class PostAnalytics : Entity
    {
        // Properties
        public Guid PostPlatformId { get; private set; }
        public int Likes { get; private set; }
        public int Comments { get; private set; }
        public int Shares { get; private set; }
        public int Retweets { get; private set; }
        public int Replies { get; private set; }
        public int Impressions { get; private set; }
        public int Reach { get; private set; }
        public int Clicks { get; private set; }
        public int VideoViews { get; private set; }
        public int Saves { get; private set; }
        public decimal EngagementRate { get; private set; }
        public DateTime FetchedAt { get; private set; }

        // Navigation Properties
        public virtual PostPlatform PostPlatform { get; private set; }

        // Private constructor for EF
        private PostAnalytics() { }

        // Factory method
        public static PostAnalytics Create(Guid postPlatformId)
        {
            return new PostAnalytics
            {
                PostPlatformId = postPlatformId,
                FetchedAt = DateTime.UtcNow
            };
        }

        // Domain methods
        public void UpdateMetrics(
            int likes,
            int comments,
            int shares,
            int retweets,
            int replies,
            int impressions,
            int reach,
            int clicks,
            int videoViews,
            int saves)
        {
            Likes = likes;
            Comments = comments;
            Shares = shares;
            Retweets = retweets;
            Replies = replies;
            Impressions = impressions;
            Reach = reach;
            Clicks = clicks;
            VideoViews = videoViews;
            Saves = saves;

            CalculateEngagementRate();
            FetchedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        private void CalculateEngagementRate()
        {
            if (Impressions == 0)
            {
                EngagementRate = 0;
                return;
            }

            var totalEngagements = Likes + Comments + Shares + Retweets + Replies + Clicks + Saves;
            EngagementRate = Math.Round((decimal)totalEngagements / Impressions * 100, 2);
        }

        public int GetTotalEngagements()
        {
            return Likes + Comments + Shares + Retweets + Replies + Clicks + Saves;
        }
    }
}
