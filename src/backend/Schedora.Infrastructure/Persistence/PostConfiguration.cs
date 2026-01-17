using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Infrastructure.Persistence
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Content)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(PostStatus.Draft);

            builder.Property(p => p.ScheduledTimezone)
                .HasMaxLength(50);

            builder.Property(p => p.IsRecurring)
                .HasDefaultValue(false);

            builder.Property(p => p.RecurrencePattern)
                .HasMaxLength(500);

            builder.Property(p => p.ApprovalStatus)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ApprovalStatus.NotRequired);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(p => new { p.UserId, p.Status });
            builder.HasIndex(p => p.ScheduledAt);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.CreatedAt).IsDescending();
        }


        public class PostPlatformConfiguration : IEntityTypeConfiguration<PostPlatform>
        {
            public void Configure(EntityTypeBuilder<PostPlatform> builder)
            {
                builder.ToTable("PostPlatforms");
                builder.HasKey(pp => pp.Id);

                builder.Property(pp => pp.Platform)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                builder.Property(pp => pp.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(PostStatus.Pending);

                builder.Property(pp => pp.PlatformPostId)
                    .HasMaxLength(255);

                builder.Property(pp => pp.PlatformPostUrl)
                    .HasMaxLength(500);

                builder.Property(pp => pp.ErrorCode)
                    .HasMaxLength(100);

                builder.Property(pp => pp.RetryCount)
                    .HasDefaultValue(0);

                builder.Property(pp => pp.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(pp => pp.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(pp => pp.PostId);
                builder.HasIndex(pp => pp.SocialAccountId);
                builder.HasIndex(pp => pp.Status);
                builder.HasIndex(pp => new { pp.Platform, pp.PlatformPostId });
            }
        }

        public class MediaConfiguration : IEntityTypeConfiguration<Media>
        {
            public void Configure(EntityTypeBuilder<Media> builder)
            {
                builder.ToTable("Media");

                builder.HasKey(m => m.Id);

                builder.Property(m => m.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                builder.Property(m => m.OriginalFileName)
                    .HasMaxLength(255);

                builder.Property(m => m.FileSize)
                    .IsRequired();

                builder.Property(m => m.MimeType)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(m => m.ThumbnailName)
                    .HasMaxLength(500);

                builder.Property(m => m.Description)
                    .HasMaxLength(1000);

                builder.Property(m => m.IsProcessed)
                    .HasDefaultValue(false);

                builder.Property(m => m.ProcessingStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                builder.Property(m => m.Hash)
                    .HasMaxLength(255);

                builder.Property(m => m.UsageCount)
                    .HasDefaultValue(0);

                builder.Property(m => m.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(m => m.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(m => m.UserId);
                builder.HasIndex(m => m.FolderId);
                builder.HasIndex(m => m.UploadedAt).IsDescending();
                builder.HasIndex(m => m.Hash);
            }
        }

        public class MediaFolderConfiguration : IEntityTypeConfiguration<MediaFolder>
        {
            public void Configure(EntityTypeBuilder<MediaFolder> builder)
            {
                builder.ToTable("MediaFolders");

                builder.HasKey(mf => mf.Id);

                builder.Property(mf => mf.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                builder.Property(mf => mf.Path)
                    .HasMaxLength(1000);

                builder.Property(mf => mf.Color)
                    .HasMaxLength(20);

                builder.Property(mf => mf.Icon)
                    .HasMaxLength(50);

                builder.Property(mf => mf.MediaCount)
                    .HasDefaultValue(0);

                builder.Property(mf => mf.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(mf => mf.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(mf => mf.UserId);
                builder.HasIndex(mf => mf.ParentFolderId);
                builder.HasIndex(mf => mf.Path);
            }
        }

        public class PostAnalyticsConfiguration : IEntityTypeConfiguration<PostAnalytics>
        {
            public void Configure(EntityTypeBuilder<PostAnalytics> builder)
            {
                builder.ToTable("PostAnalytics");

                builder.HasKey(pa => pa.Id);

                builder.Property(pa => pa.Likes).HasDefaultValue(0);
                builder.Property(pa => pa.Comments).HasDefaultValue(0);
                builder.Property(pa => pa.Shares).HasDefaultValue(0);
                builder.Property(pa => pa.Retweets).HasDefaultValue(0);
                builder.Property(pa => pa.Replies).HasDefaultValue(0);
                builder.Property(pa => pa.Impressions).HasDefaultValue(0);
                builder.Property(pa => pa.Reach).HasDefaultValue(0);
                builder.Property(pa => pa.Clicks).HasDefaultValue(0);
                builder.Property(pa => pa.VideoViews).HasDefaultValue(0);
                builder.Property(pa => pa.Saves).HasDefaultValue(0);

                builder.Property(pa => pa.EngagementRate)
                    .HasPrecision(5, 2);

                builder.Property(pa => pa.FetchedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(pa => pa.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(pa => pa.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(pa => new { pa.PostPlatformId, pa.FetchedAt })
                    .IsDescending();
            }
        }

        public class QueueConfiguration : IEntityTypeConfiguration<Queue>
        {
            public void Configure(EntityTypeBuilder<Queue> builder)
            {
                builder.ToTable("Queues");

                builder.HasKey(q => q.Id);

                builder.Property(q => q.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                builder.Property(q => q.Schedule)
                    .IsRequired();

                builder.Property(q => q.Timezone)
                    .HasMaxLength(50);

                builder.Property(q => q.IsActive)
                    .HasDefaultValue(true);

                builder.Property(q => q.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(q => q.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            }
        }

        public class QueuePostConfiguration : IEntityTypeConfiguration<QueuePost>
        {
            public void Configure(EntityTypeBuilder<QueuePost> builder)
            {
                builder.ToTable("QueuePosts");

                builder.HasKey(qp => qp.Id);

                builder.Property(qp => qp.OrderIndex);

                builder.Property(qp => qp.AddedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(qp => qp.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(qp => qp.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                builder.HasOne(qp => qp.Queue)
                    .WithMany(q => q.QueuePosts)
                    .HasForeignKey(qp => qp.QueueId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(qp => qp.Post)
                    .WithMany()
                    .HasForeignKey(qp => qp.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

        public class TemplateConfiguration : IEntityTypeConfiguration<Template>
        {
            public void Configure(EntityTypeBuilder<Template> builder)
            {
                builder.ToTable("Templates");

                builder.HasKey(t => t.Id);

                builder.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                builder.Property(t => t.Category)
                    .HasMaxLength(100);

                builder.Property(t => t.UsageCount)
                    .HasDefaultValue(0);

                builder.Property(t => t.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(t => t.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

            }
        }

        public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
        {
            public void Configure(EntityTypeBuilder<TeamMember> builder)
            {
                builder.ToTable("TeamMembers");

                builder.HasKey(tm => tm.Id);

                builder.Property(tm => tm.Role)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                builder.Property(tm => tm.InviteStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("pending");

                builder.Property(tm => tm.InvitedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(tm => tm.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(tm => tm.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                builder.HasOne(tm => tm.TeamOwner)
                    .WithMany()
                    .HasForeignKey(tm => tm.TeamOwnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(tm => tm.MemberUser)
                    .WithMany()
                    .HasForeignKey(tm => tm.MemberUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            }
        }

        public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
        {
            public void Configure(EntityTypeBuilder<ActivityLog> builder)
            {
                builder.ToTable("ActivityLogs");

                builder.HasKey(al => al.Id);

                builder.Property(al => al.Action)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(al => al.EntityType)
                    .HasMaxLength(50);

                builder.Property(al => al.IpAddress)
                    .HasMaxLength(50);

                builder.Property(al => al.UserAgent)
                    .HasMaxLength(500);

                builder.Property(al => al.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(al => al.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(al => new { al.UserId, al.CreatedAt })
                    .IsDescending();
            }
        }

        public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
        {
            public void Configure(EntityTypeBuilder<Notification> builder)
            {
                builder.ToTable("Notifications");

                builder.HasKey(n => n.Id);

                builder.Property(n => n.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                builder.Property(n => n.Title)
                    .HasMaxLength(255);

                builder.Property(n => n.IsRead)
                    .HasDefaultValue(false);

                builder.Property(n => n.RelatedEntityType)
                    .HasMaxLength(50);

                builder.Property(n => n.ActionUrl)
                    .HasMaxLength(500);

                builder.Property(n => n.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(n => n.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
                    .IsDescending();

            }
        }

        public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
        {
            public void Configure(EntityTypeBuilder<NotificationPreference> builder)
            {
                builder.ToTable("NotificationPreferences");

                builder.HasKey(np => np.Id);

                builder.Property(np => np.EmailOnPublish).HasDefaultValue(true);
                builder.Property(np => np.EmailOnFailure).HasDefaultValue(true);
                builder.Property(np => np.EmailDailySummary).HasDefaultValue(false);
                builder.Property(np => np.EmailWeeklySummary).HasDefaultValue(true);
                builder.Property(np => np.InAppNotifications).HasDefaultValue(true);

                builder.Property(np => np.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(np => np.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Unique constraint
                builder.HasIndex(np => np.UserId)
                    .IsUnique();
            }
        }
    }
}
