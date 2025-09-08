using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Repositories;

namespace Vendas.API.Infrastructure.Repositories;

public class CrudRepository<TEntity>(DbSet<TEntity> dbSet) : ICrudRepository<TEntity>
    where TEntity : class
{
    public async Task AddAsync(TEntity model) => await dbSet.AddAsync(model);
    public void Delete(TEntity model) => dbSet.Remove(model);
    public async Task<TEntity?> FindByIdAsync(int id) => await dbSet.FindAsync(id);
    public async Task<IEnumerable<TEntity>> ListAsync() => await dbSet.ToListAsync();
    public void Update(TEntity model) => dbSet.Update(model);
}
