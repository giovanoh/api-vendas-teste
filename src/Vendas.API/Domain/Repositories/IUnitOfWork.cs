namespace Vendas.API.Domain.Repositories;

public interface IUnitOfWork
{
     Task CompleteAsync();
}