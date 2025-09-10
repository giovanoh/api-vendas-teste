using System.ComponentModel.DataAnnotations;

namespace Vendas.API.DTOs;

public record SaveItemDto
{
    [Required]
    public int Quantidade { get; init; }

    [Required]
    public decimal Unitario { get; init; }

    [Required]
    public int? ProdutoId { get; init; }
}
