using ApiCentral.Domain.Entities;

namespace ApiCentral.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario> AddAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(int id);
    Task<Cliente?> GetByEmailAsync(string email);
    Task<IEnumerable<Cliente>> GetAllAsync();
    Task<Cliente> AddAsync(Cliente cliente);
    Task<Cliente> UpdateAsync(Cliente cliente);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<int> CountActiveAsync();
}

public interface IPerfilProcessamentoRepository
{
    Task<PerfilProcessamento?> GetByIdAsync(int id);
    Task<IEnumerable<PerfilProcessamento>> GetAllAsync();
    Task<IEnumerable<PerfilProcessamento>> GetByClienteIdAsync(int clienteId);
    Task<PerfilProcessamento> AddAsync(PerfilProcessamento perfil);
    Task<PerfilProcessamento> UpdateAsync(PerfilProcessamento perfil);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasProcessingBatchesAsync(int perfilId);
}

public interface ILoteProcessamentoRepository
{
    Task<LoteProcessamento?> GetByIdAsync(int id);
    Task<LoteProcessamento?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<LoteProcessamento>> GetAllAsync();
    Task<IEnumerable<LoteProcessamento>> GetByClienteAsync(int clienteId);
    Task<IEnumerable<LoteProcessamento>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<LoteProcessamento>> GetByUsuarioAsync(int usuarioId);
    Task<LoteProcessamento> AddAsync(LoteProcessamento lote);
    Task<LoteProcessamento> UpdateAsync(LoteProcessamento lote);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
    Task<int> CountByStatusAsync(string status);
    Task<object> GetProcessingStatisticsAsync(DateTime startDate, DateTime endDate);
}

public interface IProcessamentoLogRepository
{
    Task<ProcessamentoLog> AddAsync(ProcessamentoLog log);
    Task<IEnumerable<ProcessamentoLog>> GetByLoteAsync(int loteId);
    Task<IEnumerable<ProcessamentoLog>> GetByLoteIdAsync(int loteId);
    Task<int> CountAsync();
    Task<int> CountTodayAsync();
    Task<int> CountByStatusAsync(string status);
}