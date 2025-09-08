using System.ComponentModel.DataAnnotations;

namespace Vendas.API.DTOs;

public record SaveProdutoDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; init; } = null!;

    [Required]
    public decimal Valor { get; init; }

    [Required]
    public string Imagem { get; init; } = null!;
}