namespace SuperBlazorComponents.Components.GoogleCharts;

public class GoogleChartOptions
{
	public string Title { get; set; } = string.Empty;
	public string Width { get; set; } = "100%";
	public string Height { get; set; } = "400";
	public GoogleChartSeriesType DefaultSeriesType { get; set; } = GoogleChartSeriesType.Line;
	public Dictionary<int, GoogleChartSeries> Series { get; set; } = new();
	public string? BarGroupWidth { get; set; }
	public string HAxisTitle { get; set; } = string.Empty;
	public List<int>? HAxisTickIndices { get; set; }
	public string VAxisTitle { get; set; } = string.Empty;
	public string? VAxis2Title { get; set; }
	public decimal? VAxisMinValue { get; set; }
	public decimal? VAxisMaxValue { get; set; }
	public string? VAxisFormat { get; set; }
	public decimal? VAxis2MinValue { get; set; }
	public string? VAxis2Format { get; set; }
	public bool ShowLegend { get; set; } = true;
	public string LegendPosition { get; set; } = "right";
	public bool EnableAnimation { get; set; } = true;
	public int AnimationDuration { get; set; } = 1000;
	public string AnimationEasing { get; set; } = "out";
	public bool EnableInteraction { get; set; } = true;
	public bool ShowCrosshair { get; set; } = false;
	public bool IsStacked { get; set; } = false;
	public bool AlternatingRowStyle { get; set; } = false;
	public string? BackgroundColor { get; set; }
	public bool AllowFullscreen { get; set; } = false;
	public int? ChartAreaLeft { get; set; }
	public int? ChartAreaTop { get; set; }
	public int? ChartAreaRight { get; set; }
	public int? ChartAreaBottom { get; set; }
	public string? ChartAreaWidth { get; set; }
	public string? ChartAreaHeight { get; set; }
 public string? ChartAreaBackgroundColor { get; set; }
	public int? HAxisTextFontSize { get; set; }
	public int? VAxisTextFontSize { get; set; }
	public Dictionary<int, TrendLine> TrendLines { get; set; } = new();
	public int? HAxisGridlinesCount { get; set; }
	public string? HAxisGridlinesColor { get; set; }
	public int? VAxisGridlinesCount { get; set; }
	public string? VAxisGridlinesColor { get; set; }
}
