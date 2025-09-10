using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Vendas.API.Domain.Models;

namespace Vendas.API.Infrastructure.Contexts.Configuration;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired();
        builder.Property(p => p.Imagem).IsRequired();
        builder.Property(p => p.Valor).IsRequired();
    }
}