using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.Infrastructure.Repositories;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Repositories;

public class ProdutoRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddAndListAsync_ShouldReturnProducts_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProdutoRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var produto = new Produto
        {
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "imagem.png"
        };

        await repository.AddAsync(produto);
        await unitOfWork.CompleteAsync();

        var (result, count) = await repository.ListPagedAsync(new PagedRequest { Page = 1, PageSize = 10 });
        result.Should().HaveCount(1);
        count.Should().Be(1);
        var retrievedProduto = result.First();
        retrievedProduto.Should().BeEquivalentTo(produto);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnProduct_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedProdutos(context);
        var repository = new ProdutoRepository(context);

        var produto = await repository.FindByIdAsync(1);

        produto.Should().NotBeNull();
        produto.Id.Should().Be(1);
        produto.Nome.Should().Be("Produto 1");
        produto.Valor.Should().Be(100);
        produto.Imagem.Should().Be("Imagem1.png");
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenProductNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProdutoRepository(context);

        var produto = await repository.FindByIdAsync(1);

        produto.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedProdutos(context);
        var repository = new ProdutoRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var produto = await repository.FindByIdAsync(1);
        produto.Should().NotBeNull();
        produto.Nome = "Produto Atualizado";
        produto.Valor = 200;
        produto.Imagem = "ImagemAtualizada.png";

        repository.Update(produto);
        await unitOfWork.CompleteAsync();

        produto = await repository.FindByIdAsync(1);
        produto.Should().NotBeNull();
        produto.Id.Should().Be(1);
        produto.Nome.Should().Be("Produto Atualizado");
        produto.Valor.Should().Be(200);
        produto.Imagem.Should().Be("ImagemAtualizada.png");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenProductNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProdutoRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var produto = new Produto
        {
            Id = 999,
            Nome = "Produto Atualizado",
            Valor = 200,
            Imagem = "ImagemAtualizada.png"
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Update(produto);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedProdutos(context);
        var repository = new ProdutoRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var produto = await repository.FindByIdAsync(1);
        repository.Delete(produto!);
        await unitOfWork.CompleteAsync();

        var retrievedProduto = await repository.FindByIdAsync(1);
        retrievedProduto.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenProductNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProdutoRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var produto = new Produto
        {
            Id = 999,
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "Imagem1.png"
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Delete(produto);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }
}
