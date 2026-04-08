namespace SuperBlazorComponents.Components.GoogleCharts;

public class ChartOptions
{
	public string Title { get; set; } = "Graphique";
	public string YAxisTitle { get; set; } = "Valeur";
	public int Height { get; set; } = 300;
	public int Width { get; set; } = 0;
	public ValueFormat ValueFormat { get; set; } = ValueFormat.Decimal;
	public int DecimalPlaces { get; set; } = 2;
	public string CurrencySymbol { get; set; } = "€";
	public string LineColor { get; set; } = "#4A90E2";
	public double LineWidth { get; set; } = 2;
	public bool ShowMarkers { get; set; } = false;
	public bool ShowVerticalGrid { get; set; } = true;
	public bool ShowHorizontalGrid { get; set; } = true;
	public bool ShowMonthMarkers { get; set; } = true;
	public bool ShowWeekendBands { get; set; } = true;
	public bool ShowWeekSeparators { get; set; } = true;
	public string Culture { get; set; } = "fr-FR";
	public decimal? MinValue { get; set; }
	public decimal? MaxValue { get; set; }
	public ChartPadding Padding { get; set; } = new();
}
