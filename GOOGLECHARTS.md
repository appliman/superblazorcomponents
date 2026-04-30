# 📊 Google Charts

> Three Blazor chart components: `GoogleComboChart` (line/area/bar/column/scatter combos), `GooglePieChart` (pies, donuts, 3D), and `TimeSeriesChart` (pure-SVG time series — no third-party JS).

[← Back to README](README.md)

---

## 📑 Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Architecture](#architecture)
- [GoogleComboChart](#googlecombochart)
- [GooglePieChart](#googlepiechart)
- [TimeSeriesChart](#timeserieschart)
- [Models & Enums](#models--enums)
- [Tips & Best Practices](#tips--best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The `GoogleCharts` namespace ships three different chart components:

| Component | Renderer | Use case |
|---|---|---|
| `GoogleComboChart` | Google Charts JS (line / area / bars / columns / scatter) | Multi-series charts with dual axes, trend lines, annotations |
| `GooglePieChart` | Google Charts JS (pie / donut / 3D) | Distributions and proportions |
| `TimeSeriesChart` | **Pure SVG** (no JS dependency) | Single-series time series with month markers, weekend bands, week separators |

All three accept strongly-typed C# data — no JSON gymnastics required.

---

## Getting Started

### Service registration

```csharp
builder.Services.AddSuperComponents();
```

### Imports

```razor
@using SuperBlazorComponents.Components.GoogleCharts
```

### Reference Google Charts JS

`GoogleComboChart` and `GooglePieChart` rely on the Google Charts loader. Add this to `App.razor` (or `_Host.cshtml`):

```html
<script src="https://www.gstatic.com/charts/loader.js"></script>
<script src="_content/SuperBlazorComponents/js/google-charts-interop.js"></script>
```

`TimeSeriesChart` has **no** JS dependency.

---

## Architecture

```mermaid
flowchart TB
    subgraph JS["Google Charts (JS)"]
        Combo["GoogleComboChart.razor"] --> Interop["googleChartsInterop.initializeChart"]
        Pie["GooglePieChart.razor"]   --> Interop2["googleChartsInterop.initializePieChart"]
    end
    subgraph SVG["Pure SVG (no JS)"]
        TS["TimeSeriesChart.razor"]
    end
    Combo  -.->|Columns + Data + Options| Render
    Pie    -.->|Columns + Data + Options| Render
    TS     -.->|Data + Options|           Render
    Render["Rendered chart"]
```

---

## GoogleComboChart

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Columns` | `List<GoogleChartColumn>` | `[]` | Schema (first column is the X axis label) |
| `Data` | `List<GoogleChartDataRow>` | `[]` | Rows of data |
| `Options` | `GoogleChartOptions` | `new()` | All chart options (see below) |

### Example — multi-series combo with dual axis

```razor
<GoogleComboChart Columns="_columns" Data="_rows" Options="_options" />

@code {
    private readonly List<GoogleChartColumn> _columns = new()
    {
        new() { Type = "string", Label = "Month" },
        new() { Type = "number", Label = "Revenue" },
        new() { Type = "number", Label = "Forecast" }
    };

    private readonly List<GoogleChartDataRow> _rows = new()
    {
        new() { Label = "Jan", Values = new() { 12000m, 11000m } },
        new() { Label = "Feb", Values = new() { 14500m, 13500m } },
        new() { Label = "Mar", Values = new() { 13000m, 14000m } },
    };

    private readonly GoogleChartOptions _options = new()
    {
        Title = "Revenue vs Forecast",
        Width = "100%", Height = "360",
        DefaultSeriesType = GoogleChartSeriesType.Columns,
        Series = new()
        {
            [1] = new GoogleChartSeries { Type = GoogleChartSeriesType.Line, Color = "#dc3545" }
        },
        VAxisTitle = "EUR",
        ShowLegend = true
    };
}
```

### Common options

| Option | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Chart title |
| `Width` / `Height` | `string` | `"100%"` / `"400"` | CSS dimensions |
| `DefaultSeriesType` | `GoogleChartSeriesType` | `Line` | Default for all series |
| `Series` | `Dictionary<int, GoogleChartSeries>` | `{}` | Override per-index series settings |
| `IsStacked` | `bool` | `false` | Stack columns/areas |
| `VAxisTitle` / `VAxis2Title` | `string` | — | Axis titles |
| `VAxisMinValue` / `VAxisMaxValue` | `decimal?` | — | Bounds |
| `VAxisFormat` / `VAxis2Format` | `string?` | — | Number format |
| `ShowLegend` | `bool` | `true` | Show/hide legend |
| `LegendPosition` | `string` | `"right"` | `top`, `bottom`, `left`, `right` |
| `EnableAnimation` | `bool` | `true` | First-render animation |
| `AnimationDuration` | `int` | `1000` | ms |
| `BackgroundColor` | `string?` | — | Outer background |
| `ChartAreaBackgroundColor` | `string?` | — | Plot area background |
| `TrendLines` | `Dictionary<int, TrendLine>` | `{}` | Add a trend line for series at index N |
| `HAxisGridlinesCount` / `Color` | — | — | Custom gridlines |
| `AllowFullscreen` | `bool` | `false` | Provides a fullscreen escape hatch |

### Trend line example

```csharp
_options.TrendLines[0] = new TrendLine
{
    Type = "linear",
    Color = "#198754",
    LineWidth = 2,
    ShowR2 = true,
    LabelInLegend = "Trend"
};
```

### Tooltips & annotations

Each `GoogleChartDataRow` exposes `Tooltips` (one entry per value) and `Annotation`. They are forwarded to Google Charts as `role="tooltip"` and `role="annotation"` columns:

```csharp
new GoogleChartDataRow
{
    Label = "Apr",
    Values = new() { 17000m, 16500m },
    Tooltips = new() { "Best month so far!", null },
    Annotation = "🚀"
}
```

---

## GooglePieChart

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Columns` | `List<GoogleChartColumn>` | `[]` | Two columns expected: label + value |
| `Data` | `List<GoogleChartDataRow>` | `[]` | Slices |
| `Options` | `GooglePieChartOptions` | `new()` | Pie-specific options |

### Example — donut chart

```razor
<GooglePieChart Columns="_pieColumns" Data="_pieData" Options="_pieOptions" />

@code {
    private readonly List<GoogleChartColumn> _pieColumns = new()
    {
        new() { Type = "string", Label = "Channel" },
        new() { Type = "number", Label = "Sales"  }
    };

    private readonly List<GoogleChartDataRow> _pieData = new()
    {
        new() { Label = "Online",   Values = new() { 65m } },
        new() { Label = "Retail",   Values = new() { 25m } },
        new() { Label = "Wholesale", Values = new() { 10m } }
    };

    private readonly GooglePieChartOptions _pieOptions = new()
    {
        Title = "Sales by channel",
        PieHole = 0.4,           // donut
        PieSliceText = "percentage",
        ShowLegend = true,
        LegendPosition = "bottom"
    };
}
```

### Options

| Option | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Chart title |
| `Width` / `Height` | `string` | `"100%"` / `"400"` | Dimensions |
| `Is3D` | `bool` | `false` | 3D pie |
| `PieHole` | `double?` | `null` | `0..1` — turns the pie into a donut |
| `PieSliceText` | `string?` | `null` | `percentage`, `value`, `label`, `none` |
| `PieStartAngle` | `int?` | — | Rotation in degrees |
| `SliceVisibilityThreshold` | `double?` | — | Group small slices into "Other" |
| `ShowLegend` / `LegendPosition` | — | — | Same as combo chart |
| `BackgroundColor` / `ChartAreaBackgroundColor` | — | — | Backgrounds |

---

## TimeSeriesChart

A **pure SVG** chart (no JS interop) that draws a single time series with optional month markers, weekend bands, and week separators. Ideal for dashboards and printable reports.

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Data` | `List<ChartDataPoint>` | `[]` | Points (`Date`, `Value`, optional `Label`, `IsHighlighted`) |
| `Options` | `ChartOptions` | `new()` | Visual options |

### Example

```razor
<TimeSeriesChart Data="_points" Options="_chartOptions" />

@code {
    private readonly List<ChartDataPoint> _points = Enumerable.Range(0, 30)
        .Select(i => new ChartDataPoint
        {
            Date  = DateTimeOffset.UtcNow.AddDays(-30 + i),
            Value = 1000 + Random.Shared.Next(-200, 200)
        })
        .ToList();

    private readonly ChartOptions _chartOptions = new()
    {
        Title          = "Daily revenue",
        YAxisTitle     = "EUR",
        Height         = 320,
        ValueFormat    = ValueFormat.Currency,
        CurrencySymbol = "€",
        LineColor      = "#0d6efd",
        ShowMarkers    = true,
        ShowMonthMarkers = true,
        ShowWeekendBands = true,
        Culture        = "en-US"
    };
}
```

### Options

| Option | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `"Graphique"` | Chart title |
| `YAxisTitle` | `string` | `"Valeur"` | Y label |
| `Height` / `Width` | `int` | `300` / `0` (auto) | Pixels |
| `ValueFormat` | `ValueFormat` | `Decimal` | `Integer`, `Decimal`, `Percentage`, `Currency` |
| `DecimalPlaces` | `int` | `2` | For `Decimal` / `Percentage` / `Currency` |
| `CurrencySymbol` | `string` | `"€"` | When `ValueFormat = Currency` |
| `LineColor` | `string` | `"#4A90E2"` | Stroke |
| `LineWidth` | `double` | `2` | Stroke width |
| `ShowMarkers` | `bool` | `false` | Circles on each data point |
| `ShowVerticalGrid` / `ShowHorizontalGrid` | `bool` | `true` | Grid |
| `ShowMonthMarkers` | `bool` | `true` | Month labels on the X axis |
| `ShowWeekendBands` | `bool` | `true` | Light gray bands for Saturdays |
| `ShowWeekSeparators` | `bool` | `true` | Vertical separators on Mondays |
| `Culture` | `string` | `"fr-FR"` | Date/number formatting |
| `MinValue` / `MaxValue` | `decimal?` | — | Force Y bounds |
| `Padding` | `ChartPadding` | `top=50, right=40, bottom=100, left=70` | Chart-area padding |

---

## Models & Enums

### `GoogleChartColumn`

```csharp
public class GoogleChartColumn
{
    public string Label { get; set; } = "";
    public string Type  { get; set; } = "number"; // "string" | "number" | "date" | "datetime"
    public string? Role { get; set; }              // e.g. "tooltip", "annotation"
}
```

### `GoogleChartDataRow`

```csharp
public class GoogleChartDataRow
{
    public object Label { get; set; } = default!;
    public List<decimal?> Values   { get; set; } = new();
    public List<string?>  Tooltips { get; set; } = new();
    public string? Annotation      { get; set; }
}
```

### `GoogleChartSeries`

```csharp
public class GoogleChartSeries
{
    public GoogleChartSeriesType Type { get; set; } = GoogleChartSeriesType.Line;
    public string? Color { get; set; }
    public int? LineWidth { get; set; }
    public int? BarWidth { get; set; }
    public int? PointSize { get; set; }
    public string? PointShape { get; set; }
    public int TargetAxisIndex { get; set; }   // 0 or 1 for dual axis
    public bool? VisibleInLegend { get; set; }
    public string? Format { get; set; }
}
```

### `TrendLine`

```csharp
public class TrendLine
{
    public string Type { get; set; } = "linear";    // "linear" | "exponential" | "polynomial"
    public int? Degree { get; set; }
    public string? Color { get; set; }
    public int? LineWidth { get; set; }
    public double? Opacity { get; set; }
    public bool? VisibleInLegend { get; set; }
    public string? LabelInLegend { get; set; }
    public bool? ShowEquation { get; set; }
    public bool? ShowR2 { get; set; }
}
```

### Enums

```csharp
public enum GoogleChartSeriesType { Line, Area, Bars, Columns, Scatter }
public enum ValueFormat            { Integer, Decimal, Percentage, Currency }
```

---

## Tips & Best Practices

- ✅ **Combo + line trend**: pair `DefaultSeriesType = Columns` with a `Series[N] = { Type = Line }` to create combos.
- ✅ Use `Tooltips` per row instead of relying on default tooltips for full control over the displayed text.
- ✅ For dual-axis charts, set `TargetAxisIndex = 1` on the series + `VAxis2Title` / `VAxis2Format` in options.
- ✅ Prefer **`TimeSeriesChart`** when you want SSR-friendly, JS-free output (it just renders SVG).
- ✅ Use `SliceVisibilityThreshold` on pies to fold tiny slices into a single "Other" group.
- ⚠️ Do not put `GoogleComboChart` / `GooglePieChart` in tabs that aren't yet rendered — Google Charts needs the container to exist and have a measurable size when initialized. Combine with `LazyLoad` of `SuperTabs`.
- ⚠️ The interop initializes once per chart instance; changing parameters triggers `updateChart` so the chart updates without re-mount.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Chart never appears, console says `google is not defined` | `loader.js` not included | Add `<script src="https://www.gstatic.com/charts/loader.js"></script>` |
| Chart shows then disappears on tab switch | Container detached during animation | Disable animation (`EnableAnimation = false`) or use `TimeSeriesChart` |
| Trend line not shown | `TrendLines[index]` index out of range | Use the **column** index (1-based: 0 = first numeric series) |
| Pie slices labeled "0%" | Values are integers stored as 0 | Pass `decimal` values; ensure `Values.Count == Columns.Count - 1` |
| Time series Y axis squished | Single value or zero range | Set `MinValue` / `MaxValue` on `ChartOptions` |
| Dates appear in wrong language | `Culture` left as default | Set `Culture = "en-US"` (or your culture) on `ChartOptions` |

---

[← Back to README](README.md)
