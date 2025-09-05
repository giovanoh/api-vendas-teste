using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Mapping;

public class ModelToDtoProfile : Profile
{
    public ModelToDtoProfile()
    {
        CreateMap<Cliente, ClienteDto>();
    }
}