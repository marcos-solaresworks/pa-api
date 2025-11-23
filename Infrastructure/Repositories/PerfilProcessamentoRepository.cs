using Microsoft.EntityFrameworkCore;
using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Infrastructure.Data;

namespace ApiCentral.Infrastructure.Repositories
{
    public class PerfilProcessamentoRepository : IPerfilProcessamentoRepository
    {
        private readonly ApiCentralDbContext _context;

        public PerfilProcessamentoRepository(ApiCentralDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilProcessamento?> GetByIdAsync(int id)
        {
            return await _context.PerfisProcessamento
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PerfilProcessamento>> GetAllAsync()
        {
            return await _context.PerfisProcessamento
                .Include(p => p.Cliente)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerfilProcessamento>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.PerfisProcessamento
                .Include(p => p.Cliente)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<PerfilProcessamento> AddAsync(PerfilProcessamento perfilProcessamento)
        {
            _context.PerfisProcessamento.Add(perfilProcessamento);
            await _context.SaveChangesAsync();
            return perfilProcessamento;
        }

        public async Task<PerfilProcessamento> UpdateAsync(PerfilProcessamento perfilProcessamento)
        {
            _context.Entry(perfilProcessamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return perfilProcessamento;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var perfilProcessamento = await _context.PerfisProcessamento.FindAsync(id);
            if (perfilProcessamento == null)
                return false;

            _context.PerfisProcessamento.Remove(perfilProcessamento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PerfisProcessamento.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> HasProcessingBatchesAsync(int perfilId)
        {
            return await _context.LotesProcessamento
                .AnyAsync(l => l.PerfilProcessamentoId == perfilId);
        }
    }
}