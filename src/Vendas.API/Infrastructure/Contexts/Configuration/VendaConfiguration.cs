using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Vendas.API.Domain.Models;

namespace Vendas.API.Infrastructure.Contexts.Configuration;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("vendas");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Id)
            .IsUnique();
        builder.Property(v => v.Data).IsRequired();
        builder.Property(v => v.ValorTotal).HasColumnName("valor_total").IsRequired();
        builder.Property(v => v.ClienteId)/*.HasColumnName("cliente_id")*/.IsRequired();
        builder.HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId);
        builder.HasMany(v => v.Itens).WithOne().HasForeignKey(i => i.VendaId);
    }
}