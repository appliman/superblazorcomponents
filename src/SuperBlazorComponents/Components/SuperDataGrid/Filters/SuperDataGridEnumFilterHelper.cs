namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

internal static class SuperDataGridEnumFilterHelper
{
    internal sealed record EnumOption(string Value, string Label, string? Description);

    internal static IReadOnlyList<EnumOption> GetOptions(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        var normalizedType = Nullable.GetUnderlyingType(enumType) ?? enumType;
        if (!normalizedType.IsEnum)
        {
            return [];
        }

        var result = new List<EnumOption>();
        foreach (var enumValue in Enum.GetValues(normalizedType).Cast<Enum>())
        {
            var name = enumValue.ToString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            result.Add(new EnumOption(name, enumValue.GetDisplayName(), enumValue.GetDisplayDescription()));
        }

        return result;
    }

    internal static SuperDataGridEnumFilterSelection NormalizeSelection(
        SuperDataGridEnumFilterSelection? selection,
        IReadOnlyList<EnumOption> options)
    {
        if (selection is null || selection.SelectedValues.Count == 0 || options.Count == 0)
        {
            return SuperDataGridEnumFilterSelection.Empty;
        }

        var allowedValues = options.Select(option => option.Value).ToHashSet(StringComparer.Ordinal);
        var normalizedValues = selection.SelectedValues
            .Where(value => !string.IsNullOrWhiteSpace(value) && allowedValues.Contains(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedValues.Count == 0 || normalizedValues.Count == options.Count)
        {
            return SuperDataGridEnumFilterSelection.Empty;
        }

        var orderedValues = options
            .Where(option => normalizedValues.Contains(option.Value, StringComparer.Ordinal))
            .Select(option => option.Value)
            .ToList();

        return new SuperDataGridEnumFilterSelection(orderedValues);
    }
}
