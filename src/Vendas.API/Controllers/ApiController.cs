using Microsoft.AspNetCore.Mvc;

using Vendas.API.Domain.Services.Communication;
using Vendas.API.DTOs.Response;
using Vendas.API.Infrastructure.Factories;

namespace Vendas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiController : ControllerBase
{
    protected IActionResult Success<T>(T data, int statusCode = 200, Dictionary<string, object>? meta = null)
    {
        var response = ApiResponse<T>.Ok(data, meta);
        return StatusCode(statusCode, response);
    }

    protected IActionResult Created<T>(string routeName, object routeValues, T data)
    {
        var response = ApiResponse<T>.Ok(data);
        return CreatedAtRoute(routeName, routeValues, response);
    }

    protected IActionResult Error(ApiProblemDetails problemDetails)
    {
        return StatusCode(problemDetails.Status, problemDetails);
    }

    protected IActionResult HandleErrorResponse<T>(Response<T> response)
    {
        var instance = $"{Request.Path}{Request.QueryString}";

        ApiProblemDetails problemDetails = response.Error switch
        {
            ErrorType.ValidationError => ApiProblemDetailsFactory.Create(
                StatusCodes.Status400BadRequest,
                "Validation Error",
                response.Message ?? "A requisição é inválida",
                instance),
            ErrorType.NotFound => ApiProblemDetailsFactory.CreateNotFound(
                response.Message ?? "Recurso não encontrado",
                instance),
            ErrorType.Conflict => ApiProblemDetailsFactory.Create(
                StatusCodes.Status409Conflict,
                "Conflict",
                response.Message ?? "Um conflito ocorreu ao processar a requisição",
                instance),
            ErrorType.DatabaseError => ApiProblemDetailsFactory.CreateInternalServerError(
                response.Message ?? "Um erro ocorreu ao processar a operação no banco de dados",
                instance),
            _ => ApiProblemDetailsFactory.Create(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                response.Message ?? "Um erro ocorreu ao processar a requisição",
                instance)
        };

        return Error(problemDetails);
    }
}