using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Removes endpoints marked <see cref="DevelopmentOnlyAttribute"/> from the application
/// model; on a controller the whole controller is removed, on an action only that action.
/// </summary>
public sealed class DevelopmentOnlyControllerConvention : IControllerModelConvention
{
    /// <inheritdoc />
    public void Apply(ControllerModel controller)
    {
        if (controller.Attributes.OfType<DevelopmentOnlyAttribute>().Any())
        {
            controller.Actions.Clear();
            return;
        }

        var devOnlyActions = controller
            .Actions.Where(action => action.Attributes.OfType<DevelopmentOnlyAttribute>().Any())
            .ToList();

        foreach (var action in devOnlyActions)
        {
            controller.Actions.Remove(action);
        }
    }
}