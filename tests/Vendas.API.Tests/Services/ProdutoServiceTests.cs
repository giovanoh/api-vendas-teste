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

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _produtoRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<ProdutoService>> _logger;
    private readonly Mock<ICacheService> _cacheService;

    public ProdutoServiceTests()
    {
        _produtoRepository = new Mock<IProdutoRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<ProdutoService>>();
        _cacheService = new Mock<ICacheService>();
    }

    private ProdutoService CreateService()
        => new ProdutoService(_produtoRepository.Object, _unitOfWork.Object, _logger.Object, _cacheService.Object);

    [Fact]
    public async Task ListPagedAsync_ShouldReturnProducts_WhenSuccessful()
    {
        ProdutoService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo =>
            repo.ListPagedAsync(request))
            .ReturnsAsync((TestDataHelper.Produtos, TestDataHelper.Produtos.Count));

        Response<PagedResult<Produto>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Data.Should().HaveCount(TestDataHelper.Produtos.Count);
        result.Model.Data.Should().BeEquivalentTo(TestDataHelper.Produtos);
        result.Model.Page.Should().Be(1);
        result.Model.PageSize.Should().Be(10);
        result.Model.TotalCount.Should().Be(TestDataHelper.Produtos.Count);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Produto>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ProdutoService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.ListPagedAsync(request)).ThrowsAsync(new Exception("test_exception"));

        Response<PagedResult<Produto>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Produto>>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.ListPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task ListPagedAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ProdutoService service = CreateService();
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Produto>>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _produtoRepository.Setup(repo => repo.ListPagedAsync(request)).ReturnsAsync((TestDataHelper.Produtos, TestDataHelper.Produtos.Count));

        Response<PagedResult<Produto>> result = await service.ListPagedAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao listar os dados");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<PagedResult<Produto>>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _produtoRepository.Verify(repo => repo.ListPagedAsync(request), Times.Never);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnProduct_WhenSuccessful()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);

        Response<Produto> result = await service.FindByIdAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(produto);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenProductNotFound()
    {
        ProdutoService service = CreateService();
        var id = 999;
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(id)).ReturnsAsync(null as Produto);

        Response<Produto> result = await service.FindByIdAsync(id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be("Recurso com id 999 não encontrado");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ThrowsAsync(new Exception("test_exception"));

        Response<Produto> result = await service.FindByIdAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception("test_exception"));
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);

        Response<Produto> result = await service.FindByIdAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao buscar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnProduct_WhenSuccessful()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.AddAsync(produto)).Returns(Task.CompletedTask);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);
        Response<Produto> result = await service.AddAsync(produto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(produto);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.AddAsync(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.AddAsync(produto)).ThrowsAsync(new DbUpdateException("test_exception"));

        Response<Produto> result = await service.AddAsync(produto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao incluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.AddAsync(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _produtoRepository.Setup(repo => repo.AddAsync(produto)).Returns(Task.CompletedTask);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.AddAsync(produto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _produtoRepository.Verify(repo => repo.AddAsync(produto), Times.Once);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnProduct_WhenSuccessful()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Update(produto));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.UpdateAsync(produto.Id, produto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(produto);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Exactly(2));
        _produtoRepository.Verify(repo => repo.Update(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Update(produto)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.UpdateAsync(produto.Id, produto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao atualizar o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
        _produtoRepository.Verify(repo => repo.Update(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Update(produto));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.UpdateAsync(produto.Id, produto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Exactly(2));
        _produtoRepository.Verify(repo => repo.Update(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnProduct_WhenSuccessful()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Delete(produto));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.DeleteAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(produto);
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
        _produtoRepository.Verify(repo => repo.Delete(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenRepositoryFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(false);
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Delete(produto)).Throws(new DbUpdateException("test_exception"));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.DeleteAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.DatabaseError);
        result.Message.Should().Be("Ocorreu um erro ao excluir o recurso");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Never);
        _cacheService.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Response<Produto>>>>(), It.IsAny<TimeSpan?>()), Times.Never);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
        _produtoRepository.Verify(repo => repo.Delete(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenCacheServiceFails()
    {
        ProdutoService service = CreateService();
        Produto produto = TestDataHelper.Produtos.First();
        _cacheService.Setup(cache => cache.IsEnabled).Returns(true);
        _cacheService.Setup(cache => cache.RemoveByPatternAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test_exception"));
        _produtoRepository.Setup(repo => repo.FindByIdAsync(produto.Id)).ReturnsAsync(produto);
        _produtoRepository.Setup(repo => repo.Delete(produto));
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(Task.CompletedTask);

        Response<Produto> result = await service.DeleteAsync(produto.Id);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Model.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(ErrorType.Unknown);
        result.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação");

        _cacheService.Verify(cache => cache.IsEnabled, Times.Once);
        _cacheService.Verify(cache => cache.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
        _produtoRepository.Verify(repo => repo.FindByIdAsync(produto.Id), Times.Once);
        _produtoRepository.Verify(repo => repo.Delete(produto), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }
}