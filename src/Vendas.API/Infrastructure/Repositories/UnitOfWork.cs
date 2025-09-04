using Vendas.API.Domain.Repositories;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.Infrastructure.Repositories;

public class UnitOfWork(ApiDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync() => await context.SaveChangesAsync();
}
