using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Schedora.Domain.RabbitMq.Producers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Schedora.Infrastructure.RabbitMq.Producers;

public class PostProducer : BaseProducer, IPostProducer
{
    public PostProducer(IConnection connection, ILogger<BaseProducer> logger) : base(connection, logger)
    {
    }

    public async Task SendPostCreated(long postId)
    {
        _channel = await _connection.CreateChannelAsync();
        
        await _channel.ExchangeDeclareAsync("posts.events.created", ExchangeType.Direct, true, false);
        
        var serialize = JsonSerializer.Serialize(postId);
        var bytes = Encoding.UTF8.GetBytes(serialize);
        
        await _channel.BasicPublishAsync("posts.events.created", "post.created", bytes);
    }

    public async Task SendPublishPost(long postId)
    {
        _channel = await _connection.CreateChannelAsync();
        
        await _channel.ExchangeDeclareAsync("posts.commands.publish", ExchangeType.Direct, true, false);
        
        var serialize = JsonSerializer.Serialize(postId);
        var bytes = Encoding.UTF8.GetBytes(serialize);
        
        await _channel.BasicPublishAsync("posts.commands.publish", "post.publish", bytes);
    }

    public async Task SendPostScheduled(long postId, DateTime scheduledAtUtc)
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync("posts.events.scheduled", ExchangeType.Direct, true, false);

        var payload = new
        {
            PostId = postId,
            ScheduledAtUtc = scheduledAtUtc
        };

        var serialize = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(serialize);

        await _channel.BasicPublishAsync("posts.events.scheduled", "post.scheduled", bytes);
    }

    public async Task SendPostUnscheduled(long postId)
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync("posts.events.unscheduled", ExchangeType.Direct, true, false);

        var serialize = JsonSerializer.Serialize(postId);
        var bytes = Encoding.UTF8.GetBytes(serialize);

        await _channel.BasicPublishAsync("posts.events.unscheduled", "post.unscheduled", bytes);
    }
}