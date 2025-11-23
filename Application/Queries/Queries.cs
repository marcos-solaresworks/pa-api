using MediatR;
using ApiCentral.Application.DTOs;

namespace ApiCentral.Application.Queries;

public record GetUsuarioByIdQuery(int Id) : IRequest<UsuarioDto?>;

public record GetClientesQuery() : IRequest<List<ClienteDto>>;

public record GetClienteByIdQuery(int Id) : IRequest<ClienteDto?>;

public record GetPerfilProcessamentoQuery() : IRequest<List<PerfilProcessamentoDto>>;

public record GetPerfilProcessamentoByIdQuery(int Id) : IRequest<PerfilProcessamentoDto?>;

public record GetLoteByIdQuery(int Id) : IRequest<LoteProcessamentoDto?>;

public record GetDashboardResumoQuery() : IRequest<DashboardResumoDto>;