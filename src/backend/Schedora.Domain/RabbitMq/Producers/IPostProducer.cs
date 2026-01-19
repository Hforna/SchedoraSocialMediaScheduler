namespace Schedora.Domain.RabbitMq.Producers;

public interface IPostProducer
{
    public Task SendPostCreated();
}