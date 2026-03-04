namespace Schedora.Domain.RabbitMq.Producers;

public interface IPostProducer
{
    public Task SendPostCreated(long postId);
    public Task SendPublishPost(long postId);
    public Task SendPostScheduled(long postId, DateTime scheduledAtUtc);
    public Task SendPostUnscheduled(long postId);
}