using System.ComponentModel.DataAnnotations;

namespace Vendas.API.DTOs;

public record SaveVendaDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Data { get; init; }

    [Required]
    public decimal ValorTotal { get; init; }

    [Required]
    public int? ClienteId { get; init; }

    [Required]
    public List<SaveItemDto> Items { get; init; } = [];
}
