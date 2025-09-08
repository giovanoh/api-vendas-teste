using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class CrudController<IService, TEntity, TInputDto, TOutputDto>(IService service, IMapper mapper)
    : ApiController
    where IService : ICrudService<TEntity>
    where TOutputDto : IdentificableDto
{

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await service.ListAsync();
        if (!result.Success)
            return HandleErrorResponse(result);

        var clientesDto = mapper.Map<IEnumerable<TOutputDto>>(result.Model);
        return Success(clientesDto);
    }

    [HttpGet("{id}", Name = "GetById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await service.FindByIdAsync(id);
        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<TOutputDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] TInputDto createDto)
    {
        var model = mapper.Map<TEntity>(createDto);
        var result = await service.AddAsync(model);

        if (!result.Success)
            return HandleErrorResponse(result);

        var modelDto = mapper.Map<TOutputDto>(result.Model);
        return Created("GetById", new { id = modelDto.Id }, modelDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] TInputDto updateDto)
    {
        var cliente = mapper.Map<TEntity>(updateDto);
        var result = await service.UpdateAsync(id, cliente);

        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<TOutputDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await service.DeleteAsync(id);

        if (!result.Success)
            return HandleErrorResponse(result);

        return NoContent();
    }
}