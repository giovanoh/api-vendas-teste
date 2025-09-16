using FluentAssertions;

using Vendas.API.Validation;

namespace Vendas.API.Tests.Validation;

public class Base64ValidationAttributeTests
{
    [Fact]
    public void IsValid_ShouldReturnTrue_WhenBase64IsValid()
    {
        var attribute = new Base64ValidationAttribute();
        var base64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD/";

        var result = attribute.IsValid(base64);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenBase64IsInvalid()
    {
        var attribute = new Base64ValidationAttribute();
        var base64 = "invalid";

        var result = attribute.IsValid(base64);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenBase64IsNull()
    {
        var attribute = new Base64ValidationAttribute();
        var base64 = null as string;

        var result = attribute.IsValid(base64);

        result.Should().BeTrue();
    }
}