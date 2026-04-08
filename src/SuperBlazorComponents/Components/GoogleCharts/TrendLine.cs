namespace SuperBlazorComponents.Components.GoogleCharts;

public class TrendLine
{
	public string Type { get; set; } = "linear";
	public int? Degree { get; set; }
	public string? Color { get; set; }
	public int? LineWidth { get; set; }
	public double? Opacity { get; set; }
	public bool? VisibleInLegend { get; set; }
	public string? LabelInLegend { get; set; }
	public int? PointSize { get; set; }
	public bool? PointsVisible { get; set; }
	public bool? ShowEquation { get; set; }
	public bool? ShowR2 { get; set; }
}
