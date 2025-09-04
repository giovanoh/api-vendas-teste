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
        builder.Property(v => v.Data).IsRequired();
        builder.Property(v => v.ValorTotal).IsRequired();
        builder.Property(v => v.ClienteId).IsRequired();
        builder.HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId);
        builder.HasMany(v => v.Items).WithOne().HasForeignKey(i => i.VendaId);
    }
}