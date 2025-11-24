using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class PostPlatform : Entity
    {
        public Guid PostId { get; private set; }
        public Guid SocialAccountId { get; private set; }
        public Platform Platform { get; private set; }
        public PostStatus Status { get; private set; }
        public string? PlatformPostId { get; private set; }
        public string? PlatformPostUrl { get; private set; }
        public string? PlatformResponse { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }
        public int RetryCount { get; private set; }
        public DateTime? LastRetryAt { get; private set; }
        public DateTime? NextRetryAt { get; private set; }
        public DateTime? PublishedAt { get; private set; }
    }
}
