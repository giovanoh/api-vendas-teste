using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Vendas.API.Infrastructure.Conventions;

public class AutoRouteNamingConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        if (!IsCrudController(action.Controller.ControllerType))
            return;

        var methodName = action.ActionMethod?.Name ?? action.ActionName;
        var isGetById = string.Equals(methodName, "GetById", StringComparison.Ordinal)
                        || string.Equals(methodName, "GetByIdAsync", StringComparison.Ordinal);
        if (!isGetById)
            return;

        var controllerName = action.Controller.ControllerName.Replace("Controller", "");
        var routeName = $"{controllerName}GetById";

        foreach (var selector in action.Selectors)
        {
            if (selector.AttributeRouteModel != null && string.IsNullOrEmpty(selector.AttributeRouteModel.Name))
            {
                selector.AttributeRouteModel.Name = routeName;
            }
        }
    }

    private static bool IsCrudController(Type controllerType)
    {
        var currentType = controllerType;
        while (currentType != null)
        {
            if (currentType.IsGenericType &&
                currentType.GetGenericTypeDefinition().Name.StartsWith("CrudController"))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }
}
