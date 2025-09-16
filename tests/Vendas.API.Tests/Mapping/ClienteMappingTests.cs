using FluentAssertions;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Tests.Mapping;

public class ClienteMappingTests : MappingTestsBase
{
    [Fact]
    public void Should_Map_Cliente_To_ClienteDto()
    {
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Telefone = "11987654321",
            Empresa = "Silva & Associados Ltda"
        };

        var result = Mapper.Map<ClienteDto>(cliente);

        result.Should().NotBeNull();
        result.Id.Should().Be(cliente.Id);
        result.Nome.Should().Be(cliente.Nome);
        result.Telefone.Should().Be(cliente.Telefone);
        result.Empresa.Should().Be(cliente.Empresa);
    }

    [Fact]
    public void Should_Map_SaveClienteDto_To_Cliente()
    {
        var saveClienteDto = new SaveClienteDto
        {
            Nome = "Maria Santos",
            Telefone = "11999887766",
            Empresa = "Santos Consultoria"
        };

        var result = Mapper.Map<Cliente>(saveClienteDto);

        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.Nome.Should().Be(saveClienteDto.Nome);
        result.Telefone.Should().Be(saveClienteDto.Telefone);
        result.Empresa.Should().Be(saveClienteDto.Empresa);
    }
}