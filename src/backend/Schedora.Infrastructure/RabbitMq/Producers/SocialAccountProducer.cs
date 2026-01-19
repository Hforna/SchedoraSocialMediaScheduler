using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;

namespace Schedora.Infrastructure.RabbitMq.Producers;

public class SocialAccountProducer : BaseProducer, ISocialAccountProducer
{
    public SocialAccountProducer(IConnection connection, ILogger<SocialAccountProducer> logger) : base(connection, logger)
    {
    }

    public async Task SendAccountConnected(SocialAccountConnectedDto dto)
    {
        _channel = await _connection.CreateChannelAsync();
        
        await _channel.ExchangeDeclareAsync("schedora.social.events", ExchangeType.Direct, true, false);

        var serialize = JsonSerializer.Serialize(dto);
        var dtoBytes = Encoding.UTF8.GetBytes(serialize);

        await _channel.BasicPublishAsync("schedora.social.events", "socialaccount.connected", dtoBytes);
    }
}