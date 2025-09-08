using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Mapping;

public class DtoToModelProfile : Profile
{
    public DtoToModelProfile()
    {
        CreateMap<SaveClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<SaveProdutoDto, Produto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImagemPath, opt => opt.Ignore());
    }
}