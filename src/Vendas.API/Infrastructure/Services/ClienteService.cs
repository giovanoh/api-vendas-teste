using Microsoft.EntityFrameworkCore;

using Vendas.API.Domain.Models;
using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;
using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IClienteRepository clienteRepository, IUnitOfWork unitOfWork, ILogger<ClienteService> logger)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<IEnumerable<Cliente>>> ListAsync()
    {
        try
        {
            return Response<IEnumerable<Cliente>>.Ok(await _clienteRepository.ListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao listar os clientes.");
            return Response<IEnumerable<Cliente>>.Fail(
                "Ocorreu um erro ao listar os clientes",
                ErrorType.DatabaseError);
        }
    }

    public async Task<Response<Cliente>> FindByIdAsync(int clienteId)
    {
        try
        {
            var cliente = await _clienteRepository.FindByIdAsync(clienteId);
            if (cliente == null)
                return Response<Cliente>.NotFound($"Cliente com id {clienteId} não encontrado");

            return Response<Cliente>.Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao buscar o cliente {ClienteId}.", clienteId);
            return Response<Cliente>.Fail(
                "Ocorreu um erro ao buscar o cliente",
                ErrorType.DatabaseError);
        }
    }

    public async Task<Response<Cliente>> AddAsync(Cliente cliente)
    {
        try
        {
            await _clienteRepository.AddAsync(cliente);
            await _unitOfWork.CompleteAsync();

            return Response<Cliente>.Ok(cliente);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao incluir o cliente no banco de dados.");
            return Response<Cliente>.Fail(
                "Ocorreu um erro ao incluir o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro inesperado ao incluir o cliente.");
            return Response<Cliente>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    public async Task<Response<Cliente>> UpdateAsync(int clienteId, Cliente cliente)
    {
        try
        {
            var clienteExistente = await _clienteRepository.FindByIdAsync(clienteId);
            if (clienteExistente == null)
                return Response<Cliente>.NotFound($"Cliente com id {clienteId} não encontrado");

            AtualizaPropriedades(clienteExistente, cliente);

            _clienteRepository.Update(clienteExistente);
            await _unitOfWork.CompleteAsync();

            return Response<Cliente>.Ok(clienteExistente);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao atualizar o cliente {ClienteId}.", clienteId);
            return Response<Cliente>.Fail(
                "Ocorreu um erro ao atualizar o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro inesperado ao atualizar o cliente {ClienteId}.", clienteId);
            return Response<Cliente>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    public async Task<Response<Cliente>> DeleteAsync(int clienteId)
    {
        try
        {
            var clienteExistente = await _clienteRepository.FindByIdAsync(clienteId);
            if (clienteExistente == null)
                return Response<Cliente>.NotFound($"Cliente com id {clienteId} não encontrado");

            _clienteRepository.Delete(clienteExistente);
            await _unitOfWork.CompleteAsync();

            return Response<Cliente>.Ok(clienteExistente);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ocorreu um erro ao excluir o cliente {ClienteId}.", clienteId);
            return Response<Cliente>.Fail(
                "Ocorreu um erro ao excluir o cliente",
                ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro inesperado ao excluir o cliente {ClienteId}.", clienteId);
            return Response<Cliente>.Fail(
                "Ocorreu um erro inesperado ao processar a solicitação",
                ErrorType.Unknown);
        }
    }

    private static void AtualizaPropriedades(Cliente clienteExistente, Cliente novoCliente)
    {
        clienteExistente.Nome = novoCliente.Nome;
        clienteExistente.Telefone = novoCliente.Telefone;
        clienteExistente.Empresa = novoCliente.Empresa;
    }

}