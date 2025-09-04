using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Vendas.API.Domain.Models;

namespace Vendas.API.Infrastructure.Contexts.Configuration;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired();
        builder.Property(c => c.Telefone).IsRequired();
        builder.Property(c => c.Empresa).IsRequired();
    }
}