using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Schedora.Infrastructure.RabbitMq.Producers;

public class PostProducer : BaseProducer
{
    public PostProducer(IConnection connection, ILogger<BaseProducer> logger) : base(connection, logger)
    {
    }

    public async Task SendPostCreated()
    {
        
    }
}