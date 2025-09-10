using System.ComponentModel.DataAnnotations;

namespace Vendas.API.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class Base64Attribute : ValidationAttribute
{
    public Base64Attribute() : base("O campo {0} não é um base64 válido.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }
        if (value is string base64Value)
        {
            return base64Value.StartsWith("data:image/") && base64Value.Contains("base64,");
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name);
    }
}