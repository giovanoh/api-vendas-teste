namespace Vendas.API.Domain.Models;

public class Produto : Entity
{
    public string Nome { get; set; } = null!;
    public string Imagem { get; set; } = null!;
    public decimal Valor { get; set; }
}
