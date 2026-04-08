using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.GoogleCharts;

public partial class TimeSeriesChart
{
	[Parameter]
	public List<ChartDataPoint> Data { get; set; } = new();

	[Parameter]
	public ChartOptions Options { get; set; } = new();

	private int _chartWidth;
	private int _chartHeight;
	private decimal _minValue;
	private decimal _maxValue;
	private DateTime _minDate;
	private DateTime _maxDate;
	private CultureInfo _culture = null!;
	private string _tooltipContent = string.Empty;
	private double _tooltipX = 0;
	private double _tooltipY = 0;
	private bool _showTooltip = false;

	protected override void OnParametersSet()
	{
		_culture = new CultureInfo(Options.Culture);
		CalculateBounds();
	}

	private void CalculateBounds()
	{
		if (!Data.Any())
		{
			_minValue = Options.MinValue ?? 0;
			_maxValue = Options.MaxValue ?? 100;
			_minDate = DateTime.Today;
			_maxDate = DateTime.Today.AddMonths(1);
			_chartWidth = 1000;
			_chartHeight = Options.Height;
			return;
		}

		var dataMinValue = Data.Min(d => d.Value);
		var dataMaxValue = Data.Max(d => d.Value);

		if (Options.MinValue.HasValue)
		{
			_minValue = Options.MinValue.Value;
		}
		else
		{
			var range = dataMaxValue - dataMinValue;
			if (range == 0)
			{
				range = 1;
			}
			_minValue = dataMinValue - range * 0.1m;
		}

		if (Options.MaxValue.HasValue)
		{
			_maxValue = Options.MaxValue.Value;
		}
		else
		{
			var range = dataMaxValue - dataMinValue;
			if (range == 0)
			{
				range = 1;
			}
			_maxValue = dataMaxValue + range * 0.1m;
		}

		_minDate = Data.Min(d => d.Date).Date;
		_maxDate = Data.Max(d => d.Date).Date;

		_chartHeight = Options.Height;
		_chartWidth = Options.Width > 0 ? Options.Width : 1000;
	}

	private int GetChartAreaWidth() => _chartWidth - Options.Padding.Left - Options.Padding.Right;
	private int GetChartAreaHeight() => _chartHeight - Options.Padding.Top - Options.Padding.Bottom;

	private double GetX(DateTime date)
	{
		var totalDays = (_maxDate - _minDate).TotalDays;
		if (totalDays == 0)
		{
			return Options.Padding.Left;
		}

		var daysSinceStart = (date - _minDate).TotalDays;
		return Options.Padding.Left + (daysSinceStart / totalDays * GetChartAreaWidth());
	}

	private double GetY(decimal value)
	{
		var range = _maxValue - _minValue;
		if (range == 0)
		{
			return Options.Padding.Top + GetChartAreaHeight() / 2.0;
		}

		var normalizedValue = (double)((value - _minValue) / range);
		return Options.Padding.Top + GetChartAreaHeight() * (1 - normalizedValue);
	}

	private string GenerateLinePath()
	{
		if (!Data.Any())
		{
			return "";
		}

		var orderedData = Data.OrderBy(d => d.Date).ToList();
		var pathSegments = new List<string>();

		for (int i = 0; i < orderedData.Count; i++)
		{
			var point = orderedData[i];
			var x = GetX(point.Date);
			var y = GetY(point.Value);

			if (i == 0)
			{
				pathSegments.Add($"M {x:F2} {y:F2}");
			}
			else
			{
				pathSegments.Add($"L {x:F2} {y:F2}");
			}
		}

		return string.Join(" ", pathSegments);
	}

	private string FormatValue(decimal value)
	{
		return Options.ValueFormat switch
		{
			ValueFormat.Integer => Math.Round(value).ToString("N0", _culture),
			ValueFormat.Decimal => value.ToString($"N{Options.DecimalPlaces}", _culture),
			ValueFormat.Percentage => $"{value.ToString($"N{Options.DecimalPlaces}", _culture)}%",
			ValueFormat.Currency => $"{value.ToString($"N{Options.DecimalPlaces}", _culture)} {Options.CurrencySymbol}",
			_ => value.ToString(_culture)
		};
	}

	private void ShowTooltip(ChartDataPoint point, double mouseX, double mouseY)
	{
		_tooltipContent = $"{point.Date.ToString("dd/MM/yyyy", _culture)}: {FormatValue(point.Value)}";
		_tooltipX = mouseX;
		_tooltipY = mouseY - 40;
		_showTooltip = true;
		StateHasChanged();
	}

	private void HideTooltip()
	{
		_showTooltip = false;
		StateHasChanged();
	}

	private List<(DateTime Date, string Label)> GetMonthMarkers()
	{
		var markers = new List<(DateTime, string)>();
		var current = new DateTime(_minDate.Year, _minDate.Month, 1);

		while (current <= _maxDate)
		{
			if (current >= _minDate)
			{
				markers.Add((current, current.ToString("MMM", _culture)));
			}
			current = current.AddMonths(1);
		}

		return markers;
	}

	private List<(DateTime Start, DateTime End)> GetWeekendBands()
	{
		var bands = new List<(DateTime, DateTime)>();
		var current = _minDate;

		while (current <= _maxDate)
		{
			if (current.DayOfWeek == DayOfWeek.Saturday)
			{
				bands.Add((current, current.AddDays(1)));
			}
			current = current.AddDays(1);
		}

		return bands;
	}

	private List<DateTime> GetWeekSeparators()
	{
		var separators = new List<DateTime>();
		var current = _minDate;

		while (current.DayOfWeek != DayOfWeek.Monday && current <= _maxDate)
		{
			current = current.AddDays(1);
		}

		while (current <= _maxDate)
		{
			separators.Add(current);
			current = current.AddDays(7);
		}

		return separators;
	}

	private List<(decimal Value, string Label)> GetYAxisTicks()
	{
		var ticks = new List<(decimal, string)>();
		var range = _maxValue - _minValue;
		var tickCount = 5;
		var tickInterval = range / (tickCount - 1);

		for (int i = 0; i < tickCount; i++)
		{
			var value = _minValue + (tickInterval * i);
			ticks.Add((value, FormatValue(value)));
		}

		return ticks;
	}

	private List<(DateTime Date, string Label)> GetXAxisTicks()
	{
		var ticks = new List<(DateTime, string)>();
		var totalDays = (_maxDate - _minDate).TotalDays;

		if (totalDays <= 7)
		{
			var current = _minDate;
			while (current <= _maxDate)
			{
				ticks.Add((current, current.ToString("dd/MM", _culture)));
				current = current.AddDays(1);
			}
		}
		else if (totalDays <= 31)
		{
			var current = _minDate;
			var interval = (int)Math.Ceiling(totalDays / 10);
			while (current <= _maxDate)
			{
				ticks.Add((current, current.ToString("dd/MM", _culture)));
				current = current.AddDays(interval);
			}
		}
		else
		{
			var current = _minDate;
			while (current.DayOfWeek != DayOfWeek.Monday && current <= _maxDate)
			{
				current = current.AddDays(1);
			}

			while (current <= _maxDate)
			{
				ticks.Add((current, current.ToString("dd/MM", _culture)));
				current = current.AddDays(7);
			}
		}

		return ticks;
	}
}
