using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher(IConnection connection)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        
        // Declarar exchange e filas
        _channel.ExchangeDeclare("graficaltda.exchange", ExchangeType.Topic, true);
        _channel.QueueDeclare("lote.processamento", true, false, false, null);
        _channel.QueueBind("lote.processamento", "graficaltda.exchange", "lote.processamento", null);
    }

    public async Task PublishAsync<T>(T message, string routingKey) where T : class
    {
        var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var body = Encoding.UTF8.GetBytes(jsonMessage);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: "graficaltda.exchange",
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        await Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _channel?.Close();
            _channel?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}