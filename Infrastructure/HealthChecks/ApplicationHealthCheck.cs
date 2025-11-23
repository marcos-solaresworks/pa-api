using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ApiCentral.Infrastructure.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private static readonly DateTime StartTime = DateTime.UtcNow;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var uptime = DateTime.UtcNow - StartTime;
            var memoryUsage = GC.GetTotalMemory(false);

            var data = new Dictionary<string, object>
            {
                ["application"] = "API Central",
                ["version"] = "1.0.0",
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["uptime_seconds"] = (int)uptime.TotalSeconds,
                ["uptime_readable"] = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m",
                ["memory_usage_bytes"] = memoryUsage,
                ["memory_usage_mb"] = Math.Round(memoryUsage / (1024.0 * 1024.0), 2),
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["machine_name"] = Environment.MachineName,
                ["process_id"] = Environment.ProcessId,
                ["thread_count"] = System.Diagnostics.Process.GetCurrentProcess().Threads.Count
            };

            // Verificação básica de recursos
            if (memoryUsage > 500 * 1024 * 1024) // 500MB
            {
                _logger.LogWarning("High memory usage detected: {MemoryMB}MB", Math.Round(memoryUsage / (1024.0 * 1024.0), 2));
                return Task.FromResult(HealthCheckResult.Degraded("High memory usage", null, data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Application is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Application health check failed", ex));
        }
    }
}