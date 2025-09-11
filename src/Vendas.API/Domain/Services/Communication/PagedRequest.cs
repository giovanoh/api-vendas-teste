using System.ComponentModel.DataAnnotations;

namespace Vendas.API.Domain.Services.Communication;

public class PagedRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page deve ser maior que 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize deve estar entre 1 e 100")]
    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "id";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder deve ser 'asc' ou 'desc'")]
    public string SortOrder { get; set; } = "asc";

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
