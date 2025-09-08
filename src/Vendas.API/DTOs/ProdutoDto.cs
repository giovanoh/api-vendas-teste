namespace Vendas.API.DTOs;

public record ProdutoDto : IdentificableDto
{
    public string Nome { get; init; } = null!;
    public decimal Valor { get; init; }
}