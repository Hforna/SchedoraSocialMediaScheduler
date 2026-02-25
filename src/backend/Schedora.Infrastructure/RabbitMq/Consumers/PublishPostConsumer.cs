using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schedora.Application.Services;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Migrations;
using Schedora.Infrastructure.Services.Externals;

namespace Schedora.Infrastructure.RabbitMq.Consumers;

public class PublishPostConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<PostValidationConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PublishPostConsumer(IConnection connection, ILogger<PostValidationConsumer> logger, IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("posts.commands.publish", "direct", true, false, cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync("post-publish-queue", true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("post-publish-queue", "posts.commands.publish", "post.publish", cancellationToken: stoppingToken);
        await _channel.QueuePurgeAsync("post-publish-queue", cancellationToken: stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                await Consume(Encoding.UTF8.GetString(ea.Body.ToArray()), stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming publish post message");

                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("post-publish-queue", false, consumer, cancellationToken: stoppingToken);
    }

    private async Task Consume(string message, CancellationToken stoppingToken = default)
    {
        var postId = JsonSerializer.Deserialize<long>(message);

        using var scope = _serviceProvider.CreateScope();
        
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();

        var post = await uow.PostRepository.GetPostById(postId)
            ?? throw new NotFoundException("Post not found");
        var postMedias = await uow.MediaRepository.GetPostMedias(postId);
        
        var canPublish = post.CanBePublished();

        if (!canPublish)
        {
            _logger.LogWarning("Post cannot be published");
            return;
        }
        
        var mediasUrls = new Dictionary<Media, string>();
        foreach (var postMedia in postMedias)
        {
            var media = await storageService.GetUrlMedia(postMedia.FileName, postMedia.UserId);
            mediasUrls.Add(postMedia, media);
        }
        
        var postPlatforms = post.Platforms.ToList();
        for (var i = 0; i < postPlatforms.Count; i++)
        {
            var socialAccount = await uow.GenericRepository.GetById<SocialAccount>(postPlatforms[i].SocialAccountId);
            
            var currPlatform = postPlatforms[i];
            var postService = scope.ServiceProvider.GetRequiredService<IEnumerable<ISocialPostService>>();
            if (currPlatform.Platform == SocialPlatformsNames.Twitter)
            {
                var twitterPostService = postService.FirstOrDefault(d => d.Platform == SocialPlatformsNames.Twitter);
                List<string> mediasIds = null;
                if (mediasUrls.Count != 0)
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IEnumerable<ISocialMediaUploaderService>>()
                        .FirstOrDefault(d => d.Platform == SocialPlatformsNames.Twitter);
                
                    var mediasUploaded = await HandleTwitterMediaUploading(mediasUrls, service, socialAccount, stoppingToken);
                    if (mediasUploaded.Any(d => d.ProcessingInfo.State == "failed"))
                    {
                        throw new ExternalServiceException("Twitter media processing failed");   
                    }
                    mediasIds = mediasUploaded.Select(d => d.Id).ToList();
                }

                await twitterPostService.CreatePost(new SocialCreatePostRequestDto()
                {
                    MediaIds = mediasIds,
                    Text = post.Content
                }, socialAccount, stoppingToken);
            }
        }
    }

    public async Task<List<MediaUploadDataDto>> HandleTwitterMediaUploading(
        Dictionary<Media, string> medias,
        ISocialMediaUploaderService service,
        SocialAccount socialAccount,
        CancellationToken stoppingToken)
    {
        var response = new List<MediaUploadDataDto>();

        foreach (var media in medias)
        {
            var uploadResponse = await service.Upload(socialAccount, media.Key, media.Value);
            var mediaResponse = await service.GetMediaUploadStatus(uploadResponse.Id, socialAccount);
            response.Add(mediaResponse);
        }

        if (service is not ITwitterMediaService twitterMediaService)
            return response;

        for (var i = 0; i < response.Count; i++)
        {
            var mediaId = response[i].Id;
            var state = response[i].ProcessingInfo?.State;

            if (string.IsNullOrWhiteSpace(mediaId))
                continue;

            if (string.Equals(state, "pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(state, "in_progress", StringComparison.OrdinalIgnoreCase))
            {
                response[i] = await twitterMediaService.WaitForMediaProcessingAsync(
                    mediaId,
                    socialAccount,
                    stoppingToken);
            }
        }

        return response;
    }
}