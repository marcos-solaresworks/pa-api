using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ApiCentral.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica o status geral da aplicação (endpoint simples para load balancers)
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Health check detalhado com informações de todos os serviços
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    exception = entry.Value.Exception?.Message
                }),
                timestamp = DateTime.UtcNow
            };

            var status = report.Status switch
            {
                HealthStatus.Healthy => 200,
                HealthStatus.Degraded => 200, // Still accessible but with warnings
                HealthStatus.Unhealthy => 503,
                _ => 500
            };

            return StatusCode(status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health check status");
            return StatusCode(500, new { status = "error", message = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Health check rápido apenas para serviços críticos
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("ready"));

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Count,
                ready = report.Status == HealthStatus.Healthy,
                timestamp = DateTime.UtcNow
            };

            return report.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking readiness");
            return StatusCode(503, new { ready = false, error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Health check básico apenas para verificar se a aplicação está viva
    /// </summary>
    [HttpGet("live")]
    public async Task<IActionResult> GetLiveness()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("live"));

            var response = new
            {
                status = report.Status.ToString(),
                alive = report.Status != HealthStatus.Unhealthy,
                timestamp = DateTime.UtcNow
            };

            return report.Status != HealthStatus.Unhealthy 
                ? Ok(response) 
                : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking liveness");
            return StatusCode(503, new { alive = false, error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}