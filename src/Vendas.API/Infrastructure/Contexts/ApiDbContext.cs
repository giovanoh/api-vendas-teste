using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Infrastructure.Extensions;

namespace Vendas.API.Infrastructure.Contexts;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Venda> Vendas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuração global para usar tudo em minúsculas
        builder.UseLowerCaseNamingConvention();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
