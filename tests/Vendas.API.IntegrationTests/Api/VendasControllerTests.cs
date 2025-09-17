using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Vendas.API.DTOs;
using Vendas.API.DTOs.Response;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Api;

public class VendasControllerTests(VendasApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnVendas_WhenSuccessful()
    {
        TestDataHelper.SeedVendasWithRelatedData(Factory, true);
        var endpoint = "/api/vendas";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<VendaDto>>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnVenda_WhenSuccessful()
    {
        TestDataHelper.SeedVendasWithRelatedData(Factory, true);
        var endpoint = "/api/vendas/1";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Data.Should().Be(new DateTime(2025, 9, 1));
        apiResponse.Data.ValorTotal.Should().Be(100);
        apiResponse.Data.NomeCliente.Should().Be("Cliente 1");
        apiResponse.Data.Itens.Should().HaveCount(1);
        apiResponse.Data.Itens.First().Quantidade.Should().Be(1);
        apiResponse.Data.Itens.First().NomeProduto.Should().Be("Produto 1");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenVendaNotFound()
    {
        var endpoint = "/api/vendas/999";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenSuccessful()
    {
        TestDataHelper.SeedClientes(Factory, true);
        TestDataHelper.SeedProdutos(Factory, false);

        var endpoint = "/api/vendas";
        var newVenda = new SaveVendaDto
        {
            Data = new DateTime(2025, 9, 2),
            ValorTotal = 200,
            ClienteId = 1,
            Itens = [
                new SaveItemDto
                {
                    Quantidade = 2,
                    Unitario = 100,
                    ProdutoId = 1
                }
            ]
        };

        var response = await Client.PostAsJsonAsync(endpoint, newVenda, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);
        var location = response.Headers.Location;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Data.Should().Be(new DateTime(2025, 9, 2));
        apiResponse.Data.ValorTotal.Should().Be(200);
        apiResponse.Data.NomeCliente.Should().Be("Cliente 1");
        apiResponse.Data.Itens.Should().HaveCount(1);
        apiResponse.Data.Itens.First().Quantidade.Should().Be(2);
        apiResponse.Data.Itens.First().Unitario.Should().Be(100);
        apiResponse.Data.Itens.First().NomeProduto.Should().Be("Produto 1");
        location.Should().NotBeNull();
        location.ToString().Should().EndWith($"{endpoint}/{apiResponse.Data.Id}");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        var endpoint = "/api/vendas";
        var newVenda = new SaveVendaDto
        {
            Data = new DateTime(2025, 9, 2),
            ValorTotal = 200,
            ClienteId = null, // Invalid: null ClienteId
            Itens = []
        };

        var response = await Client.PostAsJsonAsync(endpoint, newVenda, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("ClienteId");
        apiResponse.Errors["ClienteId"].Should().Contain("The ClienteId field is required.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedVenda_WhenSuccessful()
    {
        TestDataHelper.SeedVendasWithRelatedData(Factory, true);
        var endpoint = "/api/vendas/1";
        var updatedVenda = new SaveVendaDto
        {
            Data = new DateTime(2025, 9, 3),
            ValorTotal = 150,
            ClienteId = 2,
            Itens = [
                new SaveItemDto
                {
                    Quantidade = 1,
                    Unitario = 150,
                    ProdutoId = 2
                }
            ]
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedVenda, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Data.Should().Be(new DateTime(2025, 9, 3));
        apiResponse.Data.ValorTotal.Should().Be(150);
        apiResponse.Data.NomeCliente.Should().Be("Cliente 2");
        apiResponse.Data.Itens.Should().HaveCount(1);
        apiResponse.Data.Itens.First().NomeProduto.Should().Be("Produto 2");
        apiResponse.Data.Itens.First().Quantidade.Should().Be(1);
        apiResponse.Data.Itens.First().Unitario.Should().Be(150);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenVendaNotFound()
    {
        var endpoint = "/api/vendas/999";
        var updatedVenda = new SaveVendaDto
        {
            Data = new DateTime(2025, 9, 3),
            ValorTotal = 150,
            ClienteId = 1,
            Itens = []
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedVenda, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        TestDataHelper.SeedVendasWithRelatedData(Factory, true);
        var endpoint = "/api/vendas/1";
        var updatedVenda = new SaveVendaDto
        {
            Data = new DateTime(2025, 9, 3),
            ValorTotal = 150,
            ClienteId = null, // Invalid: null ClienteId
            Itens = []
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedVenda, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("ClienteId");
        apiResponse.Errors["ClienteId"].Should().Contain("The ClienteId field is required.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenSuccessful()
    {
        TestDataHelper.SeedVendasWithRelatedData(Factory, true);
        var endpoint = "/api/vendas/1";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenVendaNotFound()
    {
        var endpoint = "/api/vendas/999";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<VendaDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }
}
