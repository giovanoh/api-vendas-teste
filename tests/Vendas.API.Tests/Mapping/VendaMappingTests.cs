using FluentAssertions;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Tests.Mapping;

public class VendaMappingTests : MappingTestsBase
{
    [Fact]
    public void Should_Map_Venda_To_VendaDto()
    {
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Telefone = "11987654321",
            Empresa = "Silva & Associados Ltda"
        };
        var produto1 = new Produto
        {
            Id = 1,
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "imagem.png"
        };
        var produto2 = new Produto
        {
            Id = 2,
            Nome = "Produto 2",
            Valor = 200,
            Imagem = "imagem.png"
        };
        var item1 = new Item
        {
            Id = 1,
            Quantidade = 1,
            Unitario = 100,
            Produto = produto1,
        };
        var item2 = new Item
        {
            Id = 2,
            Quantidade = 2,
            Unitario = 200,
            Produto = produto2,
        };
        var venda = new Venda
        {
            Id = 1,
            Data = DateTime.Now,
            ValorTotal = 100,
            Cliente = cliente,
            Itens = [item1, item2]
        };

        var result = Mapper.Map<VendaDto>(venda);

        result.Should().NotBeNull();
        result.Id.Should().Be(venda.Id);
        result.Data.Should().Be(venda.Data);
        result.ValorTotal.Should().Be(venda.ValorTotal);
        result.NomeCliente.Should().Be(venda.Cliente.Nome);
        result.Itens.Count.Should().Be(venda.Itens.Count);
        result.Itens[0].Quantidade.Should().Be(venda.Itens[0].Quantidade);
        result.Itens[0].Unitario.Should().Be(venda.Itens[0].Unitario);
        result.Itens[0].NomeProduto.Should().Be(venda.Itens[0].Produto.Nome);
        result.Itens[1].Quantidade.Should().Be(venda.Itens[1].Quantidade);
        result.Itens[1].Unitario.Should().Be(venda.Itens[1].Unitario);
        result.Itens[1].NomeProduto.Should().Be(venda.Itens[1].Produto.Nome);
    }

    [Fact]
    public void Should_Map_SaveVendaDto_To_Venda()
    {
        var saveVendaDto = new SaveVendaDto
        {
            Data = DateTime.Now,
            ValorTotal = 100,
            ClienteId = 1,
            Itens = [
                new SaveItemDto { Quantidade = 1, Unitario = 100, ProdutoId = 1 },
                new SaveItemDto { Quantidade = 2, Unitario = 200, ProdutoId = 2 }
            ]
        };

        var result = Mapper.Map<Venda>(saveVendaDto);

        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.Data.Should().Be(saveVendaDto.Data);
        result.ValorTotal.Should().Be(saveVendaDto.ValorTotal);
        result.ClienteId.Should().Be(saveVendaDto.ClienteId);
        result.Cliente.Should().BeNull();
        result.Itens.Count.Should().Be(saveVendaDto.Itens.Count);
        result.Itens[0].Quantidade.Should().Be(saveVendaDto.Itens[0].Quantidade);
        result.Itens[0].Unitario.Should().Be(saveVendaDto.Itens[0].Unitario);
        result.Itens[0].ProdutoId.Should().Be(saveVendaDto.Itens[0].ProdutoId);
        result.Itens[0].Produto.Should().BeNull();
        result.Itens[1].Quantidade.Should().Be(saveVendaDto.Itens[1].Quantidade);
        result.Itens[1].Unitario.Should().Be(saveVendaDto.Itens[1].Unitario);
        result.Itens[1].ProdutoId.Should().Be(saveVendaDto.Itens[1].ProdutoId);
        result.Itens[1].Produto.Should().BeNull();
    }
}