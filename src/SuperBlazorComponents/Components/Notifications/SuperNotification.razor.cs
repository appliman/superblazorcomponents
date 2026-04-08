using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.Notifications;

public partial class SuperNotification
{
	private static readonly TimeSpan ProgressTick = TimeSpan.FromMilliseconds(100);
	private PeriodicTimer? _timer;
	private DateTimeOffset _now = DateTimeOffset.UtcNow;

	[Parameter]
	public NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

	private string PositionCssClass => Position switch
	{
		NotificationPosition.TopLeft => "top-0 start-0",
		NotificationPosition.TopRight => "top-0 end-0",
		NotificationPosition.BottomLeft => "bottom-0 start-0",
		_ => "bottom-0 end-0"
	};

	protected override void OnInitialized()
	{
		NotificationService.OnChange += OnChangeAsync;
		_timer = new PeriodicTimer(ProgressTick);
		_ = RunTimerAsync();
	}

	private async Task RunTimerAsync()
	{
		if (_timer is null)
		{
			return;
		}

		try
		{
			while (await _timer.WaitForNextTickAsync())
			{
				if (!NotificationService.Notifications.Any(ShouldShowProgress))
				{
					continue;
				}

				_now = DateTimeOffset.UtcNow;
				await InvokeAsync(StateHasChanged);
			}
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private Task OnChangeAsync()
	{
		return InvokeAsync(StateHasChanged);
	}

	private async Task CloseAsync(NotificationMessage notification)
	{
		_starts.Remove(notification);
		await NotificationService.Remove(notification);
	}

	private async Task OnToastClickAsync(NotificationMessage notification)
	{
		if (notification.Click is not null)
		{
			notification.Click.Invoke(notification);
		}

		if (notification.CloseOnClick)
		{
			_starts.Remove(notification);
			await NotificationService.Remove(notification);
		}
	}

	private static string GetDurationText(double? duration)
	{
		if (duration is null || duration <= 0)
		{
			return "";
		}

		return $"{Math.Round(duration.Value / 1000)}s";
	}

	private static string GetIcon(NotificationSeverity severity)
	{
		return severity switch
		{
			NotificationSeverity.Success => "fa-solid fa-circle-check",
			NotificationSeverity.Warning => "fa-solid fa-triangle-exclamation",
			NotificationSeverity.Error => "fa-solid fa-circle-xmark",
			_ => "fa-solid fa-circle-info"
		};
	}

	private static string GetIconCssClass(NotificationSeverity severity)
	{
		return severity switch
		{
			NotificationSeverity.Success => "text-success",
			NotificationSeverity.Warning => "text-warning",
			NotificationSeverity.Error => "text-danger",
			_ => "text-info"
		};
	}

	public void Dispose()
	{
		NotificationService.OnChange -= OnChangeAsync;
		_timer?.Dispose();
	}

	private bool ShouldShowProgress(NotificationMessage n)
	{
		return n.Duration is > 0 && (n.ShowProgress || n.Duration is not null);
	}

	private string GetProgressPercent(NotificationMessage n)
	{
		if (n.Duration is null || n.Duration <= 0)
		{
			return "0%";
		}

		var start = n.CreatedAt ?? GetOrCreateStart(n);
		var elapsedMs = (_now - start).TotalMilliseconds;
		var remainingRatio = 1.0 - (elapsedMs / n.Duration.Value);
		remainingRatio = Math.Clamp(remainingRatio, 0, 1);

		return $"{remainingRatio * 100:0.##}%";
	}

	private DateTimeOffset GetOrCreateStart(NotificationMessage n)
	{
		if (_starts.TryGetValue(n, out var start))
		{
			return start;
		}

		start = _now;
		_starts[n] = start;
		return start;
	}

	private readonly Dictionary<NotificationMessage, DateTimeOffset> _starts = new();
}