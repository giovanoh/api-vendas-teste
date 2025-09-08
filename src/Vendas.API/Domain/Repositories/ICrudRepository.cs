namespace Vendas.API.Domain.Repositories;

public interface ICrudRepository<TEntity>
{
    Task<IEnumerable<TEntity>> ListAsync();
    Task<TEntity?> FindByIdAsync(int id);
    Task AddAsync(TEntity model);
    void Update(TEntity model);
    void Delete(TEntity model);
}
