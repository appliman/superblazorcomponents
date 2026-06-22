using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperSplitter;

public partial class SuperSplitter
{
	private const string DefaultStoragePrefix = "SuperBlazorComponents.Components.SuperSplitter";

	[Inject]
	private NavigationManager NavigationManager { get; set; } = default!;

	[Inject]
	private ILogger<SuperSplitter> Logger { get; set; } = default!;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public SuperSplitterOrientation Orientation { get; set; } = SuperSplitterOrientation.Horizontal;

	[Parameter]
	public double FirstPaneSize { get; set; } = 50;

	[Parameter]
	public EventCallback<double> FirstPaneSizeChanged { get; set; }

	[Parameter]
	public double MinFirstPaneSize { get; set; } = 10;

	[Parameter]
	public double MaxFirstPaneSize { get; set; } = 90;

	[Parameter]
	public bool Collapsible { get; set; } = false;

	[Parameter]
	public bool EnableStatePersistence { get; set; } = true;

	[Parameter]
	public string? PersistenceKey { get; set; }

	private ElementReference splitterContainer;
	private bool isDragging;
	private DotNetObjectReference<SuperSplitter>? dotNetRef;
	private IJSObjectReference? jsModule;
	private bool _restoredFromStorage;
	private readonly List<SplitPane> _panes = new();

	internal void RegisterPane(SplitPane pane)
	{
		if (_panes.Count >= 2)
		{
			throw new InvalidOperationException($"{nameof(SuperSplitter)} ne peut contenir que 2 {nameof(SplitPane)}");
		}
		_panes.Add(pane);
	}

	internal SplitPane? FirstPane => _panes.Count > 0 ? _panes[0] : null;
	internal SplitPane? SecondPane => _panes.Count > 1 ? _panes[1] : null;

	private string OrientationClass => Orientation == SuperSplitterOrientation.Horizontal
		? "super-splitter-horizontal"
		: "super-splitter-vertical";

	private string GripIcon => Orientation == SuperSplitterOrientation.Horizontal
		? "fa-grip-lines"
		: "fa-grip-lines-vertical";

	private string FirstPaneStyle => Orientation == SuperSplitterOrientation.Horizontal
		? $"height: {FirstPaneSize}%;"
		: $"width: {FirstPaneSize}%;";

	private string SecondPaneStyle => Orientation == SuperSplitterOrientation.Horizontal
		? $"height: {100 - FirstPaneSize}%;"
		: $"width: {100 - FirstPaneSize}%;";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await TryRestoreSizeAsync();

			dotNetRef = DotNetObjectReference.Create(this);
			jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
				"import", "./_content/SuperBlazorComponents/Components/SuperSplitter/SuperSplitter.razor.js");

			if (jsModule != null)
			{
				await jsModule.InvokeVoidAsync("initSplitter", splitterContainer, dotNetRef, Orientation.ToString().ToLower());
			}

			NavigationManager.LocationChanged += OnLocationChanged;

			// Force un re-render pour s'assurer que les styles sont correctement appliqués
			StateHasChanged();
		}
	}

	private async void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
	{
		_restoredFromStorage = false;
		try
		{
			await TryRestoreSizeAsync();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to restore splitter size on navigation.");
		}
	}

	private void StartDragging(MouseEventArgs e)
	{
		isDragging = true;
	}

	private void StartDraggingTouch(TouchEventArgs e)
	{
		isDragging = true;
	}

	[JSInvokable]
	public async Task UpdateSize(double newSize)
	{
		var clampedSize = Math.Clamp(newSize, MinFirstPaneSize, MaxFirstPaneSize);
		if (Math.Abs(FirstPaneSize - clampedSize) > 0.1)
		{
			FirstPaneSize = clampedSize;
			await TryPersistSizeAsync(FirstPaneSize);
			await FirstPaneSizeChanged.InvokeAsync(FirstPaneSize);
			StateHasChanged();
		}
	}

	[JSInvokable]
	public void StopDragging()
	{
		isDragging = false;
		StateHasChanged();
	}

	private string GetStorageKey()
	{
		var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
		if (string.IsNullOrWhiteSpace(relativePath))
		{
			relativePath = "/";
		}

		var key = string.IsNullOrWhiteSpace(PersistenceKey)
			? $"{relativePath}:{nameof(SuperSplitter)}"
			: PersistenceKey;

		return $"{DefaultStoragePrefix}:{key}";
	}

	private async Task TryRestoreSizeAsync()
	{
		if (!EnableStatePersistence || _restoredFromStorage)
		{
			return;
		}

		try
		{
			var key = GetStorageKey();
			var value = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", key);
			if (!string.IsNullOrWhiteSpace(value)
				&& double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
			{
				FirstPaneSize = Math.Clamp(parsed, MinFirstPaneSize, MaxFirstPaneSize);
				await FirstPaneSizeChanged.InvokeAsync(FirstPaneSize);
			}
		}
		catch (JSException)
		{
			// ignore (e.g., localStorage not available)
		}
		finally
		{
			_restoredFromStorage = true;
		}
	}

	private async Task TryPersistSizeAsync(double size)
	{
		if (!EnableStatePersistence)
		{
			return;
		}

		try
		{
			var key = GetStorageKey();
			await JSRuntime.InvokeVoidAsync("localStorage.setItem", key, size.ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture));
		}
		catch (JSException)
		{
			// ignore (e.g., localStorage not available)
		}
	}

	public async ValueTask DisposeAsync()
	{
		NavigationManager.LocationChanged -= OnLocationChanged;

		if (jsModule != null)
		{
			try
			{
				await jsModule.InvokeVoidAsync("disposeSplitter", splitterContainer);
				await jsModule.DisposeAsync();
			}
			catch (JSDisconnectedException)
			{
				// Circuit is disconnected, ignore
			}
		}
		dotNetRef?.Dispose();
	}
}
