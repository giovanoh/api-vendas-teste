using AutoMapper;

using Vendas.API.Mapping;

namespace Vendas.API.Tests.Mapping;

public class MappingTestsBase
{
    protected readonly IMapper Mapper;

    public MappingTestsBase()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DtoToModelProfile>();
            cfg.AddProfile<ModelToDtoProfile>();
        });
        Mapper = configuration.CreateMapper();
    }

    [Fact]
    private void Should_Have_Valid_Configuration()
    {
        Mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}