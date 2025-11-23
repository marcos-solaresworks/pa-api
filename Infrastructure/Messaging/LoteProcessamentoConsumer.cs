using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Domain.Entities;

namespace ApiCentral.Infrastructure.Messaging;

public class LoteProcessamentoConsumer : IMessageConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<LoteProcessamentoConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const string QueueName = "lote.processamento";

    public LoteProcessamentoConsumer(
        IConnection connection,
        ILogger<LoteProcessamentoConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _channel = _connection.CreateModel();
        
        // Configurar exchange, fila e binding
        _channel.ExchangeDeclare("graficaltda.exchange", ExchangeType.Topic, true);
        _channel.QueueDeclare(QueueName, true, false, false, null);
        _channel.QueueBind(QueueName, "graficaltda.exchange", "lote.processamento", null);
        _channel.BasicQos(0, 1, false); // Processar uma mensagem por vez
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                _logger.LogInformation("Recebida mensagem para processamento: {Message}", message);
                
                var loteData = JsonSerializer.Deserialize<LoteProcessamentoMessage>(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (loteData != null)
                {
                    await ProcessarLote(loteData);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Lote {LoteId} processado com sucesso", loteData.LoteId);
                }
                else
                {
                    _logger.LogWarning("Mensagem inválida recebida: {Message}", message);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);
                _channel.BasicNack(ea.DeliveryTag, false, true); // Rejeitar e reenviar para fila
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        
        _logger.LogInformation("Consumer iniciado para fila {QueueName}", QueueName);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parando consumer...");
        await Task.CompletedTask;
    }

    private async Task ProcessarLote(LoteProcessamentoMessage loteData)
    {
        using var scope = _serviceProvider.CreateScope();
        var loteRepository = scope.ServiceProvider.GetRequiredService<ILoteProcessamentoRepository>();
        var logRepository = scope.ServiceProvider.GetRequiredService<IProcessamentoLogRepository>();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var perfilRepository = scope.ServiceProvider.GetRequiredService<IPerfilProcessamentoRepository>();

        try
        {
            // Buscar o lote
            var lote = await loteRepository.GetByIdAsync(loteData.LoteId);
            if (lote == null)
            {
                _logger.LogWarning("Lote {LoteId} não encontrado", loteData.LoteId);
                return;
            }

            // Atualizar status para "Processando"
            lote.Status = "Processando";
            await loteRepository.UpdateAsync(lote);
            
            await AdicionarLog(logRepository, loteData.LoteId, "Iniciando processamento do lote", "Info");

            // Baixar arquivo do S3
            var arquivoBytes = await storageService.DownloadFileAsync(loteData.CaminhoS3);
            var conteudoArquivo = Encoding.UTF8.GetString(arquivoBytes);

            // Buscar perfil de processamento
            var perfil = await perfilRepository.GetByIdAsync(loteData.PerfilId);
            if (perfil == null)
            {
                throw new Exception($"Perfil de processamento {loteData.PerfilId} não encontrado");
            }

            // Processar arquivo baseado no tipo
            var registrosProcessados = await ProcessarArquivo(conteudoArquivo, perfil);
            
            await AdicionarLog(logRepository, loteData.LoteId, $"Processados {registrosProcessados} registros", "Info");

            // Simular processamento PCL (aqui você implementaria a lógica real)
            await SimularProcessamentoPCL(loteData, registrosProcessados);

            // Atualizar status para "Concluído"
            lote.Status = "Concluido";
            lote.DataProcessamento = DateTime.UtcNow;
            await loteRepository.UpdateAsync(lote);
            
            await AdicionarLog(logRepository, loteData.LoteId, "Processamento concluído com sucesso", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento do lote {LoteId}", loteData.LoteId);
            
            // Atualizar status para "Erro"
            var lote = await loteRepository.GetByIdAsync(loteData.LoteId);
            if (lote != null)
            {
                lote.Status = "Erro";
                await loteRepository.UpdateAsync(lote);
            }
            
            await AdicionarLog(logRepository, loteData.LoteId, $"Erro no processamento: {ex.Message}", "Error");
            throw;
        }
    }

    private async Task<int> ProcessarArquivo(string conteudo, PerfilProcessamento perfil)
    {
        var linhas = conteudo.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var registrosProcessados = 0;

        foreach (var linha in linhas.Skip(1)) // Pular cabeçalho
        {
            if (string.IsNullOrWhiteSpace(linha)) continue;

            var campos = linha.Split(perfil.Delimitador ?? ",");
            
            // Aqui você processaria cada registro conforme o template PCL
            // Por agora, apenas simular
            registrosProcessados++;
        }

        // Simular tempo de processamento
        await Task.Delay(100);
        
        return registrosProcessados;
    }

    private async Task SimularProcessamentoPCL(LoteProcessamentoMessage loteData, int registros)
    {
        // Simular geração de arquivos PCL
        _logger.LogInformation("Gerando {Registros} arquivos PCL para lote {LoteId}", registros, loteData.LoteId);
        
        // Simular tempo de processamento baseado na quantidade de registros
        var tempoProcessamento = Math.Min(registros * 10, 5000); // Max 5 segundos
        await Task.Delay(tempoProcessamento);
        
        _logger.LogInformation("Arquivos PCL gerados com sucesso para lote {LoteId}", loteData.LoteId);
    }

    private async Task AdicionarLog(IProcessamentoLogRepository logRepository, int loteId, string mensagem, string tipo)
    {
        var log = new ProcessamentoLog
        {
            LoteProcessamentoId = loteId,
            Mensagem = mensagem,
            TipoLog = tipo,
            DataHora = DateTime.UtcNow
        };
        
        await logRepository.AddAsync(log);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}

public class LoteProcessamentoMessage
{
    public int LoteId { get; set; }
    public int ClienteId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoS3 { get; set; } = string.Empty;
    public int PerfilId { get; set; }
}