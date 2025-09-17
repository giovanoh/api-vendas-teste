using Vendas.API.Domain.Models;

namespace Vendas.API.Tests.Helpers;

public static class TestDataHelper
{
    public static List<Cliente> Clientes =>
    [
        new Cliente
        {
            Id = 1,
            Nome = "Cliente 1",
            Telefone = "11987654321",
            Empresa = "Empresa 1"
        },
        new Cliente
        {
            Id = 2,
            Nome = "Cliente 2",
            Telefone = "11987654322",
            Empresa = "Empresa 2"
        }
    ];

    public static List<Produto> Produtos =>
    [
        new Produto
        {
            Id = 1,
            Nome = "Produto 1",
            Valor = 100,
            Imagem = "Imagem1.png"
        },
        new Produto
        {
            Id = 2,
            Nome = "Produto 2",
            Valor = 200,
            Imagem = "Imagem2.png"
        }
    ];

    public static List<Venda> Vendas =>
    [
        new Venda
        {
            Id = 1,
            Data = new DateTime(2025, 9, 1),
            ValorTotal = 100,
            ClienteId = 1,
            Cliente = Clientes[0],

            Itens = [
                new Item
                {
                    Id = 1,
                    ProdutoId = 1,
                    Produto = Produtos[0],
                    Quantidade = 1,
                    VendaId = 1
                }
            ]
        }
    ];
}
