using System;
using FluentAssertions;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Xunit;

namespace Schedora.UnitTests.Domain;

public class PostTests
{
    [Theory]
    [InlineData(PostStatus.Draft, true)]
    [InlineData(PostStatus.Pending, true)]
    [InlineData(PostStatus.Failed, true)]
    [InlineData(PostStatus.Cancelled, true)]
    [InlineData(PostStatus.Scheduled, false)]
    [InlineData(PostStatus.Publishing, false)]
    public void CanBeScheduled_ShouldReturnExpectedResult(PostStatus status, bool expected)
    {
        var post = Post.Create("content", 1, status, 1, TimeZoneInfo.Utc.Id);

        var result = post.CanBeScheduled();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PostStatus.Scheduled, true)]
    [InlineData(PostStatus.Pending, false)]
    [InlineData(PostStatus.Draft, false)]
    public void CanBeRescheduled_ShouldReturnExpectedResult(PostStatus status, bool expected)
    {
        var post = Post.Create("content", 1, status, 1, TimeZoneInfo.Utc.Id);

        var result = post.CanBeRescheduled();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PostStatus.Scheduled, true)]
    [InlineData(PostStatus.Pending, false)]
    [InlineData(PostStatus.Draft, false)]
    public void CanBeUnscheduled_ShouldReturnExpectedResult(PostStatus status, bool expected)
    {
        var post = Post.Create("content", 1, status, 1, TimeZoneInfo.Utc.Id);

        var result = post.CanBeUnscheduled();

        result.Should().Be(expected);
    }

    [Fact]
    public void Schedule_ValidStatus_ShouldUpdateProperties()
    {
        var post = Post.Create("content", 1, PostStatus.Pending, 1, TimeZoneInfo.Utc.Id);
        var scheduledAt = DateTime.UtcNow.AddHours(1);
        var timezone = TimeZoneInfo.Utc.Id;

        post.Schedule(scheduledAt, timezone);

        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledAt.Should().Be(scheduledAt);
        post.ScheduledTimezone.Should().Be(timezone);
    }

    [Fact]
    public void Schedule_InvalidStatus_ShouldThrowDomainException()
    {
        var post = Post.Create("content", 1, PostStatus.Scheduled, 1, TimeZoneInfo.Utc.Id);

        var act = () => post.Schedule(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("Post cannot be scheduled in the current status");
    }

    [Fact]
    public void Reschedule_ValidStatus_ShouldUpdateScheduledAt()
    {
        var post = Post.Create("content", 1, PostStatus.Pending, 1, TimeZoneInfo.Utc.Id);
        post.Schedule(DateTime.UtcNow.AddHours(1), TimeZoneInfo.Utc.Id);
        var newTime = DateTime.UtcNow.AddHours(2);

        post.Reschedule(newTime);

        post.ScheduledAt.Should().Be(newTime);
    }

    [Fact]
    public void Reschedule_InvalidStatus_ShouldThrowDomainException()
    {
        var post = Post.Create("content", 1, PostStatus.Pending, 1, TimeZoneInfo.Utc.Id);

        var act = () => post.Reschedule(DateTime.UtcNow);

        act.Should().Throw<DomainException>()
            .WithMessage("Post cannot be rescheduled in the current status");
    }

    [Fact]
    public void Unschedule_ValidStatus_ShouldClearSchedule()
    {
        var post = Post.Create("content", 1, PostStatus.Pending, 1, TimeZoneInfo.Utc.Id);
        post.Schedule(DateTime.UtcNow.AddHours(1), TimeZoneInfo.Utc.Id);

        post.Unschedule();

        post.ScheduledAt.Should().BeNull();
        post.Status.Should().Be(PostStatus.Pending);
    }

    [Fact]
    public void Unschedule_InvalidStatus_ShouldThrowDomainException()
    {
        var post = Post.Create("content", 1, PostStatus.Pending, 1, TimeZoneInfo.Utc.Id);

        var act = () => post.Unschedule();

        act.Should().Throw<DomainException>()
            .WithMessage("Post cannot be unscheduled in the current status");
    }
}

