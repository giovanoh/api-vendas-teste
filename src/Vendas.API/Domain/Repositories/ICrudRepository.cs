using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Domain.Repositories;

public interface ICrudRepository<TEntity>
{
    Task<(IEnumerable<TEntity> Items, int TotalCount)> ListPagedAsync(PagedRequest request);
    Task<TEntity?> FindByIdAsync(int id);
    Task AddAsync(TEntity model);
    void Update(TEntity model);
    void Delete(TEntity model);
}
