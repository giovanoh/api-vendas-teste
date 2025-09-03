namespace Vendas.API.Domain.Models;

public class Item
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal Unitario { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int VendaId { get; set; }
}