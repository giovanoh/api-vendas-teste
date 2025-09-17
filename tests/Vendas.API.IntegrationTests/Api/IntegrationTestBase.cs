using Microsoft.AspNetCore.Mvc.Testing;

using Vendas.API.IntegrationTests.Fixtures;

namespace Vendas.API.IntegrationTests.Api;

public abstract class IntegrationTestBase : IClassFixture<VendasApiFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly VendasApiFactory Factory;
    private CancellationTokenSource? _cts;
    private const int TestTimeoutSeconds = 10;

    protected IntegrationTestBase(VendasApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

    public ValueTask InitializeAsync()
    {
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(TestTimeoutSeconds));
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        return ValueTask.CompletedTask;
    }
}