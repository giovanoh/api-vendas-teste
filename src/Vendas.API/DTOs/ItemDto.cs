namespace Vendas.API.DTOs;

public record ItemDto
{
    public int Quantidade { get; init; }
    public decimal Unitario { get; init; }
    public string NomeProduto { get; init; } = null!;
}