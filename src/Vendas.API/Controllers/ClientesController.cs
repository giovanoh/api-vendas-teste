using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class ClientesController(IClienteService service, IMapper mapper)
    : CrudController<IClienteService, Cliente, SaveClienteDto, ClienteDto>(service, mapper)
{
    protected override HashSet<string> SortableFields
        => [.. base.SortableFields, "nome"];
}