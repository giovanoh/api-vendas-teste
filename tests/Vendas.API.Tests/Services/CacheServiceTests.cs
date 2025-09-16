using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Vendas.API.Domain.Services;
using Vendas.API.Infrastructure.Services;

namespace Vendas.API.Tests.Services;

public class CacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<CacheService>> _logger;
    private readonly IOptions<CacheSettings> _cacheSettings;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = new Mock<ILogger<CacheService>>();
        _cacheSettings = new OptionsWrapper<CacheSettings>(new CacheSettings
        {
            EnableCache = true,
            DefaultSlidingExpiration = TimeSpan.FromSeconds(10),
            DefaultAbsoluteExpiration = TimeSpan.FromSeconds(20)
        });
    }

    private CacheService CreateService()
        => new CacheService(_memoryCache, _logger.Object, _cacheSettings);

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedValue()
    {
        CacheService cacheService = CreateService();
        var key = "test_key";
        var value = "test_value";
        var expirationTime = TimeSpan.FromSeconds(10);
        _memoryCache.Set(key, value, expirationTime);

        var result = await cacheService.GetOrSetAsync(key, () => Task.FromResult("new_value"), expirationTime);

        result.Should().Be(value);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnFactoryValue()
    {
        CacheService cacheService = CreateService();
        var key = "test_key";
        var value = "test_value";
        var expirationTime = TimeSpan.FromSeconds(10);

        var result = await cacheService.GetOrSetAsync(key, () => Task.FromResult(value), expirationTime);
        var cachedValue = _memoryCache.Get<string>(key);

        result.Should().Be(value);
        cachedValue.Should().Be(value);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnFactoryValue_WhenExceptionIsThrown()
    {
        var failingCache = new FailingMemoryCache();
        var cacheService = new CacheService(failingCache, _logger.Object, _cacheSettings);
        var key = "test_key";
        var value = "test_value";
        var expirationTime = TimeSpan.FromSeconds(10);

        var result = await cacheService.GetOrSetAsync(key, () => Task.FromResult(value), expirationTime);

        result.Should().Be(value);
    }

    // Cache customizado que falha ao salvar dados
    private class FailingMemoryCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key) => throw new NotImplementedException();
        public void Dispose() { }
        public void Remove(object key) { }

        public bool TryGetValue(object key, out object? value)
        {
            value = null;
            return false; // Sempre retorna false (cache miss)
        }

        public void Set(object key, object? value, MemoryCacheEntryOptions? options)
        {
            throw new Exception("test_exception"); // Sempre falha ao salvar
        }
    }
}