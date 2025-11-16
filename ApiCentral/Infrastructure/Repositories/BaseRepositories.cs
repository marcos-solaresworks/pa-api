using Microsoft.EntityFrameworkCore;
using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Infrastructure.Data;

namespace ApiCentral.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApiCentralDbContext _context;

    public UsuarioRepository(ApiCentralDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        _context.Entry(usuario).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await GetByIdAsync(id);
        if (usuario == null)
            return false;

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == id);
    }
}

public class ClienteRepository : IClienteRepository
{
    private readonly ApiCentralDbContext _context;

    public ClienteRepository(ApiCentralDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<Cliente?> GetByEmailAsync(string email)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _context.Clientes.ToListAsync();
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        _context.Entry(cliente).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await GetByIdAsync(id);
        if (cliente == null)
            return false;

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Clientes.AnyAsync(c => c.Id == id);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Clientes.CountAsync();
    }

    public async Task<int> CountActiveAsync()
    {
        return await _context.Clientes.CountAsync(c => c.Ativo);
    }
}