using AutoMapper;

using Vendas.API.Domain.Models;
using Vendas.API.DTOs;

namespace Vendas.API.Mapping;

public class ModelToDtoProfile : Profile
{
    public ModelToDtoProfile()
    {
        CreateMap<Cliente, ClienteDto>();
        CreateMap<Produto, ProdutoDto>();

        CreateMap<Venda, VendaDto>()
            .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src => src.Cliente.Nome));

        CreateMap<Item, ItemDto>()
            .ForMember(dest => dest.NomeProduto, opt => opt.MapFrom(src => src.Produto.Nome));
    }
}