using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Components;

public partial class SuperMenuItem : IDisposable
{
	[CascadingParameter]
	public SuperLayout.SuperLayout MainLayout { get; set; } = default!;

	[CascadingParameter]
	public SuperMenuItem? Parent { get; set; }

	[Parameter]
	public string? Icon { get; set; }

	[Parameter]
 public SuperIconStyle IconStyle { get; set; } = SuperIconStyle.Configuration;

	[Inject]
	private SuperComponentsConfiguration SuperComponentsConfiguration { get; set; } = default!;

	[Parameter]
	public string? Href { get; set; }

	[Parameter]
	public string? Text { get; set; }

	[Parameter]
	public string? Theme { get; set; }

	[Parameter]
	public string? BadgeText { get; set; }

	[Parameter]
	public string BadgeCssClass { get; set; } = "badge text-bg-success";

	[Parameter]
	public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object> CapturedAttributes { get; set; } = new();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public RenderFragment? Items { get; set; }

	[Parameter]
	public string? PolicyName { get; set; }

	private string css = "super-menu-item";
	private bool _expanded;
	private string? _lastUri;
	private string parentcss => Items is not null ? "parent" : "";
	private string themeCss => string.IsNullOrWhiteSpace(Theme) ? string.Empty : $"super-theme-{Theme}";

	private bool HasChildren => Items is not null;

	private bool IsCollapsed => MainLayout?.SidebarState is SuperLayout.SidebarState.Collapsed or SuperLayout.SidebarState.Hidden;

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
			return $"{stylePrefix} {Icon} me-1";
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

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			ApplyCss();
			NavigationManager.LocationChanged += OnLocationChanged;
			MainLayout.OnSidebarStateChanged += ResizeMenus;
		}
	}

	protected override void OnParametersSet()
	{
		ApplyCss();
	}

	public void ApplyCss()
	{
		css = "super-menu-item";
		if (CapturedAttributes.TryGetValue("class", out var @class) && @class is not null)
		{
			css = css + " " + @class;
		}

		if (!string.IsNullOrWhiteSpace(themeCss))
		{
			css = css + " " + themeCss;
		}

		if (Parent is not null)
		{
			css = css + " super-submenu";
		}

		if (HasChildren)
		{
			return;
		}

		var isActive = IsMatch(NavigationManager.Uri, Href, Match);
		if (isActive)
		{
			css = css + " active";
		}
	}

	private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
	{
		if (string.Equals(_lastUri, e.Location, StringComparison.Ordinal))
		{
			return;
		}

		_lastUri = e.Location;
		ApplyCss();
		StateHasChanged();
	}

	private static bool IsMatch(string currentAbsoluteUri, string? href, NavLinkMatch match)
	{
		if (href is null)
		{
			return false;
		}

		if (href.Length == 0)
		{
			href = "/";
		}

		var current = new Uri(currentAbsoluteUri, UriKind.Absolute);
		var target = new Uri(current, href);

		var currentPath = NormalizePath(current.AbsolutePath);
		var targetPath = NormalizePath(target.AbsolutePath);

		if (match == NavLinkMatch.All)
		{
			return string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase);
		}

		if (!currentPath.StartsWith(targetPath, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return currentPath.Length == targetPath.Length
			|| currentPath[targetPath.Length] == '/';
	}

	private static string NormalizePath(string path)
	{
		if (path.Length > 1 && path.EndsWith("/", StringComparison.Ordinal))
		{
			return path[..^1];
		}

		return path;
	}

	private void ResizeMenus(SuperLayout.SidebarState previousState, SuperLayout.SidebarState newState)
	{
		if (newState == SuperLayout.SidebarState.Collapsed)
		{
			_expanded = false;
		}
		StateHasChanged();
	}

	private void ToggleExpanded()
	{
		if (!HasChildren)
		{
			return;
		}

		_expanded = !_expanded;
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= OnLocationChanged;
		if (MainLayout is not null)
		{
			MainLayout.OnSidebarStateChanged -= ResizeMenus;
		}
	}
}