using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

internal static class EnumDisplayExtensions
{
    internal static string GetDisplayName(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var attribute = GetDisplayAttribute(value);
        var displayName = attribute?.GetName();
        return string.IsNullOrWhiteSpace(displayName) ? value.ToString() : displayName;
    }

    internal static string? GetDisplayDescription(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var description = GetDisplayAttribute(value)?.GetDescription();
        return string.IsNullOrWhiteSpace(description) ? null : description;
    }

    private static DisplayAttribute? GetDisplayAttribute(Enum value)
    {
        var field = value.GetType().GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<DisplayAttribute>();
    }
}
