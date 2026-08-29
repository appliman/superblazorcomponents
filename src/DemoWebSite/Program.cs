using DemoWebSite;
using DemoWebSite.Components;

using DemoWebSite.Mcp;

using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;

using SuperBlazorComponents;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.DataGridExporter;

using System.Text.RegularExpressions;

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
	options.Contextualization.AddAssembly<DemoWebSite.Components.ContextualizationDemo.DemoCustomer>();
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
builder.Services.AddSuperDataGridExporter(options =>
{
	options.TemporaryDirectory = Path.Combine(
		builder.Environment.ContentRootPath,
		"_temp",
		"data-grid-exports");
	options.FileLifetime = TimeSpan.FromHours(24);
	options.CleanupInterval = TimeSpan.FromDays(1);
});

System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

builder.Services.AddTransient<MarkdownHelpService>();

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
app.MapSuperDataGridExporter();

app.MapGet("/demo-source", (string route, IWebHostEnvironment environment) =>
{
	var pagesPath = Path.Combine(environment.ContentRootPath, "Components", "Pages");
	var normalizedRoute = NormalizeRoute(route);
	var pageFile = Directory.EnumerateFiles(pagesPath, "*.razor", SearchOption.TopDirectoryOnly)
		.FirstOrDefault(file => RazorPageRoutes(file).Contains(normalizedRoute, StringComparer.OrdinalIgnoreCase));

	if (pageFile is null)
	{
		return Results.NotFound();
	}

	var source = File.ReadAllText(pageFile);
	var cards = ExtractCardExamples(source)
		.Select((code, index) => new
		{
			index,
			code
		})
		.ToArray();

	return Results.Ok(new
	{
		route = normalizedRoute,
		file = Path.GetFileName(pageFile),
		cards
	});
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();

static string NormalizeRoute(string route)
{
	var value = route.Split('?', '#')[0].Trim();
	if (string.IsNullOrWhiteSpace(value))
	{
		return "/";
	}

	return value.StartsWith('/') ? value : "/" + value;
}

static IEnumerable<string> RazorPageRoutes(string file)
{
	var source = File.ReadAllText(file);
	foreach (Match match in Regex.Matches(source, "@page\\s+\"(?<route>[^\"]+)\""))
	{
		yield return NormalizeRoute(match.Groups["route"].Value);
	}
}

static IEnumerable<string> ExtractCardExamples(string source)
{
	const string marker = "<div class=\"card mt-3\"";
	var index = 0;

	while (index < source.Length)
	{
		var start = source.IndexOf(marker, index, StringComparison.Ordinal);
		if (start < 0)
		{
			yield break;
		}

		var end = FindMatchingDivEnd(source, start);
		if (end <= start)
		{
			yield break;
		}

		var card = source[start..end];
		var body = ExtractCardBody(card);
		if (!string.IsNullOrWhiteSpace(body))
		{
			yield return NormalizeSnippet(body);
		}

		index = end;
	}
}

static int FindMatchingDivEnd(string source, int start)
{
	var depth = 0;
	var index = start;

	while (index < source.Length)
	{
		var nextOpen = source.IndexOf("<div", index, StringComparison.OrdinalIgnoreCase);
		var nextClose = source.IndexOf("</div>", index, StringComparison.OrdinalIgnoreCase);

		if (nextClose < 0)
		{
			return -1;
		}

		if (nextOpen >= 0 && nextOpen < nextClose)
		{
			depth++;
			index = nextOpen + 4;
			continue;
		}

		depth--;
		index = nextClose + "</div>".Length;

		if (depth == 0)
		{
			return index;
		}
	}

	return -1;
}

static string ExtractCardBody(string card)
{
	var bodyStart = Regex.Match(card, "<div\\s+class=\"card-body[^\"]*\"[^>]*>", RegexOptions.IgnoreCase);
	if (!bodyStart.Success)
	{
		return string.Empty;
	}

	var start = bodyStart.Index + bodyStart.Length;
	var end = FindMatchingDivEnd(card, bodyStart.Index);
	if (end <= start)
	{
		return string.Empty;
	}

	var innerEnd = end - "</div>".Length;
	return card[start..innerEnd];
}

static string NormalizeSnippet(string snippet)
{
	var lines = snippet.Replace("\r\n", "\n").Split('\n');
	var nonEmpty = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
	var indent = nonEmpty.Length == 0
		? 0
		: nonEmpty.Min(line => line.TakeWhile(char.IsWhiteSpace).Count());

	return string.Join('\n', lines.Select(line => line.Length >= indent ? line[indent..].TrimEnd() : line.TrimEnd())).Trim();
}
