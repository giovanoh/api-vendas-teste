using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core;

using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.Infrastructure.Repositories;

public class CrudRepository<TEntity> : ICrudRepository<TEntity>
    where TEntity : class
{
    protected readonly DbSet<TEntity> dbSet;
    protected readonly ApiDbContext context;

    public CrudRepository(ApiDbContext context)
    {
        this.context = context;
        this.dbSet = context.Set<TEntity>();
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> ListPagedAsync(PagedRequest request)
    {
        var query = dbSet.AsNoTracking().AsQueryable();

        // Aplicar ordenação se especificada
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            var orderDirection = request.SortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? "desc" : "asc";
            query = query.OrderBy($"{request.SortBy} {orderDirection}");
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual async Task<TEntity?> FindByIdAsync(int id) => await dbSet.FindAsync(id);

    public async Task AddAsync(TEntity model) => await dbSet.AddAsync(model);

    public void Update(TEntity model) => dbSet.Update(model);

    public void Delete(TEntity model) => dbSet.Remove(model);
}
