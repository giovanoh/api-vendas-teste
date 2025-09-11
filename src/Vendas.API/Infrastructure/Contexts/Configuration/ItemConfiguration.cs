using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Vendas.API.Domain.Models;

namespace Vendas.API.Infrastructure.Contexts.Configuration;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("itens");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.Id)
            .IsUnique();
        builder.Property(i => i.Quantidade).IsRequired();
        builder.Property(i => i.Unitario).IsRequired();
        builder.Property(i => i.ProdutoId)/*.HasColumnName("produto_id")*/.IsRequired();
        builder.Property(i => i.VendaId)/*.HasColumnName("venda_id")*/.IsRequired();

        builder.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId);
        builder.HasOne<Venda>().WithMany(v => v.Itens).HasForeignKey(i => i.VendaId);
    }
}