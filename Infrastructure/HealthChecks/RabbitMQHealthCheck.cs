using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace ApiCentral.Infrastructure.HealthChecks;

public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQHealthCheck> _logger;

    public RabbitMQHealthCheck(IConnection connection, ILogger<RabbitMQHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection == null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection is null"));
            }

            if (!_connection.IsOpen)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection is closed"));
            }

            // Testar criação de canal
            using var channel = _connection.CreateModel();
            
            // Verificar se a fila existe
            try
            {
                channel.QueueDeclarePassive("lote.processamento");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Queue 'lote.processamento' não encontrada: {Error}", ex.Message);
                return Task.FromResult(HealthCheckResult.Degraded("Queue 'lote.processamento' não encontrada", ex));
            }

            var data = new Dictionary<string, object>
            {
                ["connection_state"] = _connection.IsOpen ? "Open" : "Closed",
                ["server_properties"] = _connection.ServerProperties?.Count ?? 0,
                ["queue_checked"] = "lote.processamento"
            };

            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ health check failed", ex));
        }
    }
}