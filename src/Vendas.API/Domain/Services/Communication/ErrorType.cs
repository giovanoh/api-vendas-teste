namespace Vendas.API.Domain.Services.Communication;

public enum ErrorType
{
    NotFound,
    ValidationError,
    DatabaseError,
    Conflict,
    Unknown
}