using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services;
using Vendas.API.DTOs;

namespace Vendas.API.Controllers;

public class ProdutosController(IProdutoService service, IMapper mapper)
    : CrudController<IProdutoService, Produto, SaveProdutoDto, ProdutoDto>(service, mapper)
{
}
