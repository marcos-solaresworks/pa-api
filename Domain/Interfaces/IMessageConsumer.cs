namespace ApiCentral.Domain.Interfaces;

public interface IMessageConsumer : IDisposable
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}