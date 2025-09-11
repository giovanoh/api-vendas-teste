using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.Infrastructure.Repositories;

public class ProdutoRepository(ApiDbContext context)
    : CrudRepository<Produto>(context), IProdutoRepository
{
}