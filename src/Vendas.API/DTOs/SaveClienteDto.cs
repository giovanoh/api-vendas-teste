using System.ComponentModel.DataAnnotations;

namespace Vendas.API.DTOs;

public record SaveClienteDto
{
    [Required]
    [MaxLength(100)]
    public string? Nome { get; init; }

    [Required]
    [MaxLength(45)]
    public string? Telefone { get; init; }

    [Required]
    [MaxLength(100)]
    public string? Empresa { get; init; }
}
