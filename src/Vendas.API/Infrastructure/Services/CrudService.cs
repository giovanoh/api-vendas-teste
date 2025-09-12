using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Infrastructure.Services;

public class CrudService<TEntity, IRepository, ITransaction>(IRepository repository, IUnitOfWork unitOfWork, ILogger<CrudService<TEntity, IRepository, IUnitOfWork>> logger, ICacheService cacheService)
    : ICrudService<TEntity>
    where TEntity : Entity
    where IRepository : ICrudRepository<TEntity>
    where ITransaction : IUnitOfWork
{
    private readonly string _entityName = typeof(TEntity).Name.ToLower();

    private string GeneratePagedListKey(PagedRequest request)
        => $"{_entityName}_paged_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortOrder}";

    private string GenerateEntityKey(int id)
        => $"{_entityName}_{id}";

    private string GenerateEntityListPattern()
        => $"{_entityName}_";

    private async Task InvalidateEntityCacheAsync()
    {
        var pattern = GenerateEntityListPattern();
        await cacheService.RemoveByPatternAsync(pattern);
        logger.LogDebug("Cache invalidado para entidade: {EntityName}", _entityName);
    }

    private async Task<Response<PagedResult<TEntity>>> FetchPagedDataAsync(PagedRequest request)
    {
        var (items, totalCount) = await repository.ListPagedAsync(request);

        var pagedResult = new PagedResult<TEntity>
        {
            Data = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Response<PagedResult<TEntity>>.Ok(pagedResult);
    }

    private async Task<Response<TEntity>> FetchEntityByIdAsync(int id)
    {
        var model = await repository.FindByIdAsync(id);
        if (model == null)
            return Response<TEntity>.NotFound($"Recurso com id {id} não encontrado");

        return Response<TEntity>.Ok(model);
    }

    public async Task<Response<PagedResult<TEntity>>> ListPagedAsync(PagedRequest request)
    {
        try
        {
            if (cacheService.IsEnabled)
            {
                var cacheKey = GeneratePagedListKey(request);
                return await cacheService.GetOrSetAsync(cacheKey, () => FetchPagedDataAsync(request));
            }

            return await FetchPagedDataAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao listar os dados.");
            return Response<PagedResult<TEntity>>.Fail(
                "Ocorreu um erro ao listar os dados",
                ErrorType.DatabaseError);
        }
    }

    public async Task<Response<TEntity>> FindByIdAsync(int id)
    {
        try
        {
            if (cacheService.IsEnabled)
            {
                var cacheKey = GenerateEntityKey(id);
                return await cacheService.GetOrSetAsync(cacheKey, () => FetchEntityByIdAsync(id));
            }

            return await FetchEntityByIdAsync(id);
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

            var modelResult = await repository.FindByIdAsync(model.Id);

            await InvalidateEntityCacheAsync();

            return Response<TEntity>.Ok(modelResult!);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao incluir o recurso no banco de dados.");
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao incluir o recurso",
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

            await InvalidateEntityCacheAsync();

            return Response<TEntity>.Ok(modelExistente);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao atualizar o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao atualizar o recurso",
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

            await InvalidateEntityCacheAsync();

            return Response<TEntity>.Ok(modelExistente);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Ocorreu um erro ao excluir o recurso com id {Id}.", id);
            return Response<TEntity>.Fail(
                "Ocorreu um erro ao excluir o recurso",
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