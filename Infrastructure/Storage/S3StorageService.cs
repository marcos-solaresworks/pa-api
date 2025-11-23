using Amazon.S3;
using Amazon.S3.Model;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Infrastructure.Storage;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"] ?? throw new ArgumentNullException("AWS:S3:BucketName");
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = new MemoryStream(fileContent),
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3Client.PutObjectAsync(request);
        return fileName;
    }

    public async Task<byte[]> DownloadFileAsync(string filePath)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath
        };

        using var response = await _s3Client.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath
        };

        await _s3Client.DeleteObjectAsync(request);
    }

    public string GeneratePresignedUrl(string filePath, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            Expires = DateTime.UtcNow.Add(expiration),
            Verb = HttpVerb.GET
        };

        return _s3Client.GetPreSignedURL(request);
    }
}