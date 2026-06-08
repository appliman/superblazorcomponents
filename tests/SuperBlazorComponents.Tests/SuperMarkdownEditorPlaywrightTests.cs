using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperMarkdownEditorPlaywrightTests
{
    private Process? _serverProcess;
    private readonly List<string> _serverMessages = new();

    [TestCleanup]
    public void Cleanup()
    {
        if (_serverProcess is { HasExited: false })
        {
            _serverProcess.Kill(entireProcessTree: true);
            _serverProcess.Dispose();
        }
    }

    [TestMethod]
    public async Task JavaScriptModule_SourceInput_RendersMarkdownAndNotifiesInBrowser()
    {
        var root = FindRepositoryRoot();
        var port = GetFreeTcpPort();
        var baseUrl = $"http://127.0.0.1:{port}";

        _serverProcess = StartDemoWebSite(root, baseUrl, _serverMessages);
        await WaitForServerAsync($"{baseUrl}/super-markdown-editor-demo");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(baseUrl);

        var result = await page.EvaluateAsync<MarkdownEditorJsResult>(
            @"async ({ moduleUrl }) => {
                const module = await import(moduleUrl);

                const rendered = document.createElement('div');
                rendered.contentEditable = 'true';
                const source = document.createElement('textarea');
                document.body.appendChild(rendered);
                document.body.appendChild(source);

                const notifications = [];
                const dotnetRef = {
                    invokeMethodAsync: (method, value) => {
                        notifications.push({ method, value });
                        return Promise.resolve();
                    }
                };

                const markdown = 'Commencez a ecrire ici.';
                module.initialize(rendered, source, dotnetRef, markdown);
                source.value = '**Commencez** a ecrire ici.';
                source.dispatchEvent(new Event('input', { bubbles: true }));

                return {
                    renderedHtml: rendered.innerHTML,
                    markdownValue: source.value,
                    contentChangedCount: notifications.filter(n => n.method === 'OnContentChanged').length
                };
            }",
            new
            {
                moduleUrl = $"{baseUrl}/_content/SuperBlazorComponents/Components/SuperMarkdownEditor/SuperMarkdownEditor.razor.js?v={Guid.NewGuid():N}"
            });

        Assert.IsTrue(
            result.RenderedHtml.Contains("<strong>Commencez</strong>", StringComparison.OrdinalIgnoreCase),
            $"Content changed notifications: {result.ContentChangedCount}; HTML: {result.RenderedHtml}; Markdown: {result.MarkdownValue}");
        Assert.AreEqual("**Commencez** a ecrire ici.", result.MarkdownValue);
        Assert.AreEqual(1, result.ContentChangedCount);
    }

    private static Process StartDemoWebSite(string root, string baseUrl, List<string> serverMessages)
    {
        var projectPath = Path.Combine(root, "src", "DemoWebSite", "DemoWebSite.csproj");
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList =
            {
                "run",
                "--project",
                projectPath,
                "--urls",
                baseUrl
            },
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start DemoWebSite process.");

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                serverMessages.Add(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                serverMessages.Add(args.Data);
            }
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task WaitForServerAsync(string url)
    {
        using var client = new HttpClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        while (!timeout.IsCancellationRequested)
        {
            try
            {
                using var response = await client.GetAsync(url, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch when (!timeout.IsCancellationRequested)
            {
            }

            await Task.Delay(250, timeout.Token);
        }

        throw new TimeoutException($"DemoWebSite did not respond at {url}.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var projectPath = Path.Combine(directory.FullName, "src", "DemoWebSite", "DemoWebSite.csproj");
            if (File.Exists(projectPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    public sealed class MarkdownEditorJsResult
    {
        public string RenderedHtml { get; set; } = string.Empty;

        public string MarkdownValue { get; set; } = string.Empty;

        public int ContentChangedCount { get; set; }

    }
}
