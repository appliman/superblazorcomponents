namespace SuperBlazorComponents.Components.GoogleCharts;

public class GooglePieChartOptions
{
    public string Title { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public string Height { get; set; } = "400";
    public bool ShowLegend { get; set; } = true;
    public string LegendPosition { get; set; } = "right";
    public bool Is3D { get; set; }
    public double? PieHole { get; set; }
    public string? PieSliceText { get; set; }
    public int? PieStartAngle { get; set; }
    public double? SliceVisibilityThreshold { get; set; }
    public string? BackgroundColor { get; set; }
    public string? ChartAreaBackgroundColor { get; set; }
}
