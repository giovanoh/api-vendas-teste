using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;

namespace Vendas.API.Infrastructure.Services;

public class ProdutoService(IProdutoRepository repository, IUnitOfWork unitOfWork, ILogger<CrudService<Produto, IProdutoRepository, IUnitOfWork>> logger)
    : CrudService<Produto, IProdutoRepository, IUnitOfWork>(repository, unitOfWork, logger), IProdutoService
{
    protected override void AtualizaPropriedades(Produto produtoExistente, Produto novoProduto)
    {
        produtoExistente.Nome = novoProduto.Nome;
        produtoExistente.Valor = novoProduto.Valor;
        produtoExistente.ImagemPath = novoProduto.ImagemPath;
    }
}