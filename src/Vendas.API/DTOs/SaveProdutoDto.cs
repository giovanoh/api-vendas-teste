using System.ComponentModel.DataAnnotations;

using Vendas.API.Validation;

namespace Vendas.API.DTOs;

public record SaveProdutoDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; init; } = null!;

    [Required]
    public decimal Valor { get; init; }

    [Required]
    [Base64Validation]
    public string Imagem { get; init; } = null!;
}