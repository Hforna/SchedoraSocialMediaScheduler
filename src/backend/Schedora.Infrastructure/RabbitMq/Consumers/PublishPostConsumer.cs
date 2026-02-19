using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Schedora.Infrastructure.RabbitMq.Consumers;

public class PublishPostConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<PostValidationConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PublishPostConsumer(IConnection connection, IChannel channel, ILogger<PostValidationConsumer> logger, IServiceProvider serviceProvider)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("posts.commands.publish", "direct", true, false, cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync("post-publish-queue", true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("post-publish-queue", "posts.commands.publish", "post.publish", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (ModuleHandle, ea) =>
        {
            await Consume(Encoding.UTF8.GetString(ea.Body.ToArray()));

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync("post-publish-queue", false, consumer, cancellationToken: stoppingToken);
    }

    private async Task Consume(string message)
    {
        var postId = JsonSerializer.Deserialize<long>(message);
        
        
    }
}