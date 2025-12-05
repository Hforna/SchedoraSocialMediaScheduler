using Schedora.Domain.Dtos;

namespace Schedora.Domain.RabbitMq.Producers;

public interface ISocialAccountProducer
{
    public Task SendAccountConnected(SocialAccountConnectedDto dto);
}