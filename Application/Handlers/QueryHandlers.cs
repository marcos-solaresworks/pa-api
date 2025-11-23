using MediatR;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Application.Handlers;

public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, List<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository;

    public GetClientesQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<List<ClienteDto>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.GetAllAsync();
        
        return clientes.Select(c => new ClienteDto(
            c.Id,
            c.Nome,
            c.Email,
            c.Telefone,
            c.Empresa,
            c.DataCriacao,
            c.Ativo
        )).ToList();
    }
}

public class GetPerfilProcessamentoQueryHandler : IRequestHandler<GetPerfilProcessamentoQuery, List<PerfilProcessamentoDto>>
{
    private readonly IPerfilProcessamentoRepository _perfilRepository;

    public GetPerfilProcessamentoQueryHandler(IPerfilProcessamentoRepository perfilRepository)
    {
        _perfilRepository = perfilRepository;
    }

    public async Task<List<PerfilProcessamentoDto>> Handle(GetPerfilProcessamentoQuery request, CancellationToken cancellationToken)
    {
        var perfis = await _perfilRepository.GetAllAsync();
        
        return perfis.Select(p => new PerfilProcessamentoDto(
            p.Id,
            p.Nome,
            p.Descricao,
            p.ClienteId,
            p.TipoArquivo,
            p.Delimitador,
            p.TemplatePcl
        )).ToList();
    }
}

public class GetLoteByIdQueryHandler : IRequestHandler<GetLoteByIdQuery, LoteProcessamentoDto?>
{
    private readonly ILoteProcessamentoRepository _loteRepository;
    private readonly IStorageService _storageService;

    public GetLoteByIdQueryHandler(ILoteProcessamentoRepository loteRepository, IStorageService storageService)
    {
        _loteRepository = loteRepository;
        _storageService = storageService;
    }

    public async Task<LoteProcessamentoDto?> Handle(GetLoteByIdQuery request, CancellationToken cancellationToken)
    {
        var lote = await _loteRepository.GetByIdWithDetailsAsync(request.Id);
        
        if (lote == null)
            return null;

        // Gerar URL pré-assinada se o arquivo processado existir
        string? urlArquivoProcessado = null;
        if (!string.IsNullOrEmpty(lote.CaminhoProcessadoS3))
        {
            try
            {
                // Extrair apenas o caminho do arquivo (remover s3://bucket-name/)
                var filePath = lote.CaminhoProcessadoS3.Replace("s3://grafica-mvp-storage-qb1g7tq6/", "");
                urlArquivoProcessado = _storageService.GeneratePresignedUrl(filePath, TimeSpan.FromHours(1));
            }
            catch (Exception)
            {
                // Se falhar ao gerar URL pré-assinada, retorna o caminho S3 original
                urlArquivoProcessado = lote.CaminhoProcessadoS3;
            }
        }

        return new LoteProcessamentoDto(
            lote.Id,
            lote.Cliente.Nome,
            lote.NomeArquivo,
            lote.Status,
            3, // RegistrosTotal - sempre 3 (enviado, processando, erro/concluído)
            LoteStatusHelper.GetRegistrosProcessadosByStatus(lote.Status),
            lote.DataCriacao,
            urlArquivoProcessado,
            lote.Logs.Select(l => new ProcessamentoLogDto(
                l.Mensagem ?? "",
                l.TipoLog,
                l.DataHora
            )).ToList()
        );
    }
}

public class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public GetUsuarioByIdQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.Id);
        if (usuario == null)
            return null;

        return new UsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil,
            usuario.DataCriacao
        );
    }
}

public static class LoteStatusHelper
{
    public static int GetRegistrosProcessadosByStatus(string status)
    {
        return status.ToLower() switch
        {
            "recebido" or "enviado" => 1,           // Primeira etapa: arquivo enviado
            "em processamento" or "processando" => 2, // Segunda etapa: em processamento  
            "concluído" or "concluido" or "sucesso" => 3, // Terceira etapa: concluído com sucesso
            "erro" or "falha" or "falhado" => 3,     // Terceira etapa: concluído com erro
            _ => 1 // Status desconhecido, assume como enviado
        };
    }
}