namespace Vendas.API.DTOs;

public record VendaDto : IdentificableDto
{
    public DateTime Data { get; init; }
    public decimal ValorTotal { get; init; }
    public string NomeCliente { get; init; } = null!;
    public List<ItemDto> Items { get; init; } = [];
}
