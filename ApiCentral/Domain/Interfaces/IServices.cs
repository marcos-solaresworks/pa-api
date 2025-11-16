namespace ApiCentral.Domain.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType);
    Task<byte[]> DownloadFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    string GeneratePresignedUrl(string filePath, TimeSpan expiration);
}

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string routingKey) where T : class;
}

public interface IJwtService
{
    string GenerateToken(int userId, string email, string perfil);
    bool ValidateToken(string token);
    int GetUserIdFromToken(string token);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}