using Microsoft.AspNetCore.Components;

using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Components.Buttons;

public partial class SuperLinkButton : IDisposable
{
	private string? _additionalCssClass;
	private readonly Dictionary<string, object> _additionalAttributes = [];

	[Inject]
	private SuperComponentsConfiguration SuperComponentsConfiguration { get; set; } = default!;

	[CascadingParameter]
	public SuperLayout.SuperLayout? MainLayout { get; set; }

	[Parameter]
	public string Text { get; set; } = null!;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public string? Href { get; set; }

	[Parameter]
	public string? Icon { get; set; }

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
	public bool Disabled { get; set; }

	[Parameter]
	public bool OpenInNewTab { get; set; }

	[Parameter]
	public bool AllowCollapse { get; set; } = true;

	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object> CapturedAttributes { get; set; } = [];

	private bool IsDisabled => Disabled || CapturedAttributes.ContainsKey("disabled");

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

			if (IsDisabled)
			{
				bootstrap = $"{bootstrap} disabled";
			}

			if (string.IsNullOrWhiteSpace(_additionalCssClass))
			{
				return bootstrap;
			}

			return $"{bootstrap} {_additionalCssClass}";
		}
	}

	private string? ResolvedHref => IsDisabled ? null : Href;

	private string? AriaDisabled => IsDisabled ? "true" : null;

	private int? TabIndex => IsDisabled ? -1 : null;

	protected override void OnInitialized()
	{
		if (MainLayout is null)
		{
			return;
		}

		MainLayout.OnSidebarStateChanged += OnSidebarStateChanged;
	}

	protected override void OnParametersSet()
	{
		CapturedAttributes.TryGetValue("class", out var cls);
		_additionalCssClass = cls?.ToString();

		if (UseIconOnly)
		{
			if (!CapturedAttributes.ContainsKey("title"))
			{
				CapturedAttributes["title"] = Text;
			}

			if (!CapturedAttributes.ContainsKey("aria-label"))
			{
				CapturedAttributes["aria-label"] = Text;
			}
		}

		_additionalAttributes.Clear();
		foreach (var (key, value) in CapturedAttributes)
		{
			if (string.Equals(key, "class", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(key, "href", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			_additionalAttributes[key] = value;
		}

		if (OpenInNewTab && !IsDisabled)
		{
			if (!_additionalAttributes.ContainsKey("target"))
			{
				_additionalAttributes["target"] = "_blank";
			}

			if (!_additionalAttributes.ContainsKey("rel"))
			{
				_additionalAttributes["rel"] = "noopener noreferrer";
			}
		}
	}

	private void OnSidebarStateChanged(SuperLayout.SidebarState previousState, SuperLayout.SidebarState newState)
	{
		StateHasChanged();
	}

	public void Dispose()
	{
		if (MainLayout is null)
		{
			return;
		}

		MainLayout.OnSidebarStateChanged -= OnSidebarStateChanged;
	}
}
