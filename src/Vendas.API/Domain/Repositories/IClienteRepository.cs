using Vendas.API.Domain.Models;

namespace Vendas.API.Domain.Repositories;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ListAsync();
    Task<Cliente?> FindByIdAsync(int clienteId);
    Task AddAsync(Cliente cliente);
    void Update(Cliente cliente);
    void Delete(Cliente cliente);
}
