using FluentAssertions;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Tests.Mapping;

public class ProdutoMappingTests : MappingTestsBase
{
    [Fact]
    public void Should_Map_Produto_To_ProdutoDto()
    {
        var produto = new Produto
        {
            Id = 1,
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "imagem.png"
        };

        var result = Mapper.Map<ProdutoDto>(produto);

        result.Should().NotBeNull();
        result.Id.Should().Be(produto.Id);
        result.Nome.Should().Be(produto.Nome);
        result.Valor.Should().Be(produto.Valor);
    }

    [Fact]
    public void Should_Map_SaveProdutoDto_To_Produto()
    {
        var saveProdutoDto = new SaveProdutoDto
        {
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD/"
        };

        var result = Mapper.Map<Produto>(saveProdutoDto);

        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.Nome.Should().Be(saveProdutoDto.Nome);
        result.Valor.Should().Be(saveProdutoDto.Valor);
        result.Imagem.Should().BeNull();
    }
}