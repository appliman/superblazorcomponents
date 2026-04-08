using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

internal static class SuperDataGridNumberFilterOperatorHelper
{
    public static IReadOnlyList<KeyValuePair<SuperDataGridFilterOperator, string>> Options { get; } =
    [
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.Equals, "Egale"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.NotEquals, "Differente"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.GreaterThan, "Supérieure"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.GreaterThanOrEqual, "Supérieure ou égale"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.LessThan, "Inférieure"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.LessThanOrEqual, "Inférieure ou égale"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.Between, "Comprise entre"),
        new KeyValuePair<SuperDataGridFilterOperator, string>(SuperDataGridFilterOperator.NotBetween, "Exclue entre")
    ];

    public static string GetLabel(SuperDataGridFilterOperator filterOperator)
    {
        return Options.FirstOrDefault(option => option.Key == filterOperator).Value ?? filterOperator.ToString();
    }

    public static bool IsRangeOperator(SuperDataGridFilterOperator filterOperator)
    {
        return filterOperator is SuperDataGridFilterOperator.Between or SuperDataGridFilterOperator.NotBetween;
    }
}
