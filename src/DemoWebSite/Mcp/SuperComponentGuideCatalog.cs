namespace DemoWebSite.Mcp;

public sealed class SuperComponentGuideCatalog
{
	private readonly IReadOnlyDictionary<string, SuperComponentGuide> _guides;

	public SuperComponentGuideCatalog()
	{
		_guides = CreateGuides()
			.ToDictionary(guide => guide.Key, StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyCollection<SuperComponentGuide> List() => _guides.Values
		.OrderBy(guide => guide.Name, StringComparer.OrdinalIgnoreCase)
		.ToList();

	public SuperComponentGuide? Find(string component)
	{
		if (string.IsNullOrWhiteSpace(component))
		{
			return null;
		}

		var normalized = Normalize(component);
		return _guides.TryGetValue(normalized, out var guide)
			? guide
			: _guides.Values.FirstOrDefault(guide => guide.Aliases.Any(alias => Normalize(alias) == normalized));
	}

	public string RenderIndex()
	{
		var rows = List().Select(guide =>
			$"- {guide.Name} (`{guide.Key}`): {guide.Summary}");

		return string.Join(Environment.NewLine, rows);
	}

	public string RenderGuide(string component)
	{
		var guide = Find(component);
		return guide is null
			? $"No SuperComponents guide found for '{component}'. Available keys: {string.Join(", ", List().Select(item => item.Key))}."
			: guide.ToMarkdown();
	}

	private static string Normalize(string value)
	{
		return new string(value
			.Where(char.IsLetterOrDigit)
			.Select(char.ToLowerInvariant)
			.ToArray());
	}

	private static IEnumerable<SuperComponentGuide> CreateGuides()
	{
		yield return new(
			"super-data-grid",
			"SuperDataGrid",
			"Virtualized data grid with sorting, filtering, frozen columns, fixed row height with overflow preview, row actions, selection, settings persistence, and row editing.",
			["SuperDataGrid", "DataGrid", "Grid"],
			["SuperBlazorComponents.Components.SuperDataGrid"],
			["/supergrid-demo", "/supergrid-simple-demo", "/supergrid-fixed-row-height-demo", "/todo-supergrid-demo"],
			"Call builder.Services.AddSuperComponents(...) in Program.cs. Use <SuperDataGrid TItem=\"MyItem\" ItemsProvider=\"LoadItemsAsync\"> with one <DataGridColumn> per property. Implement GridItemsProviderResult<TItem> and give rows a stable KeyValue when they implement IDataItem. Keep FixedRowHeight enabled for virtualized grids when row content may be taller than RowHeight; overflowing cells scroll internally and show a hover preview.",
			"""
			<SuperDataGrid TItem="Customer" ItemsProvider="LoadCustomersAsync" Height="520px" RowHeight="44" FixedRowHeight="true" AllowFiltering="true" AllowSorting="true">
			    <ChildContent>
			        <DataGridColumn For="@(item => item.Name)" Title="Name" Width="220px" />
			        <DataGridColumn For="@(item => item.City)" Title="City" Width="180px" />
			    </ChildContent>
			</SuperDataGrid>
			""");

		yield return new(
			"super-buttons",
			"SuperButtons",
			"Button family for actions, links, split menus, confirmation flows, toggle buttons, groups, icons, busy state, and Bootstrap styling.",
			["SuperButton", "SuperLinkButton", "SuperSplitButton", "SuperConfirmationButton", "SuperToggleButton"],
			["SuperBlazorComponents.Components.Buttons"],
			["/superbutton-demo"],
			"Use SuperButton for commands, SuperLinkButton for navigation, SuperConfirmationButton for destructive actions, and SuperSplitButton when a primary action has secondary choices. Prefer Font Awesome icon names in the Icon parameter.",
			"""
			<SuperButton Text="Save" Icon="fa-floppy-disk" Style="SuperButtonStyle.Primary" Click="SaveAsync" />
			<SuperConfirmationButton Text="Delete" Icon="fa-trash" Style="SuperButtonStyle.Danger" Click="DeleteAsync" />
			""");

		yield return new(
			"super-breadcrumb",
			"SuperBreadCrumb",
			"Breadcrumb navigation with regular items, active items, icons, and a back item.",
			["SuperBreadCrumb", "SuperBreadCrumbItem", "SuperBackBreadcrumbItem"],
			["SuperBlazorComponents.Components.BreadCrumbs"],
			["/superbreadcrumb-demo"],
			"Place <SuperBreadCrumb> near the top of a page. Add <SuperBreadCrumbItem> children and mark the current item with IsActive=\"true\".",
			"""
			<SuperBreadCrumb>
			    <SuperBreadCrumbItem Text="Home" Href="/" Icon="fa-house" />
			    <SuperBreadCrumbItem Text="Customers" IsActive="true" />
			</SuperBreadCrumb>
			""");

		yield return new(
			"super-date-range-picker",
			"SuperDateRangePicker",
			"Date range picker and dialog with presets, two-month calendar, week selection, bounds, and formatting.",
			["SuperDateRangePicker", "SuperDateRangeDialog", "DateRange"],
			["SuperBlazorComponents.Components.SuperDateRange"],
			["/superdaterangepicker-demo", "/superdaterangedialog-demo"],
			"Bind the selected range with @bind-Value. Configure presets and minimum or maximum dates when the business flow needs constrained periods.",
			"""
			<SuperDateRangePicker @bind-Value="_period" Label="Period" />

			@code {
			    private SuperDateRangeSelection? _period;
			}
			""");

		yield return new(
			"super-dialogs",
			"SuperDialogs",
			"Modal dialog host, confirmation dialog host, and injectable services for opening dialogs from application code.",
			["SuperDialog", "SuperConfirmDialog", "SuperDialogService", "SuperNotificationService"],
			["SuperBlazorComponents.Components.Dialogs", "SuperBlazorComponents.Services"],
			["/dialogservice-demo", "/confirmdialog-demo"],
			"Register AddSuperComponents, place <SuperDialog /> and <SuperConfirmDialog /> in the app layout, then inject SuperDialogService or SuperNotificationService where needed.",
			"""
			<SuperDialog />
			<SuperConfirmDialog />

			@inject SuperDialogService DialogService
			""");

		yield return new(
			"super-layout",
			"SuperLayout",
			"Application shell with header, sidebar, body, footer, chat panel, responsive sidebar state, and toolbar sections.",
			["SuperLayout", "SuperHeader", "SuperSidebar", "SuperBody", "SuperFooter", "SuperChat"],
			["SuperBlazorComponents.Components.SuperLayout"],
			["/superlayout-demo"],
			"Wrap routes in <SuperLayout>. Put navigation in SuperSidebar, page content in SuperBody, and optional actions in named SectionContent blocks consumed by the layout.",
			"""
			<SuperLayout>
			    <SuperSidebar>...</SuperSidebar>
			    <SuperBody>@Body</SuperBody>
			</SuperLayout>
			""");

		yield return new(
			"super-menu-item",
			"SuperMenuItem",
			"Sidebar and menu navigation item with icons, badges, active matching, nesting, and collapsed layout support.",
			["SuperMenuItem", "MenuItem"],
			["SuperBlazorComponents.Components.Menus"],
			["/superlayout-demo"],
			"Use inside SuperSidebar or any nav list. Set Href for navigation, Icon for the visual, and Match when a route prefix should stay active.",
			"""
			<SuperMenuItem Href="customers" Match="NavLinkMatch.Prefix" Icon="fa-users" Text="Customers" BadgeText="New" />
			""");

		yield return new(
			"super-notifications",
			"SuperNotifications",
			"Notification host and injectable service for success, warning, info, and error messages.",
			["SuperNotification", "SuperNotificationService", "Toast", "Notification"],
			["SuperBlazorComponents.Components.Notifications", "SuperBlazorComponents.Services"],
			["/notifications-demo"],
			"Place <SuperNotification /> in the layout once. Inject SuperNotificationService into pages or services and call it after user actions.",
			"""
			<SuperNotification />

			@inject SuperNotificationService Notifications
			""");

		yield return new(
			"super-tabs",
			"SuperTabs",
			"Tabbed workspace with horizontal or vertical positions, close buttons, badges, disabled tabs, and dynamic tab service support.",
			["SuperTabs", "TabItem", "Tabs"],
			["SuperBlazorComponents.Components.SuperTabs"],
			["/supertabs-demo", "/supertabs-horizontal-demo", "/supertabs-vertical-demo"],
			"Use declarative <TabItem> children for static tabs or SuperTabsService for dynamic tabs. Pick horizontal tabs for content pages and vertical tabs for settings-like surfaces.",
			"""
			<SuperTabs>
			    <TabItem Title="Details" Icon="fa-circle-info">Details content</TabItem>
			    <TabItem Title="History" Icon="fa-clock">History content</TabItem>
			</SuperTabs>
			""");

		yield return new(
			"super-forms",
			"SuperForms",
			"Form helpers including SuperDropDown and SuperSwitch with labels, help text, validation-friendly binding, and disabled states.",
			["SuperDropDown", "SuperSwitch", "Forms"],
			["SuperBlazorComponents.Components.Forms"],
			["/forms-demo"],
			"Use SuperSwitch for boolean settings and SuperDropDown<TItem,TValue> for select inputs where item text and value are projected from a data source.",
			"""
			<SuperSwitch Label="Enabled" @bind-Value="_enabled" />
			""");

		yield return new(
			"super-splitter",
			"SuperSplitter",
			"Resizable split panes with horizontal or vertical orientation and pane sizing constraints.",
			["SuperSplitter", "SplitPane", "Splitter"],
			["SuperBlazorComponents.Components.SuperSplitter"],
			["/supersplitter-demo", "/supersplitter-horizontal-demo", "/supersplitter-vertical-demo"],
			"Use SuperSplitter when two work areas need adjustable space. Put exactly two SplitPane children inside the splitter and choose Orientation for horizontal or vertical resizing.",
			"""
			<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal">
			    <SplitPane Size="35%">Left</SplitPane>
			    <SplitPane>Right</SplitPane>
			</SuperSplitter>
			""");

		yield return new(
			"super-tooltip",
			"SuperTooltip",
			"Tooltip component with configurable content, position, trigger behavior, and disabled state.",
			["SuperTooltip", "Tooltip"],
			["SuperBlazorComponents.Components.Tooltips"],
			["/supertooltip-demo"],
			"Wrap concise helper text around controls that need extra explanation. Keep tooltip content short and do not use it for required workflow information.",
			"""
			<SuperTooltip Content="Visible only to administrators">
			    <SuperButton Text="Archive" Icon="fa-box-archive" />
			</SuperTooltip>
			""");

		yield return new(
			"google-charts",
			"GoogleCharts",
			"Google chart wrappers for combo, pie, and time-series charts with strongly typed options and data rows.",
			["GoogleComboChart", "GooglePieChart", "TimeSeriesChart", "Charts"],
			["SuperBlazorComponents.Components.GoogleCharts"],
			["/googlecharts-demo", "/googlepiechart-demo"],
			"Add the Google Charts loader script to the host page, then use the chart component with typed data and options. Prefer chart components only where the page has real comparative data.",
			"""
			<GooglePieChart Data="_chartData" Options="_chartOptions" />
			""");

		yield return new(
			"theme-toggle",
			"ThemeToggle",
			"Theme switcher for light and dark modes.",
			["ThemeToggle", "Theme"],
			["SuperBlazorComponents.Components.Themes"],
			["/"],
			"Place ThemeToggle in the application header or settings surface. Keep it globally reachable when the app supports both light and dark modes.",
			"""
			<ThemeToggle />
			""");

		yield return new(
			"super-icons",
			"SuperIcon",
			"Font Awesome icon component with configurable style, size, title, and additional attributes.",
			["SuperIcon", "Icon"],
			["SuperBlazorComponents.Components.SuperIcons"],
			["/superbutton-demo"],
			"Use SuperIcon for standalone icons. Use the Icon parameter on SuperButton and SuperMenuItem for icons inside those controls.",
			"""
			<SuperIcon Icon="fa-check" Size="SuperIconSize.Large" Title="Completed" />
			""");

		yield return new(
			"super-validations",
			"SuperValidations",
			"Validation summary and validation message helpers for warning and error display.",
			["SuperValidationSummary", "SuperValidationMessage", "Validation"],
			["SuperBlazorComponents.Components.Validations"],
			["/forms-demo"],
			"Use inside EditForm surfaces when the app wants consistent warning and error presentation. Bind messages to the relevant model field.",
			"""
			<SuperValidationSummary />
			<SuperValidationMessage For="@(() => Model.Name)" />
			""");
	}
}

public sealed record SuperComponentGuide(
	string Key,
	string Name,
	string Summary,
	string[] Aliases,
	string[] Namespaces,
	string[] DemoRoutes,
	string Setup,
	string Example)
{
	public string ToMarkdown()
	{
		return $"""
		# {Name}

		{Summary}

		## Package setup

		```csharp
		builder.Services.AddSuperComponents();
		```

		## Namespaces

		{string.Join(Environment.NewLine, Namespaces.Select(item => $"- `{item}`"))}

		## Integration guidance

		{Setup}

		## Minimal example

		```razor
		{Example.Trim()}
		```

		## Demo routes

		{string.Join(Environment.NewLine, DemoRoutes.Select(route => $"- `{route}`"))}
		""";
	}
}
