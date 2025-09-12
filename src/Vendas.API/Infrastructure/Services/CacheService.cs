using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Vendas.API.Domain.Services;

namespace Vendas.API.Infrastructure.Services;

public class CacheService(IMemoryCache cache, ILogger<CacheService> logger, IOptions<CacheSettings> cacheSettings) : ICacheService
{
    private readonly CacheSettings _settings = cacheSettings.Value;

    public bool IsEnabled => _settings.EnableCache;
    private MemoryCacheEntryOptions _defaultOptions => new()
    {
        SlidingExpiration = _settings.DefaultSlidingExpiration,
        AbsoluteExpirationRelativeToNow = _settings.DefaultAbsoluteExpiration
    };

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
    {
        if (cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
        {
            logger.LogDebug("Cache hit para chave: {Key}", key);
            return cachedValue;
        }
        logger.LogDebug("Cache miss para chave: {Key}. Buscando dados...", key);

        // Se não está no cache, executa a factory
        var value = await factory();
        try
        {
            var options = expirationTime.HasValue
                ? new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expirationTime.Value,
                    AbsoluteExpirationRelativeToNow = expirationTime.Value * 2
                }
                : _defaultOptions;

            cache.Set(key, value, options);
            logger.LogDebug("Dados armazenados em cache para chave: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao salvar dados no cache para chave: {Key}", key);
        }

        return value;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (cache.TryGetValue(key, out T? cachedValue))
            {
                logger.LogDebug("Cache hit para chave: {Key}", key);
                return Task.FromResult(cachedValue);
            }

            logger.LogDebug("Cache miss para chave: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar dados do cache para chave: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        try
        {
            var options = expirationTime.HasValue
                ? new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expirationTime.Value,
                    AbsoluteExpirationRelativeToNow = expirationTime.Value * 2
                }
                : _defaultOptions;

            cache.Set(key, value, options);
            logger.LogDebug("Dados armazenados em cache para chave: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao armazenar dados em cache para chave: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            cache.Remove(key);
            logger.LogDebug("Cache removido para chave: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover dados do cache para chave: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // O IMemoryCache não suporta remoção por padrão, então removemos manualmente
            // Isso é uma limitação do cache em memória - em produção com Redis seria mais eficiente
            var keysToRemove = cache.GetType()
                .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(cache) as dynamic;

            if (keysToRemove != null)
            {
                var keys = new List<string>();
                foreach (var entry in keysToRemove)
                {
                    var key = entry.Key.ToString();
                    if (key.Contains(pattern))
                    {
                        keys.Add(key);
                    }
                }

                foreach (var key in keys)
                {
                    cache.Remove(key);
                }

                logger.LogDebug("Cache removido para padrão: {Pattern}. {Count} entradas removidas.", pattern, keys.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover dados do cache por padrão: {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }
}
