namespace Vendas.API.DTOs;

public record ClienteDto : IdentificableDto
{
    public string Nome { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Empresa { get; init; } = null!;
}