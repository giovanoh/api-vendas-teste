using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Infrastructure.Services;

public class CrudService<TEntity, IRepository, ITransaction>(IRepository repository, IUnitOfWork unitOfWork, ILogger<CrudService<TEntity, IRepository, IUnitOfWork>> logger)
    : ICrudService<TEntity>
    where IRepository : ICrudRepository<TEntity>
    where ITransaction : IUnitOfWork
{
    public async Task<Response<IEnumerable<TEntity>>> ListAsync()
    {
        try
        {
            return Response<IEnumerable<TEntity>>.Ok(await repository.ListAsync());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao listar os dados.");
            return Response<IEnumerable<TEntity>>.Fail(
                "Ocorreu um erro ao listar os dados",
                ErrorType.DatabaseError);
        }
    }

    public async Task<Response<TEntity>> FindByIdAsync(int id)
    {
        try
        {
            var model = await repository.FindByIdAsync(id);
            if (model == null)
                return Response<TEntity>.NotFound($"Recurso com id {id} não encontrado");

            return Response<TEntity>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao buscar o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao buscar o recurso",
                ErrorType.DatabaseError);
        }
    }

    public async Task<Response<TEntity>> AddAsync(TEntity model)
    {
        try
        {
            await repository.AddAsync(model);
            await unitOfWork.CompleteAsync();

            return Response<TEntity>.Ok(model);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao incluir o recurso no banco de dados.");
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao incluir o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado ao incluir o recurso.");
            return Response<TEntity>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    public async Task<Response<TEntity>> UpdateAsync(int id, TEntity model)
    {
        try
        {
            var modelExistente = await repository.FindByIdAsync(id);
            if (modelExistente == null)
                return Response<TEntity>.NotFound($"Recurso com id {id} não encontrado");

            AtualizaPropriedades(modelExistente, model);

            repository.Update(modelExistente);
            await unitOfWork.CompleteAsync();

            return Response<TEntity>.Ok(modelExistente);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao atualizar o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao atualizar o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado ao atualizar o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    public async Task<Response<TEntity>> DeleteAsync(int id)
    {
        try
        {
            var modelExistente = await repository.FindByIdAsync(id);
            if (modelExistente == null)
                return Response<TEntity>.NotFound($"Recurso com id {id} não encontrado");

            repository.Delete(modelExistente);
            await unitOfWork.CompleteAsync();

            return Response<TEntity>.Ok(modelExistente);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao excluir o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao excluir o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado ao excluir o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    protected virtual void AtualizaPropriedades(TEntity modeloExistente, TEntity modeloNovo)
    {
    }

}