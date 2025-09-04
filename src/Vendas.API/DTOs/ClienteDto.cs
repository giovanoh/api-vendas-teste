namespace Vendas.API.DTOs;

public record ClienteDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Empresa { get; init; } = null!;
}