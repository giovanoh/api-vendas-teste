using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;

namespace Vendas.API.Infrastructure.Services;

public class ClienteService(IClienteRepository repository, IUnitOfWork unitOfWork, ILogger<CrudService<Cliente, IClienteRepository, IUnitOfWork>> logger, ICacheService cacheService)
    : CrudService<Cliente, IClienteRepository, IUnitOfWork>(repository, unitOfWork, logger, cacheService), IClienteService
{
    protected override void AtualizaPropriedades(Cliente clienteExistente, Cliente novoCliente)
    {
        clienteExistente.Nome = novoCliente.Nome;
        clienteExistente.Telefone = novoCliente.Telefone;
        clienteExistente.Empresa = novoCliente.Empresa;
    }
}