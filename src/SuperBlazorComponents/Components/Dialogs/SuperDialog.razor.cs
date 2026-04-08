namespace SuperBlazorComponents.Components.Dialogs;

public partial class SuperDialog : IDisposable
{
	private bool _isVisible;
	private Type? _componentType;
	private string _title = string.Empty;
	private Dictionary<string, object>? _parameters;
	private DialogOptions? _options;

	protected override void OnInitialized()
	{
		DialogService.OnOpenDialog += ShowDialog;
		DialogService.OnCloseDialog += CloseDialog;
	}

	private async Task ShowDialog(Type componentType, string title, Dictionary<string, object>? parameters, DialogOptions? options)
	{
		_componentType = componentType;
		_title = title;
		_parameters = parameters;
		_options = options;
		_isVisible = true;
		await InvokeAsync(StateHasChanged);
	}

	private async Task CloseDialog()
	{
		_isVisible = false;
		_componentType = null;
		_parameters = null;
		_options = null;
		await InvokeAsync(StateHasChanged);
	}

	private async Task OnBackdropClick()
	{
		if (_options?.CloseOnBackdropClick ?? true)
		{
			await DialogService.Close(null);
		}
	}

	private string GetSizeClass()
	{
		return _options?.Size switch
		{
			DialogSize.Small => "modal-sm",
			DialogSize.Large => "modal-lg",
			DialogSize.ExtraLarge => "modal-xl",
			_ => ""
		};
	}

	private string GetDialogStyle()
	{
		var styles = new List<string>();

		if (!string.IsNullOrEmpty(_options?.Width))
		{
			styles.Add($"max-width: {_options.Width}");
		}

		return string.Join("; ", styles);
	}

	private string GetBodyStyle()
	{
		var styles = new List<string>();

		if (!string.IsNullOrEmpty(_options?.Height))
		{
			styles.Add($"height: {_options.Height}");
			styles.Add("overflow-y: auto");
		}

		return string.Join("; ", styles);
	}

	public void Dispose()
	{
		DialogService.OnOpenDialog -= ShowDialog;
		DialogService.OnCloseDialog -= CloseDialog;
	}
}