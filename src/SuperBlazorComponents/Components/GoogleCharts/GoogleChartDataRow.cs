namespace SuperBlazorComponents.Components.GoogleCharts;

public class GoogleChartDataRow
{
	public object Label { get; set; } = default!;
	public List<decimal?> Values { get; set; } = new();
	public List<string?> Tooltips { get; set; } = new();
	public string? Annotation { get; set; }
}
