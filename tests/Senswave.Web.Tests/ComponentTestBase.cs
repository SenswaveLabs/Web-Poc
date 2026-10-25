using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace Senswave.Web.Tests;

/// <summary>
/// bUnit context with MudBlazor services. JS interop is loose because MudBlazor calls JS
/// for focus, popovers and resize observers, which tests do not need.
/// </summary>
public abstract class ComponentTestBase : BunitContext, IAsyncLifetime
{
    private bool _disposedAsync;

    protected ComponentTestBase()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>MudMenu and MudSelect render their items into the popover provider.</summary>
    protected IRenderedComponent<MudPopoverProvider> Popovers { get; private set; } = default!;

    public Task InitializeAsync()
    {
        Popovers = Render<MudPopoverProvider>();
        return Task.CompletedTask;
    }

    // Some MudBlazor services only implement IAsyncDisposable, and xUnit 2 disposes test classes synchronously.
    async Task IAsyncLifetime.DisposeAsync()
    {
        _disposedAsync = true;
        await DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedAsync)
            base.Dispose(disposing);
    }
}
