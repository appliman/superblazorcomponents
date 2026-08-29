using System.Collections.Immutable;

using SuperBlazorComponents.Components.SuperDateRange;

namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Immutable snapshot of the query currently displayed by a <see cref="SuperDataGrid{TItem}"/>.
/// </summary>
public sealed record SuperDataGridQuerySnapshot(
    string? SortColumn,
    SortDirection SortDirection,
    ImmutableArray<SuperDataGridFilterSnapshot> Filters);

/// <summary>
/// Immutable representation of one active grid filter.
/// </summary>
public sealed record SuperDataGridFilterSnapshot(
    string PropertyName,
    string? PropertyValue,
    ImmutableArray<string> SelectedValues,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    long? FromNumericValue,
    long? ToNumericValue,
    string? PeriodName,
    SuperDateRangePreset? PeriodPreset,
    Type PropertyType,
    SuperDataGridFilterOperator Operator)
{
    public SuperDataGridFilterInfo ToFilterInfo() => new()
    {
        PropertyName = PropertyName,
        PropertyValue = PropertyValue,
        SelectedValues = SelectedValues,
        StartDate = StartDate,
        EndDate = EndDate,
        FromNumericValue = FromNumericValue,
        ToNumericValue = ToNumericValue,
        PeriodName = PeriodName,
        PeriodPreset = PeriodPreset,
        PropertyType = PropertyType,
        Operator = Operator
    };
}
