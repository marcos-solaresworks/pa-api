using MediatR;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Application.Handlers;

public class GetLotesByClienteQueryHandler : IRequestHandler<GetLotesByClienteQuery, IEnumerable<LoteProcessamentoDto>>
{
    private readonly ILoteProcessamentoRepository _loteRepository;
    private readonly IStorageService _storageService;

    public GetLotesByClienteQueryHandler(ILoteProcessamentoRepository loteRepository, IStorageService storageService)
    {
        _loteRepository = loteRepository;
        _storageService = storageService;
    }

    public async Task<IEnumerable<LoteProcessamentoDto>> Handle(GetLotesByClienteQuery request, CancellationToken cancellationToken)
    {
        var lotes = await _loteRepository.GetByClienteIdAsync(request.ClienteId);
        
        return lotes.Select(l => {
            // Gerar URL pré-assinada se o arquivo processado existir
            string? urlArquivoProcessado = null;
            if (!string.IsNullOrEmpty(l.CaminhoProcessadoS3))
            {
                try
                {
                    var filePath = l.CaminhoProcessadoS3.Replace("s3://grafica-mvp-storage-qb1g7tq6/", "");
                    urlArquivoProcessado = _storageService.GeneratePresignedUrl(filePath, TimeSpan.FromHours(1));
                }
                catch (Exception)
                {
                    // Se falhar ao gerar URL pré-assinada, retorna o caminho S3 original
                    urlArquivoProcessado = l.CaminhoProcessadoS3;
                }
            }
            
            return new LoteProcessamentoDto(
                l.Id,
                l.Cliente.Nome,
                l.NomeArquivo,
                l.Status,
                3, // RegistrosTotal - sempre 3 (enviado, processando, erro/concluído)
                LoteStatusHelper.GetRegistrosProcessadosByStatus(l.Status),
                l.DataCriacao,
                urlArquivoProcessado,
                new List<ProcessamentoLogDto>() // Logs vazios por performance
            );
        });
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
    private readonly IStorageService _storageService;

    public GetLotesQueryHandler(ILoteProcessamentoRepository loteRepository, IStorageService storageService)
    {
        _loteRepository = loteRepository;
        _storageService = storageService;
    }

    public async Task<IEnumerable<LoteProcessamentoDto>> Handle(GetLotesQuery request, CancellationToken cancellationToken)
    {
        var lotes = await _loteRepository.GetAllAsync();
        return lotes.Select(l => {
            // Gerar URL pré-assinada se o arquivo processado existir
            string? urlArquivoProcessado = null;
            if (!string.IsNullOrEmpty(l.CaminhoProcessadoS3))
            {
                try
                {
                    var filePath = l.CaminhoProcessadoS3.Replace("s3://grafica-mvp-storage-qb1g7tq6/", "");
                    urlArquivoProcessado = _storageService.GeneratePresignedUrl(filePath, TimeSpan.FromHours(1));
                }
                catch (Exception)
                {
                    // Se falhar ao gerar URL pré-assinada, retorna o caminho S3 original
                    urlArquivoProcessado = l.CaminhoProcessadoS3;
                }
            }
            
            return new LoteProcessamentoDto(
                l.Id,
                l.Cliente?.Nome ?? "",
                l.NomeArquivo,
                l.Status,
                3, // registrosTotal sempre 3
                LoteStatusHelper.GetRegistrosProcessadosByStatus(l.Status),
                l.DataCriacao,
                urlArquivoProcessado,
                new List<ProcessamentoLogDto>()
            );
        });
    }

}