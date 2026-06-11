using System.Reflection;

namespace DemoWebSite;

public class MarkdownHelpService
{
	public async Task<string> GetMarkdownHelpText(string fileName)
	{
		var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		var filePath = Path.Combine(currentFolder, "wwwroot", "help", fileName);
		if (!System.IO.File.Exists(filePath))
		{
			return "documentation not found";
		}
		var content = await System.IO.File.ReadAllTextAsync(filePath);
		var markdigBuilder = new Markdig.MarkdownPipelineBuilder().Build();
		var html = Markdig.Markdown.ToHtml(content, markdigBuilder);
		return html;
	}
}