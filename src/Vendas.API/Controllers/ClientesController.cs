using Microsoft.AspNetCore.Mvc;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class ClientesController(IClienteService clienteService) : ApiController
{

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await clienteService.ListAsync();
        if (!result.Success)
            return HandleErrorResponse(result);

        var clientesDto = new List<ClienteDto>();
        foreach (var cliente in result.Model!)
            clientesDto.Add(new ClienteDto { Id = cliente.Id, Nome = cliente.Nome, Telefone = cliente.Telefone, Empresa = cliente.Empresa });
        return Success(clientesDto);
    }

    [HttpGet("{id}", Name = "GetClienteById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await clienteService.FindByIdAsync(id);
        if (!result.Success)
            return HandleErrorResponse(result);

        var authorDto = new ClienteDto { Id = result.Model!.Id, Nome = result.Model.Nome, Telefone = result.Model.Telefone, Empresa = result.Model.Empresa };
        return Success(authorDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] SaveClienteDto saveClienteDto)
    {
        var cliente = new Cliente
        {
            Nome = saveClienteDto.Name!,
            Telefone = saveClienteDto.Telefone!,
            Empresa = saveClienteDto.Empresa!
        };
        var result = await clienteService.AddAsync(cliente);

        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = new ClienteDto { Id = result.Model!.Id, Nome = result.Model.Nome, Telefone = result.Model.Telefone, Empresa = result.Model.Empresa };
        return Created("GetClienteById", new { id = clienteDto.Id }, clienteDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] SaveClienteDto saveClienteDto)
    {
        var cliente = new Cliente
        {
            Nome = saveClienteDto.Name!,
            Telefone = saveClienteDto.Telefone!,
            Empresa = saveClienteDto.Empresa!
        };
        var result = await clienteService.UpdateAsync(id, cliente);

        if (!result.Success)
            return HandleErrorResponse(result);

        var clienteDto = new ClienteDto { Id = result.Model!.Id, Nome = result.Model.Nome, Telefone = result.Model.Telefone, Empresa = result.Model.Empresa };
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