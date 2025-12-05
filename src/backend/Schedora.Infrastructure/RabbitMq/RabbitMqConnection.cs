namespace Schedora.Infrastructure.RabbitMq;

public class RabbitMqConnection
{
    public int Port  { get; set; }
    public string Host { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}