using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Schedora.Domain.Exceptions;

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
        public IEnumerable<PostPlatform> Platforms { get; set; }

        public static Post Create(string content, long userId, PostStatus status, long createdBy, string scheduledTimezone,
            long? templateId = null, string? notes = null)
        {
            var post = new Post()
            {
                Status = status,
                CreatedAt = DateTime.UtcNow,
                Content = content,
                UserId = userId,
                CreatedBy = createdBy,
                ScheduledTimezone = scheduledTimezone,
                TemplateId = templateId,
                Notes = notes,
            };

            if (userId != createdBy)
                post.ApprovalStatus = ApprovalStatus.Pending;
            else
                post.ApprovalStatus = ApprovalStatus.NotRequired;
            
            return post;
        }

        public bool CanBePublished()
        {
            return Status == PostStatus.Scheduled 
                   || Status == PostStatus.Publishing;
        }

        public bool CanBeScheduled()
        {
            return Status == PostStatus.Draft
                   || Status == PostStatus.Pending
                   || Status == PostStatus.Failed
                   || Status == PostStatus.Cancelled;
        }

        public bool CanBeRescheduled()
        {
            return Status == PostStatus.Scheduled;
        }

        public bool CanBeUnscheduled()
        {
            return Status == PostStatus.Scheduled;
        }

        public void Schedule(DateTime scheduledAtUtc, string scheduledTimezone)
        {
            if (!CanBeScheduled())
                throw new DomainException("Post cannot be scheduled in the current status");

            ScheduledAt = scheduledAtUtc;
            ScheduledTimezone = scheduledTimezone;
            Status = PostStatus.Scheduled;
        }

        public void Reschedule(DateTime newScheduledAtUtc)
        {
            if (!CanBeRescheduled())
                throw new DomainException("Post cannot be rescheduled in the current status");

            ScheduledAt = newScheduledAtUtc;
        }

        public void Unschedule()
        {
            if (!CanBeUnscheduled())
                throw new DomainException("Post cannot be unscheduled in the current status");

            ScheduledAt = null;
            Status = PostStatus.Pending;
        }
    }
}
