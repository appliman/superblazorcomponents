namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

public sealed record SuperDataGridEnumFilterSelection(IReadOnlyList<string> SelectedValues)
{
    public static SuperDataGridEnumFilterSelection Empty { get; } = new([]);
}
