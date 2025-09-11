using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.DTOs;
using Vendas.API.DTOs.Response;

namespace Vendas.API.Controllers;

public class CrudController<IService, TEntity, TInputDto, TOutputDto>(IService service, IMapper mapper)
    : ApiController
    where IService : ICrudService<TEntity>
    where TOutputDto : IdentificableDto
{
    protected virtual HashSet<string> SortableFields => ["id"];

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiProblemDetails), 500)]
    public async Task<IActionResult> GetPagedAsync([FromQuery] PagedRequest request)
    {
        if (!SortableFields.Contains(request.SortBy))
            return HandleErrorResponse(
                Response<IEnumerable<TOutputDto>>.Fail(
                    $"Campo '{request.SortBy}' não é ordenável. Possíveis campos: {string.Join(", ", SortableFields)}",
                    ErrorType.ValidationError));

        var result = await service.ListPagedAsync(request);
        if (!result.Success)
            return HandleErrorResponse(result);

        var data = mapper.Map<IEnumerable<TOutputDto>>(result.Model?.Data ?? []);

        var paginationInfo = PaginationInfo.Create(request, result.Model?.TotalCount ?? 0, data.Count());
        var meta = new Dictionary<string, object> { ["pagination"] = paginationInfo.ToMetaObject() };

        return Success(data, 200, meta);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), 404)]
    [ProducesResponseType(typeof(ApiProblemDetails), 500)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await service.FindByIdAsync(id);
        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<TOutputDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiProblemDetails), 404)]
    [ProducesResponseType(typeof(ApiProblemDetails), 500)]
    public async Task<IActionResult> CreateAsync([FromBody] TInputDto createDto)
    {
        var model = mapper.Map<TEntity>(createDto);

        var result = BeforeCreateEntity(createDto, model);
        if (!result.Success)
            return HandleErrorResponse(result);

        result = await service.AddAsync(model);
        if (!result.Success)
            return HandleErrorResponse(result);

        var modelDto = mapper.Map<TOutputDto>(result.Model);
        return Created("GetById", new { id = modelDto.Id }, modelDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), 404)]
    [ProducesResponseType(typeof(ApiProblemDetails), 500)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] TInputDto updateDto)
    {
        var cliente = mapper.Map<TEntity>(updateDto);

        var result = BeforeUpdateEntity(updateDto, cliente);
        if (!result.Success)
            return HandleErrorResponse(result);

        result = await service.UpdateAsync(id, cliente);
        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<TOutputDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), 404)]
    [ProducesResponseType(typeof(ApiProblemDetails), 500)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await service.DeleteAsync(id);

        if (!result.Success)
            return HandleErrorResponse(result);

        return NoContent();
    }

    protected virtual Response<TEntity> BeforeCreateEntity(TInputDto inputDto, TEntity entity)
    {
        return Response<TEntity>.Ok(entity);
    }

    protected virtual Response<TEntity> BeforeUpdateEntity(TInputDto inputDto, TEntity entity)
    {
        return Response<TEntity>.Ok(entity);
    }

}