using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Vendas.API.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseLowerCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Define o nome das colunas em minúsculas
            foreach (var property in entity.GetProperties())
            {
                if (IsForeignKey(property))
                {
                    var columnName = property.Name.ToLower();
                    if (columnName.EndsWith("id"))
                    {
                        if (!columnName.Contains("_id"))
                        {
                            columnName = columnName.Replace("id", "_id");
                        }
                    }
                    property.SetColumnName(columnName);
                }
                else
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }
        }
    }

    private static bool IsForeignKey(IMutableProperty property)
    {
        // Verifica se a propriedade é uma foreign key usando a API do EF Core
        return property.IsForeignKey() ||
               (property.Name.EndsWith("Id") &&
                !property.Name.StartsWith("Id") &&
                !property.IsPrimaryKey());
    }
}
