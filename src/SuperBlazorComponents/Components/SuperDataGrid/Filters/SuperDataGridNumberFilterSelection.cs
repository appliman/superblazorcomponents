using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

public sealed record SuperDataGridNumberFilterSelection(
    SuperDataGridFilterOperator? Operator,
    long? Value,
    long? ValueTo)
{
    public static SuperDataGridNumberFilterSelection Empty { get; } = new(null, null, null);
}
