namespace Vendas.API.Domain.Models;

public class Venda : Entity
{
    public DateTime Data { get; set; }
    public decimal ValorTotal { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public List<Item> Itens { get; set; } = [];
}