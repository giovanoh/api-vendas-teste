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
            .ForMember(dest => dest.Imagem, opt => opt.Ignore());

        CreateMap<SaveVendaDto, Venda>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Cliente, opt => opt.Ignore());

        CreateMap<SaveItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Produto, opt => opt.Ignore())
            .ForMember(dest => dest.VendaId, opt => opt.Ignore());
    }
}