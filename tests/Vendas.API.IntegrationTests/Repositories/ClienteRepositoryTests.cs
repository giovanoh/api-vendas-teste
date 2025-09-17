using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.Infrastructure.Repositories;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Repositories;

public class ClienteRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddAndListAsync_ShouldReturnClients_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var cliente = new Cliente
        {
            Nome = "Cliente 1",
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        };

        await repository.AddAsync(cliente);
        await unitOfWork.CompleteAsync();

        var (result, count) = await repository.ListPagedAsync(new PagedRequest { Page = 1, PageSize = 10 });
        result.Should().HaveCount(1);
        count.Should().Be(1);
        var retrievedCliente = result.First();
        retrievedCliente.Should().BeEquivalentTo(cliente);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnClient_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedClientes(context);
        var repository = new ClienteRepository(context);

        var cliente = await repository.FindByIdAsync(1);

        cliente.Should().NotBeNull();
        cliente.Id.Should().Be(1);
        cliente.Nome.Should().Be("Cliente 1");
        cliente.Telefone.Should().Be("11987654321");
        cliente.Empresa.Should().Be("Empresa 1");
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenClientNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ClienteRepository(context);

        var cliente = await repository.FindByIdAsync(1);

        cliente.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateClient_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedClientes(context);
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var cliente = await repository.FindByIdAsync(1);
        cliente.Should().NotBeNull();
        cliente.Nome = "Cliente Atualizado";
        cliente.Telefone = "11987654399";
        cliente.Empresa = "Empresa Atualizada";

        repository.Update(cliente);
        await unitOfWork.CompleteAsync();

        cliente = await repository.FindByIdAsync(1);
        cliente.Should().NotBeNull();
        cliente.Id.Should().Be(1);
        cliente.Nome.Should().Be("Cliente Atualizado");
        cliente.Telefone.Should().Be("11987654399");
        cliente.Empresa.Should().Be("Empresa Atualizada");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenClientNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var cliente = new Cliente
        {
            Id = 999,
            Nome = "Cliente Atualizado",
            Telefone = "11987654399",
            Empresa = "Empresa Atualizada"
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Update(cliente);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteClient_WhenSuccessful()
    {
        using var context = CreateInMemoryContext();
        TestDataHelper.SeedClientes(context);
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var cliente = await repository.FindByIdAsync(1);
        repository.Delete(cliente!);
        await unitOfWork.CompleteAsync();

        var retrievedCliente = await repository.FindByIdAsync(1);
        retrievedCliente.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenClientNotFound()
    {
        using var context = CreateInMemoryContext();
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var cliente = new Cliente
        {
            Id = 999,
            Nome = "Cliente 1",
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        };

        await FluentActions
            .Invoking(async () =>
            {
                repository.Delete(cliente);
                await unitOfWork.CompleteAsync();
            })
            .Should()
            .ThrowAsync<DbUpdateException>();
    }
}
