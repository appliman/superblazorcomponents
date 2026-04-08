using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.GoogleCharts;

public partial class GoogleComboChart : IAsyncDisposable
{
	[Parameter]
	public List<GoogleChartColumn> Columns { get; set; } = new();

	[Parameter]
	public List<GoogleChartDataRow> Data { get; set; } = new();

	[Parameter]
	public GoogleChartOptions Options { get; set; } = new();

	[Inject]
	private IJSRuntime JSRuntime { get; set; } = null!;

	[Inject]
	private ILogger<GoogleComboChart> Logger { get; set; } = null!;

	private string _chartId = $"google-chart-{Guid.NewGuid()}";
	private bool _isInitialized = false;
	private string? _errorMessage;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
     if (firstRender)
		{
			_isLoading = false;
			StateHasChanged();
			return;
		}

		if (_isInitialized)
		{
			return;
		}

		try
		{
			Logger.LogTrace("Début de l'initialisation du graphique {ChartId}", _chartId);
			await InitializeChart();
			_isInitialized = true;
			_errorMessage = null;
			Logger.LogTrace("Graphique {ChartId} initialisé avec succès", _chartId);
		}
		catch (Exception ex)
		{
			_errorMessage = $"Erreur: {ex.Message}";
			Logger.LogError(ex, "Erreur lors de l'initialisation du graphique {ChartId}", _chartId);
		}

		StateHasChanged();
	}

	protected override async Task OnParametersSetAsync()
	{
      if (_isInitialized)
		{
			try
			{
				await UpdateChart();
				Logger.LogTrace("Graphique {ChartId} mis à jour", _chartId);
			}
			catch (Exception ex)
			{
				_errorMessage = $"Erreur de mise à jour: {ex.Message}";
				Logger.LogError(ex, "Erreur lors de la mise à jour du graphique {ChartId}", _chartId);
				StateHasChanged();
			}
		}
	}

	private async Task InitializeChart()
	{
		var chartData = PrepareChartData();
		var optionsJson = PrepareOptions();

		await JSRuntime.InvokeVoidAsync("googleChartsInterop.initializeChart", _chartId, chartData, optionsJson);
	}

	private async Task UpdateChart()
	{
		var chartData = PrepareChartData();
		var optionsJson = PrepareOptions();

		await JSRuntime.InvokeVoidAsync("googleChartsInterop.updateChart", _chartId, chartData, optionsJson);
	}

	private List<object> PrepareChartData()
	{
		var dataArray = new List<object>();
		var hasTooltips = Data.Any(row => row.Tooltips.Any(t => !string.IsNullOrEmpty(t)));

		var headers = new List<object>();

		var firstColumn = Columns.First();
		headers.Add(new
		{
			type = firstColumn.Type,
			label = firstColumn.Label
		});

		foreach (var column in Columns.Skip(1))
		{
			var header = new
			{
				type = column.Type,
				label = column.Label,
				role = column.Role
			};
			headers.Add(header);

			if (hasTooltips && column.Role == null && column.Type == "number")
			{
				headers.Add(new { type = "string", role = "tooltip" });
			}
		}

		headers.Add(new { type = "string", role = "annotation" });

		dataArray.Add(headers);

		foreach (var row in Data)
		{
			var rowData = new List<object?> { row.Label };

			for (int i = 0; i < row.Values.Count; i++)
			{
				rowData.Add(row.Values[i]);

				if (i < row.Tooltips.Count && !string.IsNullOrEmpty(row.Tooltips[i]))
				{
					rowData.Add(row.Tooltips[i]);
				}
				else if (hasTooltips)
				{
					rowData.Add(null);
				}
			}

			if (!string.IsNullOrWhiteSpace(row.Annotation))
			{
				rowData.Add(row.Annotation);
			}
			else
			{
				rowData.Add(null);
			}

			dataArray.Add(rowData);
		}

		return dataArray;
	}

	private object PrepareOptions()
	{
		var gridlinesConfig = new Dictionary<string, object>
		{
			["color"] = Options.HAxisGridlinesColor ?? "#e6e6e6"
		};

		if (Options.HAxisGridlinesCount.HasValue)
		{
			gridlinesConfig["count"] = Options.HAxisGridlinesCount.Value;
		}
		else
		{
			gridlinesConfig["count"] = -1;
		}

		var hAxisConfig = new Dictionary<string, object>
		{
			["title"] = Options.HAxisTitle,
			["format"] = "MMM yy",
			["slantedText"] = true,
			["slantedTextAngle"] = 45,
			["maxTextLines"] = 1,
			["gridlines"] = gridlinesConfig,
			["minorGridlines"] = new { count = 0 }
		};

		if (Options.HAxisTextFontSize.HasValue)
		{
			hAxisConfig["textStyle"] = new { fontSize = Options.HAxisTextFontSize.Value };
		}

		var options = new Dictionary<string, object>
		{
			["title"] = Options.Title,
			["width"] = Options.Width,
			["height"] = Options.Height,
			["seriesType"] = ConvertSeriesType(Options.DefaultSeriesType),
			["hAxis"] = hAxisConfig,
			["vAxis"] = new Dictionary<string, object>
			{
				["title"] = Options.VAxisTitle
			},
			["legend"] = new { position = Options.ShowLegend ? Options.LegendPosition : "none" },
			["animation"] = new
			{
				startup = Options.EnableAnimation,
				duration = Options.AnimationDuration,
				easing = Options.AnimationEasing
			},
			["explorer"] = new { actions = Options.EnableInteraction ? new[] { "dragToZoom", "rightClickToReset" } : Array.Empty<string>() },
			["isStacked"] = Options.IsStacked,
			["locale"] = "fr"
		};

		if (!string.IsNullOrEmpty(Options.BarGroupWidth))
		{
			options["bar"] = new { groupWidth = Options.BarGroupWidth };
		}

		var vAxisDict = (Dictionary<string, object>)options["vAxis"];
		if (Options.VAxisMinValue.HasValue)
		{
			vAxisDict["minValue"] = Options.VAxisMinValue.Value;
		}
		if (Options.VAxisMaxValue.HasValue)
		{
			vAxisDict["maxValue"] = Options.VAxisMaxValue.Value;
		}
		if (!string.IsNullOrEmpty(Options.VAxisFormat) && !Options.Series.Any(i => i.Value.Format is not null))
		{
			vAxisDict["format"] = Options.VAxisFormat;
		}
		if (Options.VAxisTextFontSize.HasValue)
		{
			vAxisDict["textStyle"] = new { fontSize = Options.VAxisTextFontSize.Value };
		}

		if (Options.VAxisGridlinesCount.HasValue || Options.VAxisGridlinesColor != null)
		{
			var vAxisGridlines = new Dictionary<string, object>();

			if (Options.VAxisGridlinesCount.HasValue)
			{
				vAxisGridlines["count"] = Options.VAxisGridlinesCount.Value;
			}

			if (Options.VAxisGridlinesColor != null)
			{
				vAxisGridlines["color"] = Options.VAxisGridlinesColor;
			}

			vAxisDict["gridlines"] = vAxisGridlines;
		}

		if (Options.ShowCrosshair)
		{
			options["crosshair"] = new { trigger = "both", orientation = "both" };
		}

		if (!string.IsNullOrEmpty(Options.BackgroundColor))
		{
			options["backgroundColor"] = Options.BackgroundColor;
		}

		var chartAreaConfig = new Dictionary<string, object>();

		if (Options.ChartAreaLeft.HasValue)
		{
			chartAreaConfig["left"] = Options.ChartAreaLeft.Value;
		}
		if (Options.ChartAreaTop.HasValue)
		{
			chartAreaConfig["top"] = Options.ChartAreaTop.Value;
		}
		if (Options.ChartAreaRight.HasValue)
		{
			chartAreaConfig["right"] = Options.ChartAreaRight.Value;
		}
		if (Options.ChartAreaBottom.HasValue)
		{
			chartAreaConfig["bottom"] = Options.ChartAreaBottom.Value;
		}
		if (!string.IsNullOrEmpty(Options.ChartAreaWidth))
		{
			chartAreaConfig["width"] = Options.ChartAreaWidth;
		}
		if (!string.IsNullOrEmpty(Options.ChartAreaHeight))
		{
			chartAreaConfig["height"] = Options.ChartAreaHeight;
		}

		if (!string.IsNullOrEmpty(Options.ChartAreaBackgroundColor))
		{
			chartAreaConfig["backgroundColor"] = new { fill = Options.ChartAreaBackgroundColor };
		}

		if (chartAreaConfig.Any())
		{
			options["chartArea"] = chartAreaConfig;
		}

		if (Options.Series.Any())
		{
			var seriesDict = new Dictionary<int, object>();
			foreach (var kvp in Options.Series)
			{
				var seriesConfig = new Dictionary<string, object>
				{
					["type"] = ConvertSeriesType(kvp.Value.Type)
				};

				if (!string.IsNullOrEmpty(kvp.Value.Color))
				{
					seriesConfig["color"] = kvp.Value.Color;
				}
				if (kvp.Value.LineWidth.HasValue)
				{
					seriesConfig["lineWidth"] = kvp.Value.LineWidth.Value;
				}
				if (kvp.Value.BarWidth.HasValue)
				{
					seriesConfig["barWidth"] = kvp.Value.BarWidth.Value;
				}
				if (kvp.Value.PointSize.HasValue)
				{
					seriesConfig["pointSize"] = kvp.Value.PointSize.Value;
				}
				if (kvp.Value.PointShape is not null)
				{
					seriesConfig["pointShape"] = kvp.Value.PointShape;
				}
				if (kvp.Value.TargetAxisIndex != 0)
				{
					seriesConfig["targetAxisIndex"] = kvp.Value.TargetAxisIndex;
				}
				if (kvp.Value.VisibleInLegend.HasValue)
				{
					seriesConfig["visibleInLegend"] = kvp.Value.VisibleInLegend.Value;
				}
				if (kvp.Value.Format is not null)
				{
					seriesConfig["format"] = kvp.Value.Format;
				}
				if (kvp.Value.MinValue.HasValue)
				{
					vAxisDict["minValue"] = kvp.Value.MinValue.Value;
				}

				seriesDict[kvp.Key] = seriesConfig;
			}
			options["series"] = seriesDict;
		}

		if (Options.TrendLines.Any())
		{
			var trendLinesDict = new Dictionary<int, object>();
			foreach (var kvp in Options.TrendLines)
			{
				var trendLineConfig = new Dictionary<string, object>
				{
					["type"] = kvp.Value.Type,
				};
				if (kvp.Value.Degree.HasValue)
				{
					trendLineConfig["degree"] = kvp.Value.Degree.Value;
				}
				if (!string.IsNullOrEmpty(kvp.Value.Color))
				{
					trendLineConfig["color"] = kvp.Value.Color;
				}
				if (kvp.Value.LineWidth.HasValue)
				{
					trendLineConfig["lineWidth"] = kvp.Value.LineWidth.Value;
				}
				if (kvp.Value.Opacity.HasValue)
				{
					trendLineConfig["opacity"] = kvp.Value.Opacity.Value;
				}
				if (kvp.Value.VisibleInLegend.HasValue)
				{
					trendLineConfig["visibleInLegend"] = kvp.Value.VisibleInLegend.Value;
				}
				if (!string.IsNullOrEmpty(kvp.Value.LabelInLegend))
				{
					trendLineConfig["labelInLegend"] = kvp.Value.LabelInLegend;
				}
				if (kvp.Value.PointSize.HasValue)
				{
					trendLineConfig["pointSize"] = kvp.Value.PointSize.Value;
				}
				trendLinesDict[kvp.Key] = trendLineConfig;
			}
			options["trendlines"] = trendLinesDict;
		}

		if (!string.IsNullOrEmpty(Options.VAxis2Title))
		{
			var rightAxis = new Dictionary<string, object>
			{
				["title"] = Options.VAxis2Title
			};
			if (Options.VAxis2MinValue.HasValue)
			{
				rightAxis["minValue"] = Options.VAxis2MinValue.Value;
			}
			if (!string.IsNullOrEmpty(Options.VAxis2Format) && !Options.Series.Any(s => s.Value.TargetAxisIndex == 1 && s.Value.Format is not null))
			{
				rightAxis["format"] = Options.VAxis2Format;
			}
			options["vAxes"] = new Dictionary<int, object>
			{
				[0] = vAxisDict,
				[1] = rightAxis
			};
		}

		return options;
	}

	private string ConvertSeriesType(GoogleChartSeriesType type)
	{
		return type switch
		{
			GoogleChartSeriesType.Line => "line",
			GoogleChartSeriesType.Area => "area",
			GoogleChartSeriesType.Bars => "bars",
			GoogleChartSeriesType.Columns => "bars",
			GoogleChartSeriesType.Scatter => "scatter",
			_ => "line"
		};
	}

	public async ValueTask DisposeAsync()
	{
        try
		{
			await JSRuntime.InvokeVoidAsync("googleChartsInterop.dispose", _chartId);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Erreur lors de la libération du graphique {ChartId}", _chartId);
		}
	}
}
