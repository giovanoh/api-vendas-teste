namespace Vendas.API.Domain.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Empresa { get; set; } = null!;
}