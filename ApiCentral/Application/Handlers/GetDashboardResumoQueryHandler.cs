using MediatR;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;

namespace ApiCentral.Application.Handlers
{
    public class GetDashboardResumoQueryHandler : IRequestHandler<GetDashboardResumoQuery, DashboardResumoDto>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ILoteProcessamentoRepository _loteRepository;
        private readonly IProcessamentoLogRepository _logRepository;

        public GetDashboardResumoQueryHandler(
            IClienteRepository clienteRepository,
            ILoteProcessamentoRepository loteRepository,
            IProcessamentoLogRepository logRepository)
        {
            _clienteRepository = clienteRepository;
            _loteRepository = loteRepository;
            _logRepository = logRepository;
        }

        public async Task<DashboardResumoDto> Handle(GetDashboardResumoQuery request, CancellationToken cancellationToken)
        {
            var totalClientes = await _clienteRepository.CountAsync();
            var clientesAtivos = await _clienteRepository.CountActiveAsync();
            
            var totalLotes = await _loteRepository.CountAsync();
            var lotesProcessando = await _loteRepository.CountByStatusAsync("Processando");
            var lotesConcluidos = await _loteRepository.CountByStatusAsync("Concluído");
            var lotesComErro = await _loteRepository.CountByStatusAsync("Erro");
            
            var totalProcessamentos = await _logRepository.CountAsync();
            var processamentosHoje = await _logRepository.CountTodayAsync();
            var processamentosSucesso = await _logRepository.CountByStatusAsync("Sucesso");
            var processamentosErro = await _logRepository.CountByStatusAsync("Erro");

            // Estatísticas dos últimos 30 dias
            var dataInicio = DateTime.UtcNow.AddDays(-30);
            var estatisticasUltimos30Dias = await _loteRepository.GetProcessingStatisticsAsync(dataInicio, DateTime.UtcNow);

            return new DashboardResumoDto(
                totalClientes,
                clientesAtivos,
                totalLotes,
                lotesProcessando,
                lotesConcluidos,
                lotesComErro,
                totalProcessamentos,
                processamentosHoje,
                processamentosSucesso,
                processamentosErro,
                estatisticasUltimos30Dias
            );
        }
    }
}