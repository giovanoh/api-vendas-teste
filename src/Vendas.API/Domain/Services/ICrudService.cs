using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Domain.Services;

public interface ICrudService<TEntity>
{
    Task<Response<IEnumerable<TEntity>>> ListAsync();
    Task<Response<TEntity>> FindByIdAsync(int id);
    Task<Response<TEntity>> AddAsync(TEntity model);
    Task<Response<TEntity>> UpdateAsync(int id, TEntity model);
    Task<Response<TEntity>> DeleteAsync(int id);
}

