using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Domain.Services;

public interface IClienteService
{
    Task<Response<IEnumerable<Cliente>>> ListAsync();
    Task<Response<Cliente>> FindByIdAsync(int clienteId);
    Task<Response<Cliente>> AddAsync(Cliente cliente);
    Task<Response<Cliente>> UpdateAsync(int clienteId, Cliente cliente);
    Task<Response<Cliente>> DeleteAsync(int clienteId);
}

