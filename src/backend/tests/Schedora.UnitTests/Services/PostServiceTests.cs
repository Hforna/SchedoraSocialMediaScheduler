using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Application.Services;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services;
using Schedora.UnitTests.Mocks;
using SocialScheduler.Domain.Constants;

namespace Schedora.UnitTests.Services;

public class PostServiceTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<IPostDomainService> _postDomainService;
    private readonly UnitOfWorkMock _uowMock;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IMediaService> _mediaService;
    private readonly Mock<IActivityLogService> _activityLogService;
    private readonly Mock<IPostProducer> _postProducer;
    private readonly List<IMediaValidationEngine> _mediaValidationEngines;
    private readonly Mock<ILogger<IPostService>> _logger;
    private readonly List<IContentValidatorEngine> _contentValidatorEngines;

    private readonly IPostService _service;

    public PostServiceTests()
    {
        _mapper = new Mock<IMapper>();
        _currentUser = new Mock<ICurrentUserService>();
        _postDomainService = new Mock<IPostDomainService>();
        _uowMock = new UnitOfWorkMock();
        _uow = _uowMock.GetMock();
        _mediaService = new Mock<IMediaService>();
        _activityLogService = new Mock<IActivityLogService>();
        _postProducer = new Mock<IPostProducer>();
        _mediaValidationEngines = new List<IMediaValidationEngine>();
        _logger = new Mock<ILogger<IPostService>>();
        _contentValidatorEngines = new List<IContentValidatorEngine>();

        _service = new PostService(
            _mapper.Object,
            _currentUser.Object,
            _postDomainService.Object,
            _uow.Object,
            _mediaService.Object,
            _activityLogService.Object,
            _postProducer.Object,
            _mediaValidationEngines,
            _logger.Object,
            _contentValidatorEngines
        );
    }

    [Fact]
    public async Task PublishPost_ValidPost_ShouldSendPublishCommand()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Scheduled, user.Id, TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);
        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);

        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        _mapper.Setup(x => x.Map<PostResponse>(post)).Returns(new PostResponse());

        var result = await _service.PublishPost(1);

        result.Should().NotBeNull();
        _postProducer.Verify(x => x.SendPublishPost(post.Id), Times.Once);
    }

    [Fact]
    public async Task SchedulePost_ValidRequest_ShouldSchedulePostAndPublishEvent()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Pending, user.Id, TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);
        _currentUser.Setup(x => x.GetCurrentUserTimeZone()).Returns(TimeZoneInfo.Utc.Id);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        var request = new SchedulePostRequest
        {
            ScheduledAtLocal = DateTime.UtcNow.AddHours(1)
        };

        _mapper.Setup(x => x.Map<PostResponse>(It.IsAny<Post>())).Returns(new PostResponse());

        var result = await _service.SchedulePost(1, request);

        result.Should().NotBeNull();
        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledAt.Should().NotBeNull();
        post.ScheduledTimezone.Should().Be(TimeZoneInfo.Utc.Id);

        _postDomainService.Verify(x => x.IsPostScheduleValid(It.IsAny<DateTime>()), Times.Once);
        _postProducer.Verify(x => x.SendPostScheduled(post.Id, It.IsAny<DateTime>()), Times.Once);
        _uowMock.GenericRepository.Verify(x => x.Update<Post>(post), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task SchedulePost_InvalidStatus_ShouldThrowDomainException()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Scheduled, user.Id, TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        var request = new SchedulePostRequest
        {
            ScheduledAtLocal = DateTime.UtcNow.AddHours(1)
        };

        var act = async () => await _service.SchedulePost(1, request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Post cannot be scheduled");
    }

    [Fact]
    public async Task ReschedulePost_ValidRequest_ShouldRescheduleAndPublishEvent()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Pending, user.Id, TimeZoneInfo.Utc.Id);
        post.Schedule(DateTime.UtcNow.AddHours(1), TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);
        _currentUser.Setup(x => x.GetCurrentUserTimeZone()).Returns(TimeZoneInfo.Utc.Id);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        var request = new ReschedulePostRequest
        {
            ScheduledAtLocal = DateTime.UtcNow.AddHours(2)
        };

        _mapper.Setup(x => x.Map<PostResponse>(It.IsAny<Post>())).Returns(new PostResponse());

        var result = await _service.ReschedulePost(1, request);

        result.Should().NotBeNull();
        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledAt.Should().NotBeNull();

        _postDomainService.Verify(x => x.IsPostScheduleValid(It.IsAny<DateTime>()), Times.Once);
        _postProducer.Verify(x => x.SendPostScheduled(post.Id, It.IsAny<DateTime>()), Times.Once);
        _uowMock.GenericRepository.Verify(x => x.Update<Post>(post), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ReschedulePost_InvalidStatus_ShouldThrowDomainException()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Pending, user.Id, TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        var request = new ReschedulePostRequest
        {
            ScheduledAtLocal = DateTime.UtcNow.AddHours(2)
        };

        var act = async () => await _service.ReschedulePost(1, request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Post cannot be rescheduled");
    }

    [Fact]
    public async Task UnschedulePost_ValidRequest_ShouldUnscheduleAndPublishEvent()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Pending, user.Id, TimeZoneInfo.Utc.Id);
        post.Schedule(DateTime.UtcNow.AddHours(1), TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        _mapper.Setup(x => x.Map<PostResponse>(It.IsAny<Post>())).Returns(new PostResponse());

        var result = await _service.UnschedulePost(1);

        result.Should().NotBeNull();
        post.Status.Should().Be(PostStatus.Pending);
        post.ScheduledAt.Should().BeNull();

        _postProducer.Verify(x => x.SendPostUnscheduled(post.Id), Times.Once);
        _uowMock.GenericRepository.Verify(x => x.Update<Post>(post), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task UnschedulePost_InvalidStatus_ShouldThrowDomainException()
    {
        var user = new User { Id = 1 };
        var post = Post.Create("content", user.Id, PostStatus.Pending, user.Id, TimeZoneInfo.Utc.Id);

        _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);

        _uowMock.PostRepository = new Mock<IPostRepository>();
        _uow.SetupGet(x => x.PostRepository).Returns(_uowMock.PostRepository.Object);
        _uowMock.PostRepository.Setup(x => x.GetPostById(It.IsAny<long>())).ReturnsAsync(post);

        var act = async () => await _service.UnschedulePost(1);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Post cannot be unscheduled");
    }
}
