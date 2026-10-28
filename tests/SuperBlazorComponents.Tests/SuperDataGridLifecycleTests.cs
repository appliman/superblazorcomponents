using System.Reflection;

using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperDataGridLifecycleTests
{
    [TestMethod]
    public async Task DisposalDuringImportSkipsInitializationAndReleasesLateModule()
    {
        var runtime = new ControlledRuntime();
        var grid = CreateGrid(runtime);
        var rendering = grid.AfterRender(true);

        await grid.DisposeAsync();
        runtime.Import.SetResult(runtime.Module);
        await rendering.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual(0, runtime.Module.InitializeCount);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
        await grid.DisposeAsync();
        Assert.AreEqual(1, runtime.Module.DisposeCount);
    }

    [TestMethod]
    public async Task DisposalDuringInitializationKeepsReferenceAliveUntilCleanup()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        var grid = CreateGrid(runtime);
        var rendering = grid.AfterRender(true);
        var reference = runtime.Module.Reference!;

        await grid.DisposeAsync();
        Assert.AreSame(grid, reference.Value);
        Assert.AreEqual(0, runtime.Module.CleanupCount);
        Assert.AreEqual(0, runtime.Module.DisposeCount);

        runtime.Module.Initialize.SetResult();
        await rendering.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual(1, runtime.Module.CleanupCount);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = reference.Value);
    }

    [TestMethod]
    public async Task DisposalDuringSettingsLoadStopsInitialization()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetResult();
        var storage = new DelayedSettingsStorage();
        var grid = CreateGrid(runtime);
        SetProperty(grid, "GridId", "lifecycle");
        SetProperty(grid, "SettingsStorage", storage);
        var rendering = grid.AfterRender(true);
        Assert.AreEqual(1, storage.LoadCount);

        await grid.DisposeAsync();
        storage.Settings.SetResult(null);
        // Completing initialization must not attempt a render on this detached component.
        await rendering.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual(1, runtime.Module.CleanupCount);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.Module.Reference!.Value);
    }

    [TestMethod]
    public async Task DisposalDuringProfileLoadDoesNotPublishColumnState()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetResult();
        var storage = new DelayedProfilesStorage();
        var grid = CreateGrid(runtime);
        SetProperty(grid, "GridId", "lifecycle");
        SetProperty(grid, "SettingsStorage", storage);
        var notifications = 0;
        grid.ColumnStateChanged += (_, _) => notifications++;
        var rendering = grid.AfterRender(true);
        Assert.AreEqual(1, storage.LoadCount);

        await grid.DisposeAsync();
        storage.Profiles.SetResult(new SuperDataGridColumnProfiles());
        await rendering.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual(0, notifications);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
    }

    [TestMethod]
    public async Task SuccessfulInitializationKeepsReferenceUntilIdempotentDisposal()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetResult();
        var grid = CreateGrid(runtime);

        await grid.AfterRender(true);
        await grid.AfterRender(false);
        Assert.AreEqual(1, runtime.Module.InitializeCount);
        Assert.AreSame(grid, runtime.Module.Reference!.Value);
        Assert.AreEqual(0, runtime.Module.DisposeCount);

        await grid.DisposeAsync();
        await grid.DisposeAsync();
        await grid.AfterRender(true);

        Assert.AreEqual(1, runtime.Module.InitializeCount);
        Assert.AreEqual(1, runtime.Module.CleanupCount);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.Module.Reference.Value);
    }

    [TestMethod]
    public async Task FailedInitializationReleasesModuleAndReference()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetException(new JSException("Initialization failed"));
        var grid = CreateGrid(runtime);

        await Assert.ThrowsAsync<JSException>(() => grid.AfterRender(true));
        await grid.DisposeAsync();

        Assert.AreEqual(1, runtime.Module.CleanupCount);
        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.Module.Reference!.Value);
    }

    [TestMethod]
    public async Task CleanupFailureStillReleasesModuleAndReference()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetResult();
        var grid = CreateGrid(runtime);
        await grid.AfterRender(true);
        runtime.Module.CleanupException = new JSException("Cleanup failed");

        await Assert.ThrowsAsync<JSException>(() => grid.DisposeAsync().AsTask());
        await grid.DisposeAsync();

        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.Module.Reference!.Value);
    }

    [TestMethod]
    public async Task DisconnectedCleanupStillReleasesModuleAndReference()
    {
        var runtime = new ControlledRuntime();
        runtime.Import.SetResult(runtime.Module);
        runtime.Module.Initialize.SetResult();
        var grid = CreateGrid(runtime);
        await grid.AfterRender(true);
        runtime.Module.CleanupException = new JSDisconnectedException("Disconnected");

        await grid.DisposeAsync();

        Assert.AreEqual(1, runtime.Module.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.Module.Reference!.Value);
    }

    private static TestGrid CreateGrid(ControlledRuntime runtime)
    {
        var grid = new TestGrid();
        SetProperty(grid, "JSRuntime", runtime);
        return grid;
    }

    private static void SetProperty(TestGrid grid, string name, object value)
    {
        typeof(SuperDataGrid<string>).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .SetValue(grid, value);
    }

    private sealed class TestGrid : SuperDataGrid<string>
    {
        public Task AfterRender(bool firstRender) => base.OnAfterRenderAsync(firstRender);
    }

    private sealed class ControlledRuntime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new();
        public ControlledModule Module { get; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Assert.AreEqual("import", identifier);
            return (TValue)await Import.Task;
        }
    }

    private sealed class ControlledModule : IJSObjectReference
    {
        public TaskCompletionSource Initialize { get; } = new();
        public DotNetObjectReference<SuperDataGrid<string>>? Reference { get; private set; }
        public int InitializeCount { get; private set; }
        public int CleanupCount { get; private set; }
        public int DisposeCount { get; private set; }
        public Exception? CleanupException { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "initialize")
            {
                InitializeCount++;
                Reference = (DotNetObjectReference<SuperDataGrid<string>>)args![1]!;
                await Initialize.Task;
                // Match JS interop's requirement that the reference is still usable.
                _ = Reference.Value;
            }
            else if (identifier == "dispose")
            {
                CleanupCount++;
                if (CleanupException is not null)
                {
                    throw CleanupException;
                }
            }
            else
            {
                Assert.Fail($"Unexpected interop call: {identifier}");
            }

            return default!;
        }

        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class DelayedProfilesStorage : ISuperDataGridSettingsStorage, ISuperDataGridColumnProfilesStorage
    {
        public TaskCompletionSource<SuperDataGridColumnProfiles?> Profiles { get; } = new();
        public int LoadCount { get; private set; }

        public Task<SuperDataGridColumnProfiles?> GetProfilesAsync(string gridId, CancellationToken cancellationToken = default)
        {
            LoadCount++;
            return Profiles.Task;
        }

        public Task SaveProfilesAsync(string gridId, SuperDataGridColumnProfiles profiles, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<SuperDataGridColumnSettings>?>(null);
        public Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DelayedSettingsStorage : ISuperDataGridSettingsStorage
    {
        public TaskCompletionSource<IEnumerable<SuperDataGridColumnSettings>?> Settings { get; } = new();
        public int LoadCount { get; private set; }

        public Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default)
        {
            LoadCount++;
            return Settings.Task;
        }

        public Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
