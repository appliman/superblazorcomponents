using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

internal static class SuperDataGridNumberFilterOperatorHelper
{
    public static IReadOnlyList<SuperDataGridFilterOperator> Options { get; } =
    [
        SuperDataGridFilterOperator.Equals,
        SuperDataGridFilterOperator.NotEquals,
        SuperDataGridFilterOperator.GreaterThan,
        SuperDataGridFilterOperator.GreaterThanOrEqual,
        SuperDataGridFilterOperator.LessThan,
        SuperDataGridFilterOperator.LessThanOrEqual,
        SuperDataGridFilterOperator.Between,
        SuperDataGridFilterOperator.NotBetween
    ];

    public static bool IsRangeOperator(SuperDataGridFilterOperator filterOperator)
    {
        return filterOperator is SuperDataGridFilterOperator.Between or SuperDataGridFilterOperator.NotBetween;
    }
}
