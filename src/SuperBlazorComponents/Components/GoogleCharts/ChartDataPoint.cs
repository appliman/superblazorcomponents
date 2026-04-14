namespace SuperBlazorComponents.Components.GoogleCharts;

public class ChartDataPoint
{
	public DateTimeOffset Date { get; set; }
	public decimal Value { get; set; }
	public string? Label { get; set; }
	public bool IsHighlighted { get; set; }
}
