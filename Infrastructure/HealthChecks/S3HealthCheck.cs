using Microsoft.Extensions.Diagnostics.HealthChecks;
using Amazon.S3;
using Microsoft.Extensions.Logging;

namespace ApiCentral.Infrastructure.HealthChecks;

public class S3HealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3HealthCheck> _logger;
    private readonly string _bucketName;

    public S3HealthCheck(IAmazonS3 s3Client, ILogger<S3HealthCheck> logger, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = configuration["AWS:S3:BucketName"] ?? "api-central-storage";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se consegue listar objetos do bucket
            var listRequest = new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = 1
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["bucket_name"] = _bucketName,
                ["objects_count"] = response.KeyCount,
                ["region"] = _s3Client.Config.RegionEndpoint?.SystemName ?? "unknown",
                ["service_url"] = _s3Client.Config.ServiceURL ?? "default"
            };

            return HealthCheckResult.Healthy("S3 is healthy", data);
        }
        catch (Amazon.S3.AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("S3 bucket '{BucketName}' not found", _bucketName);
            return HealthCheckResult.Degraded($"S3 bucket '{_bucketName}' not found", s3Ex);
        }
        catch (Amazon.S3.AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Access denied to S3 bucket '{BucketName}'", _bucketName);
            return HealthCheckResult.Degraded($"Access denied to S3 bucket '{_bucketName}'", s3Ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 health check failed for bucket '{BucketName}'", _bucketName);
            return HealthCheckResult.Unhealthy("S3 health check failed", ex, 
                new Dictionary<string, object> { ["bucket_name"] = _bucketName });
        }
    }
}