using DemoWebSite.Components;

using DemoWebSite.Mcp;

using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;

using SuperBlazorComponents;
using SuperBlazorComponents.Components.SuperDataGrid;

using static DemoWebSite.Components.Pages.SuperGridDemo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddCors(options =>
{
	options.AddPolicy("McpClients", policy =>
	{
		policy.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddSingleton<SuperComponentGuideCatalog>();
builder.Services.AddMcpServer()
	.WithHttpTransport(options =>
	{
		options.Stateless = true;
	})
	.WithToolsFromAssembly();

builder.Services.AddSuperComponents(options =>
{
	options.SuperDataGridFilterComponentList.Add(new SuperBlazorComponents.Components.SuperDataGrid.Filters.SuperDataGridFilterComponent
	{
		Name = "MyFilter",
		ComponentType = typeof(MyFilterComponent),
		PropertyType = typeof(Coordinates)
	});
	options.SuperDataGridSettingsList.Add(new SuperDataGridSettings
	{
		Name = "SimpleGrid",
		RowHeight = 42f,
		AllowColumnResize = false,
		AllowColumnReorder = false,
		AllowSorting = true,
		AllowFiltering = false,
		DisplaySelectionColumn = false,
		DisplayColumnVisibilityToggle = false,
		DisplayRowNumberColumn = true,
		DisplayRefreshButton = false,
		DisplayDefaultFooterTemplate = false
	});

	options.SuperDataGridSettingsList.Add(new SuperDataGridSettings
	{
		Name = "GridWithFooter",
		RowHeight = 42f,
		AllowColumnResize = true,
		AllowColumnReorder = true,
		AllowSorting = true,
		AllowFiltering = true,
		DisplaySelectionColumn = true,
		DisplayColumnVisibilityToggle = true,
		DisplayRowNumberColumn = true,
		DisplayRefreshButton = true,
		DisplayDefaultFooterTemplate = true
	});
});

System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseCors("McpClients");
app.UseAntiforgery();

app.MapGet("/mcp/health", () => Results.Ok("healthy"));
app.MapGet("/mcp", (SuperComponentGuideCatalog catalog) => Results.Ok(new
{
	name = "SuperBlazorComponents MCP Server",
	protocol = "Model Context Protocol over Streamable HTTP",
	endpoint = "/mcp",
	health = "/mcp/health",
	note = "Use POST /mcp from an MCP client. This GET response is only a browser-friendly discovery endpoint.",
	tools = catalog.List().Select(component => new
	{
		component.Key,
		component.Name,
		component.Summary
	})
}));
app.MapMcp("/mcp");

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
