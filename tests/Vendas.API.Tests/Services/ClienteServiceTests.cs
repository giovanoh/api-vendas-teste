using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;
using Vendas.API.Infrastructure.Services;
using Vendas.API.Tests.Helpers;

namespace Vendas.API.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<ClienteService>> _logger;
    private readonly Mock<ICacheService> _cacheService;

    public ClienteServiceTests()
    {
        _clienteRepository = new Mock<IClienteRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<ClienteService>>();
        _cacheService = new Mock<ICacheService>();
    }

    private ClienteService CreateService()
        => new ClienteService(_clienteRepository.Object, _unitOfWork.Object, _logger.Object, _cacheService.Object);

    [Fact]
    public async Task ListPagedAsync_ShouldReturnClients_WhenSuccessful()
    {
        ClienteService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo =>
            repo.ListPagedAsync(request))
            .ReturnsAsync((TestDataHelper.Clientes, TestDataHelper.Clientes.Count));

        Response<PagedResult<Cliente>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Data.Should().HaveCount(TestDataHelper.Clientes.Count);
        result.Model.Data.Should().BeEquivalentTo(TestDataHelper.Clientes);
        result.Model.Page.Should().Be(1);
        result.Model.PageSize.Should().Be(10);
        result.Model.TotalCount.Should().Be(TestDataHelper.Clientes.Count);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Cliente>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ClienteService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.ListPagedAsync(request)).ThrowsAsync(new Exception("test_exception"));

        Response<PagedResult<Cliente>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Cliente>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ClienteService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Cliente>>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _clienteRepository.Setup(repo => repo.ListPagedAsync(request)).ReturnsAsync((TestDataHelper.Clientes, TestDataHelper.Clientes.Count));

        Response<PagedResult<Cliente>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Cliente>>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _clienteRepository.Verify(repo => repo.ListPagedAsync(request), Times.Never);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnClient_WhenSuccessful()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);

        Response<Cliente> result = await service.FindByIdAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(cliente);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenClientNotFound()
    {
        ClienteService service = CreateService();
        var id = 999;
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(id)).ReturnsAsync(null as Cliente);

        Response<Cliente> result = await service.FindByIdAsync(id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be("Recurso com id 999 não encontrado");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ThrowsAsync(new Exception("test_exception"));

        Response<Cliente> result = await service.FindByIdAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);

        Response<Cliente> result = await service.FindByIdAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnClient_WhenSuccessful()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.AddAsync(cliente)).Returns(Task.CompletedTask);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);
        Response<Cliente> result = await service.AddAsync(cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(cliente);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.AddAsync(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.AddAsync(cliente)).ThrowsAsync(new DbUpdateException("test_exception"));

        Response<Cliente> result = await service.AddAsync(cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao incluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.AddAsync(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _clienteRepository.Setup(repo => repo.AddAsync(cliente)).Returns(Task.CompletedTask);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.AddAsync(cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _clienteRepository.Verify(repo => repo.AddAsync(cliente), Times.Once);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnClient_WhenSuccessful()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Update(cliente));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.UpdateAsync(cliente.Id, cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(cliente);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Update(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Update(cliente)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.UpdateAsync(cliente.Id, cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao atualizar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Update(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Update(cliente));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.UpdateAsync(cliente.Id, cliente);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Update(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnClient_WhenSuccessful()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Delete(cliente));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.DeleteAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(cliente);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Delete(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Delete(cliente)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.DeleteAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao excluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Cliente>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Delete(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ClienteService service = CreateService();
        Cliente cliente = TestDataHelper.Clientes.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _clienteRepository.Setup(repo => repo.FindByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _clienteRepository.Setup(repo => repo.Delete(cliente));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Cliente> result = await service.DeleteAsync(cliente.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _clienteRepository.Verify(repo => repo.FindByIdAsync(cliente.Id), Times.Once);
        _clienteRepository.Verify(repo => repo.Delete(cliente), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }
}
