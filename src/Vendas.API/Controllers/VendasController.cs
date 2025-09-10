using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class VendasController(IVendaService service, IMapper mapper)
    : CrudController<IVendaService, Venda, SaveVendaDto, VendaDto>(service, mapper)
{
}