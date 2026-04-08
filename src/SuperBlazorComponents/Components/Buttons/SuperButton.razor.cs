using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Components.Buttons;

public partial class SuperButton : IAsyncDisposable
{
	private IJSObjectReference? _module;
	private ElementReference _buttonRef;
	private bool _popoverInitialized;
	private string? _additionalCssClass;
	private Dictionary<string, object> _additionalAttributes = new();

	[Inject]
	private IJSRuntime JSRuntime { get; set; } = default!;

	[Inject]
	private SuperComponentsConfiguration SuperComponentsConfiguration { get; set; } = default!;

	[CascadingParameter]
	public SuperLayout.SuperLayout? MainLayout { get; set; }

	[Parameter]
	public string Text { get; set; } = null!;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public string? Icon { get; set; }

	/// <summary>
	/// Url of the image displayed in place of the icon when provided.
	/// </summary>
	[Parameter]
	public string? Image { get; set; }

	[Parameter]
	public SuperIconStyle IconStyle { get; set; } = SuperIconStyle.Configuration;

	[Parameter]
	public string? BadgeText { get; set; }

	[Parameter]
	public string BadgeCssClass { get; set; } = "badge text-bg-secondary";

	[Parameter]
	public bool Outline { get; set; }

	[Parameter]
	public SuperButtonSize Size { get; set; } = SuperButtonSize.Default;

	[Parameter]
	public SuperButtonStyle Style { get; set; } = SuperButtonStyle.Primary;

	[Parameter]
	public string? PopoverTitle { get; set; }

	[Parameter]
	public string? PopoverContent { get; set; }

	[Parameter]
	public string? PopoverPlacement { get; set; }

	[Parameter]
	public bool AllowCollapse { get; set; } = true;

	private bool HasPopover => !string.IsNullOrWhiteSpace(PopoverTitle) && !string.IsNullOrWhiteSpace(PopoverContent);

	private bool IsCollapsedOrHidden => AllowCollapse
		&& MainLayout?.SidebarState is SuperLayout.SidebarState.Collapsed or SuperLayout.SidebarState.Hidden;

	private bool HasLeadingVisual => !string.IsNullOrWhiteSpace(Image) || !string.IsNullOrWhiteSpace(Icon);

	private bool UseIconOnly => IsCollapsedOrHidden && HasLeadingVisual;

	private string IconCssClass
	{
		get
		{
			var stylePrefix = ResolvedIconStyle switch
			{
				SuperIconStyle.Regular => "fa-regular",
				SuperIconStyle.Brands => "fa-brands",
				SuperIconStyle.Duotone => "fa-duotone",
				_ => "fa-solid"
			};
			return $"{stylePrefix} {Icon}";
		}
	}

	private SuperIconStyle ResolvedIconStyle
	{
		get
		{
			var resolvedStyle = IconStyle == SuperIconStyle.Configuration
				? SuperComponentsConfiguration.DefaultSuperIconeStyle
				: IconStyle;

			return resolvedStyle == SuperIconStyle.Configuration
				? SuperIconStyle.Solid
				: resolvedStyle;
		}
	}

	private string CssClass
	{
		get
		{
			var variant = Style.ToString().ToLowerInvariant();
			var bootstrap = Outline ? $"btn btn-outline-{variant}" : $"btn btn-{variant}";

			var sizeClass = Size switch
			{
				SuperButtonSize.SuperSmall => "btn-sm super-button-super-small",
				SuperButtonSize.Small => "btn-sm",
				SuperButtonSize.Large => "btn-lg",
				_ => string.Empty
			};

			if (!string.IsNullOrWhiteSpace(sizeClass))
			{
				bootstrap = $"{bootstrap} {sizeClass}";
			}

			if (string.IsNullOrWhiteSpace(_additionalCssClass))
			{
				return bootstrap;
			}

			return $"{bootstrap} {_additionalCssClass}";
		}
	}

	protected override void OnParametersSet()
	{
		CapturedAttributes.TryGetValue("class", out var cls);
		_additionalCssClass = cls?.ToString();

		if (HasPopover)
		{
			CapturedAttributes["data-bs-toggle"] = "popover";
			CapturedAttributes["data-bs-title"] = PopoverTitle!;
			CapturedAttributes["data-bs-content"] = PopoverContent!;

			if (!CapturedAttributes.ContainsKey("data-bs-trigger"))
			{
				CapturedAttributes["data-bs-trigger"] = "focus";
			}

			if (!string.IsNullOrWhiteSpace(PopoverPlacement))
			{
				CapturedAttributes["data-bs-placement"] = PopoverPlacement;
			}

			if (!CapturedAttributes.ContainsKey("type"))
			{
				CapturedAttributes["type"] = "button";
			}
		}

		if (!UseIconOnly)
		{
			return;
		}

		if (!CapturedAttributes.ContainsKey("title"))
		{
			CapturedAttributes["title"] = Text;
		}

		if (!CapturedAttributes.ContainsKey("aria-label"))
		{
			CapturedAttributes["aria-label"] = Text;
		}

		_additionalAttributes.Clear();
		foreach (var (key, value) in CapturedAttributes)
		{
			if (string.Equals(key, "class", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			_additionalAttributes[key] = value;
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			if (MainLayout is not null)
			{
				MainLayout.OnSidebarStateChanged += OnSidebarStateChanged;
			}

			if (!HasPopover)
			{
				return;
			}

			_module = await JSRuntime.InvokeAsync<IJSObjectReference>(
				"import",
				"./_content/SuperBlazorComponents/Components/Buttons/SuperButton.razor.js");
		}

		if (!HasPopover)
		{
			return;
		}

		if (_module is null)
		{
			return;
		}

		await _module.InvokeVoidAsync("ensurePopover", _buttonRef);
		_popoverInitialized = true;
	}

	private void OnSidebarStateChanged(SuperLayout.SidebarState previousState, SuperLayout.SidebarState newState)
	{
		StateHasChanged();
	}

	public async ValueTask DisposeAsync()
	{
		if (MainLayout is not null)
		{
			MainLayout.OnSidebarStateChanged -= OnSidebarStateChanged;
		}

		if (_module is null)
		{
			return;
		}

		try
		{
			if (_popoverInitialized)
			{
				await _module.InvokeVoidAsync("disposePopover", _buttonRef);
			}

			await _module.DisposeAsync();
		}
		catch (JSDisconnectedException)
		{
		}
	}
}