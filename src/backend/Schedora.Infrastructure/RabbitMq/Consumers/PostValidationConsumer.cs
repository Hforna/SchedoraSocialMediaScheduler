using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Schedora.Application.Services;
using Schedora.Domain.Exceptions;
using Schedora.Infrastructure.RabbitMq.Producers;

namespace Schedora.Infrastructure.RabbitMq.Consumers;

public class PostValidationConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<PostValidationConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PostValidationConsumer(IConnection connection, ILogger<PostValidationConsumer> logger, IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("posts.events.created", "direct", true, false, cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync("post-created-queue", true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("post-created-queue", "posts.events.created", "post.created", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ModuleHandle, ea) =>
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            try
            {
                await ConsumeMessage(message);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (SerializationException e)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
            catch (DomainException de)
            {
                _logger.LogError("Domain error while tryinig to consume validation post message: {de.Message}",
                    de.Message);

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Unexpected error occured: {e.Message}", e.Message);
                
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };
        await _channel.BasicConsumeAsync("post-created-queue", false, consumer, cancellationToken: stoppingToken);
    }

    private async Task ConsumeMessage(string message)
    {
        var postId = JsonSerializer.Deserialize<long>(message);
        
        if (postId == 0)
            throw new SerializationException("Invalid post id from serialization");

        using var scope = _serviceProvider.CreateScope();
        var postService = scope.ServiceProvider.GetRequiredService<IPostService>();

        await postService.ValidatePost(postId);
    }
}