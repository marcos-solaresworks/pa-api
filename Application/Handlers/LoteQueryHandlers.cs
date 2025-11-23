using MediatR;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Application.Handlers;

public class GetLotesByClienteQueryHandler : IRequestHandler<GetLotesByClienteQuery, IEnumerable<LoteProcessamentoDto>>
{
    private readonly ILoteProcessamentoRepository _loteRepository;

    public GetLotesByClienteQueryHandler(ILoteProcessamentoRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<IEnumerable<LoteProcessamentoDto>> Handle(GetLotesByClienteQuery request, CancellationToken cancellationToken)
    {
        var lotes = await _loteRepository.GetByClienteIdAsync(request.ClienteId);
        
        return lotes.Select(l => new LoteProcessamentoDto(
            l.Id,
            l.Cliente.Nome,
            l.NomeArquivo,
            l.Status,
            3, // RegistrosTotal - sempre 3 (enviado, processando, erro/concluído)
            LoteStatusHelper.GetRegistrosProcessadosByStatus(l.Status),
            l.DataCriacao,
            l.CaminhoProcessadoS3,
            new List<ProcessamentoLogDto>() // Logs vazios por performance
        ));
    }
}

public class GetLoteLogsQueryHandler : IRequestHandler<GetLoteLogsQuery, IEnumerable<ProcessamentoLogDto>>
{
    private readonly IProcessamentoLogRepository _logRepository;

    public GetLoteLogsQueryHandler(IProcessamentoLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<IEnumerable<ProcessamentoLogDto>> Handle(GetLoteLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetByLoteIdAsync(request.LoteId);
        
        return logs.Select(l => new ProcessamentoLogDto(
            l.Mensagem ?? string.Empty,
            l.TipoLog,
            l.DataHora
        ));
    }
}

public class GetLotesQueryHandler : IRequestHandler<GetLotesQuery, IEnumerable<LoteProcessamentoDto>>
{
    private readonly ILoteProcessamentoRepository _loteRepository;

    public GetLotesQueryHandler(ILoteProcessamentoRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<IEnumerable<LoteProcessamentoDto>> Handle(GetLotesQuery request, CancellationToken cancellationToken)
    {
        var lotes = await _loteRepository.GetAllAsync();
        return lotes.Select(l => new LoteProcessamentoDto(
            l.Id,
            l.Cliente?.Nome ?? "",
            l.NomeArquivo,
            l.Status,
            3, // registrosTotal sempre 3
            LoteStatusHelper.GetRegistrosProcessadosByStatus(l.Status),
            l.DataCriacao,
            l.CaminhoProcessadoS3,
            new List<ProcessamentoLogDto>()
        ));
    }

}