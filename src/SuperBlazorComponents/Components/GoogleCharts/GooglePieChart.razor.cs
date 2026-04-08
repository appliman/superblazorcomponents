using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.GoogleCharts;

public partial class GooglePieChart : IAsyncDisposable
{
    [Parameter]
    public List<GoogleChartColumn> Columns { get; set; } = new();

    [Parameter]
    public List<GoogleChartDataRow> Data { get; set; } = new();

    [Parameter]
    public GooglePieChartOptions Options { get; set; } = new();

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    private ILogger<GooglePieChart> Logger { get; set; } = null!;

    private readonly string _chartId = $"google-pie-chart-{Guid.NewGuid()}";
    private bool _isInitialized;
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

        await JSRuntime.InvokeVoidAsync("googleChartsInterop.initializePieChart", _chartId, chartData, optionsJson);
    }

    private async Task UpdateChart()
    {
        var chartData = PrepareChartData();
        var optionsJson = PrepareOptions();

        await JSRuntime.InvokeVoidAsync("googleChartsInterop.updatePieChart", _chartId, chartData, optionsJson);
    }

    private List<object> PrepareChartData()
    {
        var dataArray = new List<object>();
        if (!Columns.Any())
        {
            return dataArray;
        }

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

        dataArray.Add(headers);

        foreach (var row in Data)
        {
            var rowData = new List<object?> { row.Label };

            for (var i = 0; i < row.Values.Count; i++)
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

            dataArray.Add(rowData);
        }

        return dataArray;
    }

    private object PrepareOptions()
    {
        var options = new Dictionary<string, object>
        {
            ["title"] = Options.Title,
            ["width"] = Options.Width,
            ["height"] = Options.Height,
            ["legend"] = new { position = Options.ShowLegend ? Options.LegendPosition : "none" },
            ["is3D"] = Options.Is3D
        };

        if (Options.PieHole.HasValue)
        {
            options["pieHole"] = Options.PieHole.Value;
        }

        if (!string.IsNullOrEmpty(Options.PieSliceText))
        {
            options["pieSliceText"] = Options.PieSliceText;
        }

        if (Options.PieStartAngle.HasValue)
        {
            options["pieStartAngle"] = Options.PieStartAngle.Value;
        }

        if (Options.SliceVisibilityThreshold.HasValue)
        {
            options["sliceVisibilityThreshold"] = Options.SliceVisibilityThreshold.Value;
        }

        if (!string.IsNullOrEmpty(Options.BackgroundColor))
        {
            options["backgroundColor"] = Options.BackgroundColor;
        }

        if (!string.IsNullOrEmpty(Options.ChartAreaBackgroundColor))
        {
            options["chartArea"] = new
            {
                backgroundColor = new { fill = Options.ChartAreaBackgroundColor }
            };
        }

        return options;
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
