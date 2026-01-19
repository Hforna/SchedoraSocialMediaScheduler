using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Schedora.Infrastructure.RabbitMq.Producers;

public abstract class BaseProducer
{
    protected IConnection _connection;
    protected IChannel _channel;
    protected ILogger<BaseProducer> _logger;

    public BaseProducer(IConnection connection, ILogger<BaseProducer> logger)
    {
        _connection = connection;
        _logger = logger;
    }
}