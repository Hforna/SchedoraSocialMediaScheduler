using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Schedora.Domain;
using Schedora.Domain.Entities;
using static Schedora.Infrastructure.Persistence.PostConfiguration;

namespace Schedora.Infrastructure.Persistence;

public class DataContext : IdentityDbContext<User, Role, long>
{
    public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext) {}

    public DbSet<SocialAccount> SocialAccounts { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostPlatform> PostPlatforms { get; set; }
    public DbSet<PostMedia> PostMedias { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<MediaFolder> MediaFolders { get; set; }
    public DbSet<PostAnalytics> PostAnalytics { get; set; }
    public DbSet<Queue> Queues { get; set; }
    public DbSet<QueuePost> QueuePosts { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SocialAccountConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostPlatformConfiguration());
        modelBuilder.ApplyConfiguration(new MediaConfiguration());
        modelBuilder.ApplyConfiguration(new MediaFolderConfiguration());
        modelBuilder.ApplyConfiguration(new PostAnalyticsConfiguration());
        modelBuilder.ApplyConfiguration(new QueueConfiguration());
        modelBuilder.ApplyConfiguration(new QueuePostConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TeamMemberConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}