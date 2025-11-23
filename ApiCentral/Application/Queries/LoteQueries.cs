using MediatR;
using ApiCentral.Application.DTOs;

namespace ApiCentral.Application.Queries;

public record GetLotesByClienteQuery(int ClienteId) : IRequest<IEnumerable<LoteProcessamentoDto>>;

public record GetLoteLogsQuery(int LoteId) : IRequest<IEnumerable<ProcessamentoLogDto>>;

public record GetLotesQuery() : IRequest<IEnumerable<LoteProcessamentoDto>>;