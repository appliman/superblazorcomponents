namespace SuperBlazorComponents.Components.GoogleCharts;

public class GoogleChartSeries
{
	public GoogleChartSeriesType Type { get; set; } = GoogleChartSeriesType.Line;
	public string? Color { get; set; }
	public int? LineWidth { get; set; }
	public int? BarWidth { get; set; }
	public int? PointSize { get; set; }
	public string? PointShape { get; set; }
	public int TargetAxisIndex { get; set; } = 0;
	public bool? VisibleInLegend { get; set; }
	public string? Format { get; set; }
	public int? MinValue { get; set; }
}
