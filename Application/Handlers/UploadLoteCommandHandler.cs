using MediatR;
using ApiCentral.Application.Commands;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Exceptions;

namespace ApiCentral.Application.Handlers;

public class UploadLoteCommandHandler : IRequestHandler<UploadLoteCommand, UploadLoteResponse>
{
    private readonly ILoteProcessamentoRepository _loteRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPerfilProcessamentoRepository _perfilRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IStorageService _storageService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProcessamentoLogRepository _logRepository;

    public UploadLoteCommandHandler(
        ILoteProcessamentoRepository loteRepository,
        IClienteRepository clienteRepository,
        IPerfilProcessamentoRepository perfilRepository,
        IUsuarioRepository usuarioRepository,
        IStorageService storageService,
        IMessagePublisher messagePublisher,
        IProcessamentoLogRepository logRepository)
    {
        _loteRepository = loteRepository;
        _clienteRepository = clienteRepository;
        _perfilRepository = perfilRepository;
        _usuarioRepository = usuarioRepository;
        _storageService = storageService;
        _messagePublisher = messagePublisher;
        _logRepository = logRepository;
    }

    public async Task<UploadLoteResponse> Handle(UploadLoteCommand request, CancellationToken cancellationToken)
    {
        // Validar se cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
        if (cliente == null)
            throw new NotFoundException("Cliente", request.ClienteId);

        // Validar se perfil existe
        var perfil = await _perfilRepository.GetByIdAsync(request.PerfilProcessamentoId);
        if (perfil == null)
            throw new NotFoundException("PerfilProcessamento", request.PerfilProcessamentoId);

        // Validar se usuário existe
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
        if (usuario == null)
            throw new NotFoundException("Usuario", request.UsuarioId);

        // Decodificar arquivo
        var fileBytes = Convert.FromBase64String(request.ArquivoBase64);
        
        // Upload para S3
        var s3Path = await _storageService.UploadFileAsync(
            $"lotes/{Guid.NewGuid()}/{request.NomeArquivo}",
            fileBytes,
            "application/octet-stream"
        );

        // Criar lote
        var lote = new LoteProcessamento
        {
            ClienteId = request.ClienteId,
            UsuarioId = request.UsuarioId,
            PerfilProcessamentoId = request.PerfilProcessamentoId,
            NomeArquivo = request.NomeArquivo,
            CaminhoS3 = s3Path,
            Status = "Recebido"
        };

        await _loteRepository.AddAsync(lote);

        // Adicionar log
        var log = new ProcessamentoLog
        {
            LoteProcessamentoId = lote.Id,
            Mensagem = "Upload realizado com sucesso",
            TipoLog = "Info"
        };
        await _logRepository.AddAsync(log);

        // Publicar mensagem no RabbitMQ
        var message = new
        {
            LoteId = lote.Id,
            ClienteId = request.ClienteId,
            NomeArquivo = request.NomeArquivo,
            CaminhoS3 = s3Path,
            PerfilId = request.PerfilProcessamentoId
        };

        await _messagePublisher.PublishAsync(message, "lote.processamento");

        return new UploadLoteResponse(
            lote.Id,
            lote.Status,
            "Arquivo enviado para processamento com sucesso",
            lote.DataCriacao
        );
    }
}