using Microsoft.Extensions.DependencyInjection;

using Vendas.API.Domain.Models;
using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.IntegrationTests.Fixtures;

public static class TestDataHelper
{
    public static Cliente[] GetDefaultClientes() =>
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

    public static void SeedClientes(ApiDbContext dbContext, bool resetDatabase = false)
    {
        var clientes = GetDefaultClientes();
        SeedClientesInternal(dbContext, clientes, resetDatabase);
    }

    public static void SeedClientes(VendasApiFactory factory, bool resetDatabase = false)
    {
        var clientes = GetDefaultClientes();

        WithDbContext(factory, dbContext =>
        {
            SeedClientesInternal(dbContext, clientes, resetDatabase);
        });
    }

    private static void SeedClientesInternal(ApiDbContext dbContext, Cliente[] clientes, bool resetDatabase = false)
    {
        if (resetDatabase)
        {
            dbContext.Database.EnsureDeleted();
        }
        dbContext.Database.EnsureCreated();

        dbContext.Clientes.AddRange(clientes);
        dbContext.SaveChanges();
    }

    private static Produto[] GetDefaultProdutos() =>
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

    public static void SeedProdutos(ApiDbContext dbContext, bool resetDatabase = false)
    {
        var produtos = GetDefaultProdutos();
        SeedProdutosInternal(dbContext, produtos, resetDatabase);
    }

    public static void SeedProdutos(VendasApiFactory factory, bool resetDatabase = false)
    {
        var produtos = GetDefaultProdutos();

        WithDbContext(factory, dbContext =>
        {
            SeedProdutosInternal(dbContext, produtos, resetDatabase);
        });
    }

    private static void SeedProdutosInternal(ApiDbContext dbContext, Produto[] produtos, bool resetDatabase = false)
    {
        if (resetDatabase)
        {
            dbContext.Database.EnsureDeleted();
        }
        dbContext.Database.EnsureCreated();

        dbContext.Produtos.AddRange(produtos);
        dbContext.SaveChanges();
    }

    private static Venda[] GetDefaultVendas() =>
    [
        new Venda
        {
            Id = 1,
            Data = new DateTime(2025, 9, 1),
            ValorTotal = 100,
            ClienteId = 1,

            Itens = [
                new Item
                {
                    Id = 1,
                    ProdutoId = 1,
                    Quantidade = 1,
                    VendaId = 1
                }
            ]
        }
    ];

    public static void SeedVendasWithRelatedData(ApiDbContext dbContext, bool resetDatabase = false)
    {
        if (resetDatabase)
        {
            dbContext.Database.EnsureDeleted();
        }
        dbContext.Database.EnsureCreated();

        // Seed related data first
        SeedClientesInternal(dbContext, GetDefaultClientes(), false);
        SeedProdutosInternal(dbContext, GetDefaultProdutos(), false);

        // Then seed vendas
        SeedVendasInternal(dbContext, GetDefaultVendas(), false);
    }

    public static void SeedVendasWithRelatedData(VendasApiFactory factory, bool resetDatabase = false)
    {
        WithDbContext(factory, dbContext =>
        {
            SeedVendasWithRelatedData(dbContext, resetDatabase);
        });
    }

    private static void SeedVendasInternal(ApiDbContext dbContext, Venda[] vendas, bool resetDatabase = false)
    {
        if (resetDatabase)
        {
            dbContext.Database.EnsureDeleted();
        }
        dbContext.Database.EnsureCreated();

        dbContext.Vendas.AddRange(vendas);
        dbContext.SaveChanges();
    }

    private static void WithDbContext(VendasApiFactory factory, Action<ApiDbContext> action)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IServiceProvider scopedServices = scope.ServiceProvider;
        ApiDbContext dbContext = scopedServices.GetRequiredService<ApiDbContext>();

        action(dbContext);
    }
}