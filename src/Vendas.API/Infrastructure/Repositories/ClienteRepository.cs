using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.Infrastructure.Repositories;

public class ClienteRepository(ApiDbContext context) : IClienteRepository
{
    private readonly ApiDbContext _context = context;

    public async Task<IEnumerable<Cliente>> ListAsync()
    {
        return await _context.Clientes.AsNoTracking().ToListAsync();
    }

    public async Task<Cliente?> FindByIdAsync(int authorId)
    {
        return await _context.Clientes.FindAsync(authorId);
    }

    public async Task AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
    }

    public void Update(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
    }

    public void Delete(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
    }
}
