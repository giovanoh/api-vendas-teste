using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class ClientesController(IClienteService clienteService, IMapper mapper) : ApiController
{

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await clienteService.ListAsync();
        if (!result.Success)
            return HandleErrorResponse(result);

        var clientesDto = mapper.Map<IEnumerable<ClienteDto>>(result.Model);
        return Success(clientesDto);
    }

    [HttpGet("{id}", Name = "GetClienteById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await clienteService.FindByIdAsync(id);
        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<ClienteDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] SaveClienteDto saveClienteDto)
    {
        var cliente = mapper.Map<Cliente>(saveClienteDto);
        var result = await clienteService.AddAsync(cliente);

        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<ClienteDto>(result.Model);
        return Created("GetClienteById", new { id = clienteDto.Id }, clienteDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] SaveClienteDto saveClienteDto)
    {
        var cliente = mapper.Map<Cliente>(saveClienteDto);
        var result = await clienteService.UpdateAsync(id, cliente);

        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = mapper.Map<ClienteDto>(result.Model);
        return Success(clienteDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await clienteService.DeleteAsync(id);

        if (!result.Success)
            return HandleErrorResponse(result);

        return NoContent();
    }
}