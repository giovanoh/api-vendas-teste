namespace Vendas.API.Domain.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string ImagemPath { get; set; } = null!;
    public decimal Valor { get; set; }
}
