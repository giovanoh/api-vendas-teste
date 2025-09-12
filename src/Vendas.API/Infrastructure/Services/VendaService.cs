using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;

namespace Vendas.API.Infrastructure.Services;

public class VendaService(IVendaRepository repository, IUnitOfWork unitOfWork, ILogger<CrudService<Venda, IVendaRepository, IUnitOfWork>> logger, ICacheService cacheService)
    : CrudService<Venda, IVendaRepository, IUnitOfWork>(repository, unitOfWork, logger, cacheService), IVendaService
{
    protected override void AtualizaPropriedades(Venda vendaExistente, Venda novaVenda)
    {
        vendaExistente.Data = novaVenda.Data;
        vendaExistente.ValorTotal = novaVenda.ValorTotal;
        vendaExistente.ClienteId = novaVenda.ClienteId;
        vendaExistente.Cliente = novaVenda.Cliente;
        vendaExistente.ValorTotal = novaVenda.ValorTotal;
        vendaExistente.Itens.AddRange(novaVenda.Itens);
    }
}