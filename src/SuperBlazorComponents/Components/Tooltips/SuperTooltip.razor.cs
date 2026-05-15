using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.Tooltips;

public partial class SuperTooltip : IAsyncDisposable
{
	[Inject] private ILogger<SuperTooltip> Logger { get; set; } = default!;

	private ElementReference _targetRef;
	private IJSObjectReference? _module;
	private bool _initialized;
	private string? _additionalCssClass;
	private Dictionary<string, object> _additionalAttributes = new();

	[Inject]
	private IJSRuntime JSRuntime { get; set; } = default!;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public string? Text { get; set; }

	[Parameter]
	public string? HtmlContent { get; set; }

	[Parameter]
	public string? Markdown { get; set; }

	[Parameter]
	public SuperTooltipPosition Position { get; set; } = SuperTooltipPosition.Top;

	[Parameter]
	public SuperTooltipTrigger Trigger { get; set; } = SuperTooltipTrigger.Hover;

	[Parameter]
	public int Delay { get; set; }

	[Parameter]
	public int Duration { get; set; }

	[Parameter]
	public bool CloseOnDocumentClick { get; set; }

	[Parameter]
	public string? TooltipCssClass { get; set; }

	[Parameter]
	public string? TooltipStyle { get; set; }

	/// <summary>Gets or sets the opacity level of the tooltip (0 = fully transparent, 100 = fully opaque). Defaults to null (no transparency).</summary>
	[Parameter]
	public int? Opacity { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	private string? EffectiveTooltipStyle
	{
		get
		{
			if (Opacity is null)
			{
				return TooltipStyle;
			}

			var clamped = Math.Clamp(Opacity.Value, 0, 100);
			var opacityPart = $"opacity: {clamped / 100.0:0.##};";

			return string.IsNullOrWhiteSpace(TooltipStyle)
				? opacityPart
				: $"{TooltipStyle.TrimEnd(';', ' ')};{opacityPart}";
		}
	}

	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

	private string WrapperCssClass => string.IsNullOrWhiteSpace(_additionalCssClass)
		? "super-tooltip-target"
		: $"super-tooltip-target {_additionalCssClass}";

	private string ContentHtml
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Markdown))
			{
				return RenderMarkdown(Markdown);
			}

			if (!string.IsNullOrWhiteSpace(HtmlContent))
			{
				return HtmlContent;
			}

			return WebUtility.HtmlEncode(Text ?? string.Empty);
		}
	}

	protected override void OnParametersSet()
	{
		_additionalAttributes = new Dictionary<string, object>();
		_additionalCssClass = null;

		if (AdditionalAttributes.TryGetValue("class", out var cls))
		{
			_additionalCssClass = cls?.ToString();
		}

		foreach (var (key, value) in AdditionalAttributes)
		{
			if (!string.Equals(key, "class", StringComparison.OrdinalIgnoreCase))
			{
				_additionalAttributes[key] = value;
			}
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			_module = await JSRuntime.InvokeAsync<IJSObjectReference>(
				"import",
				"./_content/SuperBlazorComponents/Components/Tooltips/SuperTooltip.razor.js");
		}

		if (_module is null)
		{
			return;
		}

		await _module.InvokeVoidAsync("configure", _targetRef, new
		{
			content = ContentHtml,
			placement = Position.ToString().ToLowerInvariant(),
			trigger = GetBootstrapTrigger(),
			delay = Math.Max(0, Delay),
			duration = Math.Max(0, Duration),
			closeOnDocumentClick = CloseOnDocumentClick,
			cssClass = TooltipCssClass,
			style = EffectiveTooltipStyle,
			disabled = Disabled
		});
		_initialized = true;
	}

	public async Task ShowAsync()
	{
		if (_module is not null)
		{
			await _module.InvokeVoidAsync("show", _targetRef);
		}
	}

	public async Task HideAsync()
	{
		if (_module is not null)
		{
			await _module.InvokeVoidAsync("hide", _targetRef);
		}
	}

	public async Task ToggleAsync()
	{
		if (_module is not null)
		{
			await _module.InvokeVoidAsync("toggle", _targetRef);
		}
	}

	private string GetBootstrapTrigger()
	{
		return Trigger switch
		{
			SuperTooltipTrigger.Click => "click",
			SuperTooltipTrigger.Focus => "focus",
			SuperTooltipTrigger.Manual => "manual",
			_ => "hover focus"
		};
	}

	private static string RenderMarkdown(string markdown)
	{
		var lines = markdown.Replace("\r\n", "\n").Split('\n');
		var html = new StringBuilder();
		var inUnorderedList = false;
		var inOrderedList = false;
		var inCodeBlock = false;
		var codeBuffer = new StringBuilder();

		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			var trimmed = line.Trim();

			if (trimmed.StartsWith("```", StringComparison.Ordinal))
			{
				if (inCodeBlock)
				{
					html.Append("<pre><code>");
					html.Append(WebUtility.HtmlEncode(codeBuffer.ToString().TrimEnd('\n')));
					html.Append("</code></pre>");
					codeBuffer.Clear();
					inCodeBlock = false;
				}
				else
				{
					CloseLists(html, ref inUnorderedList, ref inOrderedList);
					inCodeBlock = true;
				}

				continue;
			}

			if (inCodeBlock)
			{
				codeBuffer.AppendLine(line);
				continue;
			}

			if (string.IsNullOrWhiteSpace(trimmed))
			{
				CloseLists(html, ref inUnorderedList, ref inOrderedList);
				continue;
			}

			if (i + 1 < lines.Length && IsTableRow(trimmed) && IsTableSeparator(lines[i + 1].Trim()))
			{
				CloseLists(html, ref inUnorderedList, ref inOrderedList);
				i = RenderTable(lines, i, html);
				continue;
			}

			var headingLevel = GetHeadingLevel(trimmed);
			if (headingLevel > 0)
			{
				CloseLists(html, ref inUnorderedList, ref inOrderedList);
				var content = trimmed[(headingLevel + 1)..].Trim();
				html.Append($"<h{headingLevel}>");
				html.Append(RenderInlineMarkdown(content));
				html.Append($"</h{headingLevel}>");
				continue;
			}

			if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
			{
				if (!inUnorderedList)
				{
					CloseOrderedList(html, ref inOrderedList);
					html.Append("<ul>");
					inUnorderedList = true;
				}

				html.Append("<li>");
				html.Append(RenderInlineMarkdown(trimmed[2..].Trim()));
				html.Append("</li>");
				continue;
			}

			var orderedMatch = Regex.Match(trimmed, @"^\d+\.\s+(.+)$");
			if (orderedMatch.Success)
			{
				if (!inOrderedList)
				{
					CloseUnorderedList(html, ref inUnorderedList);
					html.Append("<ol>");
					inOrderedList = true;
				}

				html.Append("<li>");
				html.Append(RenderInlineMarkdown(orderedMatch.Groups[1].Value.Trim()));
				html.Append("</li>");
				continue;
			}

			CloseLists(html, ref inUnorderedList, ref inOrderedList);
			html.Append("<p>");
			html.Append(RenderInlineMarkdown(trimmed));
			html.Append("</p>");
		}

		if (inCodeBlock)
		{
			html.Append("<pre><code>");
			html.Append(WebUtility.HtmlEncode(codeBuffer.ToString().TrimEnd('\n')));
			html.Append("</code></pre>");
		}

		CloseLists(html, ref inUnorderedList, ref inOrderedList);
		return html.ToString();
	}

	private static int RenderTable(string[] lines, int headerIndex, StringBuilder html)
	{
		var headerCells = SplitTableCells(lines[headerIndex].Trim());
		var alignments = SplitTableCells(lines[headerIndex + 1].Trim())
			.Select(GetTableAlignment)
			.ToArray();

		html.Append("<table><thead><tr>");
		for (var columnIndex = 0; columnIndex < headerCells.Count; columnIndex++)
		{
			AppendTableCell(html, "th", headerCells[columnIndex], GetAlignment(alignments, columnIndex));
		}

		html.Append("</tr></thead><tbody>");

		var lastTableLineIndex = headerIndex + 1;
		for (var i = headerIndex + 2; i < lines.Length; i++)
		{
			var trimmed = lines[i].Trim();
			if (!IsTableRow(trimmed))
			{
				break;
			}

			var rowCells = SplitTableCells(trimmed);
			html.Append("<tr>");
			for (var columnIndex = 0; columnIndex < headerCells.Count; columnIndex++)
			{
				var cellContent = columnIndex < rowCells.Count ? rowCells[columnIndex] : string.Empty;
				AppendTableCell(html, "td", cellContent, GetAlignment(alignments, columnIndex));
			}

			html.Append("</tr>");
			lastTableLineIndex = i;
		}

		html.Append("</tbody></table>");
		return lastTableLineIndex;
	}

	private static void AppendTableCell(StringBuilder html, string tagName, string content, string? alignment)
	{
		html.Append('<');
		html.Append(tagName);
		if (!string.IsNullOrWhiteSpace(alignment))
		{
			html.Append(" style=\"text-align:");
			html.Append(alignment);
			html.Append("\"");
		}

		html.Append('>');
		html.Append(RenderInlineMarkdown(content.Trim()));
		html.Append("</");
		html.Append(tagName);
		html.Append('>');
	}

	private static string? GetAlignment(string?[] alignments, int columnIndex)
	{
		return columnIndex < alignments.Length ? alignments[columnIndex] : null;
	}

	private static string? GetTableAlignment(string separator)
	{
		var value = separator.Trim();
		var startsWithColon = value.StartsWith(':');
		var endsWithColon = value.EndsWith(':');

		return (startsWithColon, endsWithColon) switch
		{
			(true, true) => "center",
			(false, true) => "right",
			(true, false) => "left",
			_ => null
		};
	}

	private static bool IsTableRow(string line)
	{
		return line.Contains('|') && SplitTableCells(line).Count > 1;
	}

	private static bool IsTableSeparator(string line)
	{
		var cells = SplitTableCells(line);
		return cells.Count > 1 && cells.All(cell => Regex.IsMatch(cell.Trim(), @"^:?-{3,}:?$"));
	}

	private static List<string> SplitTableCells(string line)
	{
		var trimmed = line.Trim();
		if (trimmed.StartsWith('|'))
		{
			trimmed = trimmed[1..];
		}

		if (trimmed.EndsWith('|'))
		{
			trimmed = trimmed[..^1];
		}

		return trimmed.Split('|').Select(cell => cell.Trim()).ToList();
	}

	private static int GetHeadingLevel(string line)
	{
		var level = 0;
		while (level < line.Length && line[level] == '#')
		{
			level++;
		}

		return level is >= 1 and <= 6 && line.Length > level && line[level] == ' '
			? level
			: 0;
	}

	private static string RenderInlineMarkdown(string text)
	{
		var encoded = WebUtility.HtmlEncode(text);
		encoded = Regex.Replace(encoded, @"`([^`]+)`", "<code>$1</code>");
		encoded = Regex.Replace(encoded, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
		encoded = Regex.Replace(encoded, @"\*([^*]+)\*", "<em>$1</em>");
		encoded = Regex.Replace(encoded, @"\[([^\]]+)\]\((https?://[^)\s]+)\)", "<a href=\"$2\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");
		return encoded;
	}

	private static void CloseLists(StringBuilder html, ref bool inUnorderedList, ref bool inOrderedList)
	{
		CloseUnorderedList(html, ref inUnorderedList);
		CloseOrderedList(html, ref inOrderedList);
	}

	private static void CloseUnorderedList(StringBuilder html, ref bool inUnorderedList)
	{
		if (inUnorderedList)
		{
			html.Append("</ul>");
			inUnorderedList = false;
		}
	}

	private static void CloseOrderedList(StringBuilder html, ref bool inOrderedList)
	{
		if (inOrderedList)
		{
			html.Append("</ol>");
			inOrderedList = false;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_module is null)
		{
			return;
		}

		try
		{
			if (_initialized)
			{
				await _module.InvokeVoidAsync("dispose", _targetRef);
			}

			await _module.DisposeAsync();
		}
		catch (JSDisconnectedException ex)
		{
			Logger.LogDebug(ex, "JS disconnected while disposing tooltip JavaScript resources.");
		}
	}
}
