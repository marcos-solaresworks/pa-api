using Microsoft.EntityFrameworkCore;
using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Infrastructure.Data;

namespace ApiCentral.Infrastructure.Repositories;

public class LoteProcessamentoRepository : ILoteProcessamentoRepository
{
    private readonly ApiCentralDbContext _context;

    public LoteProcessamentoRepository(ApiCentralDbContext context)
    {
        _context = context;
    }

    public async Task<LoteProcessamento?> GetByIdAsync(int id)
    {
        return await _context.LotesProcessamento.FindAsync(id);
    }

    public async Task<LoteProcessamento?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.LotesProcessamento
            .Include(l => l.Cliente)
            .Include(l => l.Usuario)
            .Include(l => l.PerfilProcessamento)
            .Include(l => l.Registros)
            .Include(l => l.Logs)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LoteProcessamento>> GetAllAsync()
    {
        return await _context.LotesProcessamento
            .Include(l => l.Cliente)
            .Include(l => l.Usuario)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoteProcessamento>> GetByClienteAsync(int clienteId)
    {
        return await _context.LotesProcessamento
            .Include(l => l.Cliente)
            .Include(l => l.Usuario)
            .Where(l => l.ClienteId == clienteId)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoteProcessamento>> GetByClienteIdAsync(int clienteId)
    {
        return await _context.LotesProcessamento
            .Include(l => l.Cliente)
            .Include(l => l.Usuario)
            .Include(l => l.PerfilProcessamento)
            .Where(l => l.ClienteId == clienteId)
            .OrderByDescending(l => l.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoteProcessamento>> GetByUsuarioAsync(int usuarioId)
    {
        return await _context.LotesProcessamento
            .Include(l => l.Cliente)
            .Include(l => l.Usuario)
            .Where(l => l.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<LoteProcessamento> AddAsync(LoteProcessamento lote)
    {
        _context.LotesProcessamento.Add(lote);
        await _context.SaveChangesAsync();
        return lote;
    }

    public async Task<LoteProcessamento> UpdateAsync(LoteProcessamento lote)
    {
        _context.Entry(lote).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return lote;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lote = await GetByIdAsync(id);
        if (lote == null)
            return false;

        _context.LotesProcessamento.Remove(lote);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync()
    {
        return await _context.LotesProcessamento.CountAsync();
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        return await _context.LotesProcessamento.CountAsync(l => l.Status == status);
    }

    public async Task<object> GetProcessingStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var stats = await _context.LotesProcessamento
            .Where(l => l.DataCriacao >= startDate && l.DataCriacao <= endDate)
            .GroupBy(l => l.DataCriacao.Date)
            .Select(g => new
            {
                Data = g.Key,
                TotalLotes = g.Count(),
                LotesConcluidos = g.Count(l => l.Status == "Concluído"),
                LotesComErro = g.Count(l => l.Status == "Erro")
            })
            .OrderBy(s => s.Data)
            .ToListAsync();

        return stats;
    }
}

public class ProcessamentoLogRepository : IProcessamentoLogRepository
{
    private readonly ApiCentralDbContext _context;

    public ProcessamentoLogRepository(ApiCentralDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessamentoLog> AddAsync(ProcessamentoLog log)
    {
        _context.ProcessamentoLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<ProcessamentoLog>> GetByLoteAsync(int loteId)
    {
        return await _context.ProcessamentoLogs
            .Where(l => l.LoteProcessamentoId == loteId)
            .OrderBy(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessamentoLog>> GetByLoteIdAsync(int loteId)
    {
        return await _context.ProcessamentoLogs
            .Where(l => l.LoteProcessamentoId == loteId)
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.ProcessamentoLogs.CountAsync();
    }

    public async Task<int> CountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.ProcessamentoLogs
            .CountAsync(l => l.DataHora >= today && l.DataHora < today.AddDays(1));
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        return await _context.ProcessamentoLogs.CountAsync(l => l.TipoLog == status);
    }
}