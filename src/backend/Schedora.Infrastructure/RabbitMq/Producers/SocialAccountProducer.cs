using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;

namespace Schedora.Infrastructure.RabbitMq.Producers;

public class SocialAccountProducer : ISocialAccountProducer
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly RabbitMqConnection  _rabbitMqConnection;
    private readonly ILogger<SocialAccountProducer> _logger;

    public SocialAccountProducer(IOptions<RabbitMqConnection> rabbitMqConnection, ILogger<SocialAccountProducer> logger)
    {
        _rabbitMqConnection = rabbitMqConnection.Value;
        _logger = logger;
    }


    public async Task SendAccountConnected(SocialAccountConnectedDto dto)
    {
        _connection = await new ConnectionFactory()
        {
            Port = _rabbitMqConnection.Port,
            HostName = _rabbitMqConnection.Host,
            UserName = _rabbitMqConnection.UserName,
            Password = _rabbitMqConnection.Password,
        }.CreateConnectionAsync();

        _channel = await _connection.CreateChannelAsync();
        
        await _channel.ExchangeDeclareAsync("schedora.social.events", ExchangeType.Direct, true, false);

        var serialize = JsonSerializer.Serialize(dto);
        var dtoBytes = Encoding.UTF8.GetBytes(serialize);

        await _channel.BasicPublishAsync("schedora.social.events", "socialaccount.connected", dtoBytes);
    }
}