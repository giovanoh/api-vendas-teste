using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Vendas.API.DTOs;
using Vendas.API.DTOs.Response;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Api;

public class ClientesControllerTests(VendasApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnClients_WhenSuccessful()
    {
        TestDataHelper.SeedClientes(Factory, true);
        var endpoint = "/api/clientes";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ClienteDto>>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClient_WhenSuccessful()
    {
        TestDataHelper.SeedClientes(Factory, true);
        var endpoint = "/api/clientes/1";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Nome.Should().Be("Cliente 1");
        apiResponse.Data.Telefone.Should().Be("11987654321");
        apiResponse.Data.Empresa.Should().Be("Empresa 1");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenClientNotFound()
    {
        var endpoint = "/api/clientes/999";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenSuccessful()
    {
        var endpoint = "/api/clientes";
        var newClient = new SaveClienteDto
        {
            Nome = "Cliente 3",
            Telefone = "11987654323",
            Empresa = "Empresa 3"
        };

        var response = await Client.PostAsJsonAsync(endpoint, newClient, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);
        var location = response.Headers.Location;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Nome.Should().Be("Cliente 3");
        apiResponse.Data.Telefone.Should().Be("11987654323");
        apiResponse.Data.Empresa.Should().Be("Empresa 3");
        location.Should().NotBeNull();
        location.ToString().Should().EndWith($"{endpoint}/{apiResponse.Data.Id}");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        var endpoint = "/api/clientes";
        var newClient = new SaveClienteDto
        {
            Nome = "", // Invalid: empty name
            Telefone = "11987654323",
            Empresa = "Empresa 3"
        };

        var response = await Client.PostAsJsonAsync(endpoint, newClient, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("Nome");
        apiResponse.Errors["Nome"].Should().Contain("The Nome field is required.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedClient_WhenSuccessful()
    {
        TestDataHelper.SeedClientes(Factory, true);
        var endpoint = "/api/clientes/1";
        var updatedClient = new SaveClienteDto
        {
            Nome = "Cliente 1 Atualizado",
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedClient, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Nome.Should().Be("Cliente 1 Atualizado");
        apiResponse.Data.Telefone.Should().Be("11987654321");
        apiResponse.Data.Empresa.Should().Be("Empresa 1");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenClientNotFound()
    {
        var endpoint = "/api/clientes/999";
        var updatedClient = new SaveClienteDto
        {
            Nome = "Cliente 1 Atualizado",
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedClient, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        TestDataHelper.SeedClientes(Factory, true);
        var endpoint = "/api/clientes/1";
        var updatedClient = new SaveClienteDto
        {
            Nome = "", // Invalid: empty name
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedClient, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("Nome");
        apiResponse.Errors["Nome"].Should().Contain("The Nome field is required.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenSuccessful()
    {
        TestDataHelper.SeedClientes(Factory, true);
        var endpoint = "/api/clientes/1";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenClientNotFound()
    {
        var endpoint = "/api/clientes/999";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }
}
