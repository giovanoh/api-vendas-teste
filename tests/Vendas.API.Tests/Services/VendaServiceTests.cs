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

public class VendaServiceTests
{
    private readonly Mock<IVendaRepository> _vendaRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<VendaService>> _logger;
    private readonly Mock<ICacheService> _cacheService;

    public VendaServiceTests()
    {
        _vendaRepository = new Mock<IVendaRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<VendaService>>();
        _cacheService = new Mock<ICacheService>();
    }

    private VendaService CreateService()
        => new VendaService(_vendaRepository.Object, _unitOfWork.Object, _logger.Object, _cacheService.Object);

    [Fact]
    public async Task ListPagedAsync_ShouldReturnSales_WhenSuccessful()
    {
        VendaService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo =>
            repo.ListPagedAsync(request))
            .ReturnsAsync((TestDataHelper.Vendas, TestDataHelper.Vendas.Count));

        Response<PagedResult<Venda>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Data.Should().HaveCount(TestDataHelper.Vendas.Count);
        result.Model.Data.Should().BeEquivalentTo(TestDataHelper.Vendas);
        result.Model.Page.Should().Be(1);
        result.Model.PageSize.Should().Be(10);
        result.Model.TotalCount.Should().Be(TestDataHelper.Vendas.Count);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Venda>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenRepositoryFails()
    {
        VendaService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.ListPagedAsync(request)).ThrowsAsync(new Exception("test_exception"));

        Response<PagedResult<Venda>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Venda>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        VendaService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Venda>>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _vendaRepository.Setup(repo => repo.ListPagedAsync(request)).ReturnsAsync((TestDataHelper.Vendas, TestDataHelper.Vendas.Count));

        Response<PagedResult<Venda>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Venda>>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _vendaRepository.Verify(repo => repo.ListPagedAsync(request), Times.Never);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnSale_WhenSuccessful()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);

        Response<Venda> result = await service.FindByIdAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(venda);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenSaleNotFound()
    {
        VendaService service = CreateService();
        var id = 999;
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(id)).ReturnsAsync(null as Venda);

        Response<Venda> result = await service.FindByIdAsync(id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be("Recurso com id 999 não encontrado");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenRepositoryFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ThrowsAsync(new Exception("test_exception"));

        Response<Venda> result = await service.FindByIdAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);

        Response<Venda> result = await service.FindByIdAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnSale_WhenSuccessful()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.AddAsync(venda)).Returns(Task.CompletedTask);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);
        Response<Venda> result = await service.AddAsync(venda);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(venda);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.AddAsync(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenRepositoryFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.AddAsync(venda)).ThrowsAsync(new DbUpdateException("test_exception"));

        Response<Venda> result = await service.AddAsync(venda);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao incluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.AddAsync(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _vendaRepository.Setup(repo => repo.AddAsync(venda)).Returns(Task.CompletedTask);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.AddAsync(venda);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _vendaRepository.Verify(repo => repo.AddAsync(venda), Times.Once);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSale_WhenSuccessful()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Update(venda));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.UpdateAsync(venda.Id, venda);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(venda);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Update(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenRepositoryFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Update(venda)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.UpdateAsync(venda.Id, venda);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao atualizar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Update(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Update(venda));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.UpdateAsync(venda.Id, venda);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Update(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSale_WhenSuccessful()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Delete(venda));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.DeleteAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(venda);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Delete(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenRepositoryFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Delete(venda)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.DeleteAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao excluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Venda>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Delete(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        VendaService service = CreateService();
        Venda venda = TestDataHelper.Vendas.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _vendaRepository.Setup(repo => repo.FindByIdAsync(venda.Id)).ReturnsAsync(venda);
        _vendaRepository.Setup(repo => repo.Delete(venda));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Venda> result = await service.DeleteAsync(venda.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _vendaRepository.Verify(repo => repo.FindByIdAsync(venda.Id), Times.Once);
        _vendaRepository.Verify(repo => repo.Delete(venda), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }
}
