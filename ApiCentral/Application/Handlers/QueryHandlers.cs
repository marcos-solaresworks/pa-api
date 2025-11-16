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
            lote.Registros.Count,
            lote.Registros.Count, // Assumindo que todos foram processados para simplicidade
            lote.DataCriacao,
            lote.Logs.Select(l => new ProcessamentoLogDto(
                l.Mensagem ?? "",
                l.TipoLog,
                l.DataHora
            )).ToList()
        );
    }
}