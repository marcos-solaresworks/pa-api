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

    public GetLoteByIdQueryHandler(ILoteProcessamentoRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<LoteProcessamentoDto?> Handle(GetLoteByIdQuery request, CancellationToken cancellationToken)
    {
        var lote = await _loteRepository.GetByIdWithDetailsAsync(request.Id);
        
        if (lote == null)
            return null;

        return new LoteProcessamentoDto(
            lote.Id,
            lote.Cliente.Nome,
            lote.NomeArquivo,
            lote.Status,
            3, // RegistrosTotal - sempre 3 (enviado, processando, erro/concluído)
            LoteStatusHelper.GetRegistrosProcessadosByStatus(lote.Status),
            lote.DataCriacao,
            lote.CaminhoProcessadoS3,
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
            usuario.UltimoLogin
        );
    }
}

public static class LoteStatusHelper
{
    public static int GetRegistrosProcessadosByStatus(string status)
    {
        return status switch
        {
            "Recebido" or "Enviado" => 1,           // Primeira etapa: arquivo enviado
            "Em Processamento" or "Processando" => 2, // Segunda etapa: em processamento  
            "Concluído" or "Sucesso" => 3,          // Terceira etapa: concluído com sucesso
            "Erro" or "Falha" or "Falhado" => 3,    // Terceira etapa: concluído com erro
            _ => 1 // Status desconhecido, assume como enviado
        };
    }
}