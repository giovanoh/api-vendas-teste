using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Vendas.API.DTOs;
using Vendas.API.DTOs.Response;
using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Api;

public class ProdutosControllerTests(VendasApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnProducts_WhenSuccessful()
    {
        TestDataHelper.SeedProdutos(Factory, true);
        var endpoint = "/api/produtos";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ProdutoDto>>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenSuccessful()
    {
        TestDataHelper.SeedProdutos(Factory, true);
        var endpoint = "/api/produtos/1";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Nome.Should().Be("Produto 1");
        apiResponse.Data.Valor.Should().Be(100);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenProductNotFound()
    {
        var endpoint = "/api/produtos/999";

        var response = await Client.GetAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenSuccessful()
    {
        var endpoint = "/api/produtos";
        var newProduct = new SaveProdutoDto
        {
            Nome = "Produto 3",
            Valor = 300,
            Imagem = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD/"
        };

        var response = await Client.PostAsJsonAsync(endpoint, newProduct, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);
        var location = response.Headers.Location;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Nome.Should().Be("Produto 3");
        apiResponse.Data.Valor.Should().Be(300);
        location.Should().NotBeNull();
        location.ToString().Should().EndWith($"{endpoint}/{apiResponse.Data.Id}");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        var endpoint = "/api/produtos";
        var newProduct = new SaveProdutoDto
        {
            Nome = "Produto 3",
            Valor = 300,
            Imagem = "invalid"
        };

        var response = await Client.PostAsJsonAsync(endpoint, newProduct, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("Imagem");
        apiResponse.Errors["Imagem"].Should().Contain("O campo Imagem não é um base64 válido.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedProduct_WhenSuccessful()
    {
        TestDataHelper.SeedProdutos(Factory, true);
        var endpoint = "/api/produtos/1";
        var updatedProduct = new SaveProdutoDto
        {
            Nome = "Produto 1 Atualizado",
            Valor = 100,
            Imagem = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD/"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedProduct, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(1);
        apiResponse.Data.Nome.Should().Be("Produto 1 Atualizado");
        apiResponse.Data.Valor.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenProductNotFound()
    {
        var endpoint = "/api/produtos/999";
        var updatedProduct = new SaveProdutoDto
        {
            Nome = "Produto 1 Atualizado",
            Valor = 100,
            Imagem = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD/"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedProduct, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WithInvalidData()
    {
        TestDataHelper.SeedProdutos(Factory, true);
        var endpoint = "/api/produtos/1";
        var updatedProduct = new SaveProdutoDto
        {
            Nome = "Produto 1 Atualizado",
            Valor = 100,
            Imagem = "invalid"
        };

        var response = await Client.PutAsJsonAsync(endpoint, updatedProduct, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Should().NotBeNull();
        apiResponse.Title.Should().Be("Validation Error");
        apiResponse.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiResponse.Detail.Should().Be("One or more validation errors occurred.");
        apiResponse.Instance.Should().Be(endpoint);
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().HaveCount(1);
        apiResponse.Errors.Should().ContainKey("Imagem");
        apiResponse.Errors["Imagem"].Should().Contain("O campo Imagem não é um base64 válido.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenSuccessful()
    {
        TestDataHelper.SeedProdutos(Factory, true);
        var endpoint = "/api/produtos/1";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenProductNotFound()
    {
        var endpoint = "/api/produtos/999";

        var response = await Client.DeleteAsync(endpoint, CancellationToken);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>(CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().BeNull();
    }
}