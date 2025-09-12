namespace Vendas.API.Domain.Services;

public class CacheSettings
{
    public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromHours(1);
    public bool EnableCache { get; set; } = true;
}
