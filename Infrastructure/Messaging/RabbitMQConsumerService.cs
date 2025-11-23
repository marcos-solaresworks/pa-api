using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Infrastructure.Messaging;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<RabbitMQConsumerService> _logger;

    public RabbitMQConsumerService(
        IMessageConsumer consumer,
        ILogger<RabbitMQConsumerService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Consumer Service iniciado");
        
        await _consumer.StartAsync(stoppingToken);
        
        // Manter o serviço executando
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Consumer Service parando...");
        
        await _consumer.StopAsync(stoppingToken);
        await base.StopAsync(stoppingToken);
        
        _logger.LogInformation("RabbitMQ Consumer Service parado");
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}