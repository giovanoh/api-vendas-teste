using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.Infrastructure.Repositories;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Repositories;

public class VendaRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddAndListAsync_ShouldReturnSales_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedClientes(context);
        TestDataHelper.SeedProdutos(context);
        var repository = new VendaRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var venda = new Venda
        {
            Data = new DateTime(2025, 1, 15),
            ValorTotal = 150,
            ClienteId = 1,
            Itens = new List<Item>
            {
                new Item
                {
                    ProdutoId = 1,
                    Quantidade = 1,
                    Unitario = 100,
                    VendaId = 0 // Será definido automaticamente
                },
                new Item
                {
                    ProdutoId = 2,
                    Quantidade = 1,
                    Unitario = 50,
                    VendaId = 0 // Será definido automaticamente
                }
            }
        };

        await repository.AddAsync(venda);
        await unitOfWork.CompleteAsync();

        var (result, count) = await repository.ListPagedAsync(new PagedRequest { Page = 1, PageSize = 10 });
        result.Should().HaveCount(1);
        count.Should().Be(1);
        var retrievedVenda = result.First();
        retrievedVenda.Data.Should().Be(venda.Data);
        retrievedVenda.ValorTotal.Should().Be(venda.ValorTotal);
        retrievedVenda.ClienteId.Should().Be(venda.ClienteId);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnSale_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedVendasWithRelatedData(context);
        var repository = new VendaRepository(context);

        var venda = await repository.FindByIdAsync(1);

        venda.Should().NotBeNull();
        venda.Id.Should().Be(1);
        venda.Data.Should().Be(new DateTime(2025, 9, 1));
        venda.ValorTotal.Should().Be(100);
        venda.ClienteId.Should().Be(1);
        venda.Itens.Should().HaveCount(1);
        venda.Itens.First().Quantidade.Should().Be(1);
        venda.Itens.First().ProdutoId.Should().Be(1);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenSaleNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new VendaRepository(context);

        var venda = await repository.FindByIdAsync(1);

        venda.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSale_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedVendasWithRelatedData(context);
        var repository = new VendaRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var venda = await repository.FindByIdAsync(1);
        venda.Should().NotBeNull();
        venda.Data = new DateTime(2025, 10, 15);
        venda.ValorTotal = 200;
        venda.ClienteId = 2;

        repository.Update(venda);
        await unitOfWork.CompleteAsync();

        venda = await repository.FindByIdAsync(1);
        venda.Should().NotBeNull();
        venda.Id.Should().Be(1);
        venda.Data.Should().Be(new DateTime(2025, 10, 15));
        venda.ValorTotal.Should().Be(200);
        venda.ClienteId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenSaleNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new VendaRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var venda = new Venda
        {
            Id = 999,
            Data = new DateTime(2025, 10, 15),
            ValorTotal = 200,
            ClienteId = 1
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Update(venda);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSale_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedVendasWithRelatedData(context);
        var repository = new VendaRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var venda = await repository.FindByIdAsync(1);
        repository.Delete(venda!);
        await unitOfWork.CompleteAsync();

        var retrievedVenda = await repository.FindByIdAsync(1);
        retrievedVenda.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenSaleNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new VendaRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var venda = new Venda
        {
            Id = 999,
            Data = new DateTime(2025, 9, 1),
            ValorTotal = 100,
            ClienteId = 1
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Delete(venda);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }
}
