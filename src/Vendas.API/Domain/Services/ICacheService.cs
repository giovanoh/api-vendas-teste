namespace Vendas.API.Domain.Services;

public interface ICacheService
{
    bool IsEnabled { get; }
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
