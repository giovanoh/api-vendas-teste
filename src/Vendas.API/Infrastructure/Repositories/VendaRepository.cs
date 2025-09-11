using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.Infrastructure.Repositories;

public class VendaRepository(ApiDbContext context)
    : CrudRepository<Venda>(context), IVendaRepository
{
    public override async Task<Venda?> FindByIdAsync(int id) => await context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id);
}