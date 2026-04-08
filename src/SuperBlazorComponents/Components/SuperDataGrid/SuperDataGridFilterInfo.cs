using SuperBlazorComponents.Components;
using SuperBlazorComponents.Components.SuperDateRange;

namespace SuperBlazorComponents.Components.SuperDataGrid;

public enum SuperDataGridFilterOperator
{
	Equals,
	NotEquals,
	Contains,
	StartsWith,
	EndsWith,
	GreaterThan,
	LessThan,
	GreaterThanOrEqual,
	LessThanOrEqual,
	Between,
	NotBetween
}

public class SuperDataGridFilterInfo
{
	public string PropertyName { get; set; } = null!;
	public string? PropertyValue { get; set; }
    public IReadOnlyList<string> SelectedValues { get; set; } = [];
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public long? FromNumericValue { get; set; }
	public long? ToNumericValue { get; set; }
	public string? PeriodName { get; set; }
	public SuperDateRangePreset? PeriodPreset { get; set; }
	public Type PropertyType { get; set; } = default!;
	public SuperDataGridFilterOperator Operator { get; set; } = SuperDataGridFilterOperator.Contains;
}
