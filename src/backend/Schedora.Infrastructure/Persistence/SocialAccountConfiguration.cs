using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedora.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Infrastructure.Persistence
{
    public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
    {
        public void Configure(EntityTypeBuilder<SocialAccount> builder)
        {
            builder.ToTable("SocialAccounts");

            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.Platform)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(sa => sa.PlatformUserId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(sa => sa.UserName)
                .HasMaxLength(255);

            builder.Property(sa => sa.ProfileImageUrl)
                .HasMaxLength(500);

            builder.Property(sa => sa.AccessToken)
                .IsRequired();

            builder.Property(sa => sa.TokenType)
                .HasMaxLength(50)
                .HasDefaultValue("Bearer");

            builder.Property(sa => sa.Scopes)
                .HasMaxLength(500);

            builder.Property(sa => sa.IsActive)
                .HasDefaultValue(true);

            builder.Property(sa => sa.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sa => sa.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(sa => sa.UserId);
            builder.HasIndex(sa => sa.Platform);
            builder.HasIndex(sa => sa.TokenExpiresAt);

            // Unique constraint
            builder.HasIndex(sa => new { sa.UserId, sa.Platform, sa.PlatformUserId })
                .IsUnique()
                .HasDatabaseName("UQ_SocialAccounts_UserPlatformAccount");
        }
    }
}
