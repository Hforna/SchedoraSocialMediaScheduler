using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class Post : Entity
    {
        public long UserId { get; private set; }
        public string Content { get; private set; }
        public PostStatus Status { get; private set; }
        public DateTime? ScheduledAt { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public string? ScheduledTimezone { get; private set; }
        public bool IsRecurring { get; private set; }
        public string? RecurrencePattern { get; private set; }
        public long? ParentPostId { get; private set; }
        public long? QueueId { get; private set; }
        public long? TemplateId { get; private set; }
        public string? Notes { get; private set; }
        public long? CreatedBy { get; private set; }
        public long? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public ApprovalStatus ApprovalStatus { get; private set; }
        public string? RejectionReason { get; private set; }

        static Post Create(string content, long userId, PostStatus status, long createdBy, string scheduledTimezone,
            long? templateId, long? queueId)
        {
            var post = new Post()
            {
                
            };
        }
    }
}
