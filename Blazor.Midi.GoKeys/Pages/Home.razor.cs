using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Midi.GoKeys.Pages;

public partial class Home : IAsyncDisposable
{
    [Inject] private IJSRuntime js { get; set; } = default!;

    private bool _connected = false;
    private string? _connectionStatus = "disconnect";

    private IJSObjectReference JsModule { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        JsModule ??= await js.InvokeAsync<IJSObjectReference>("import", "./Pages/Home.razor.js");
    }
    private async Task PowerClick()
    {
        _connected = await JsModule.InvokeAsync<bool>("connectMIDI");

        _connectionStatus = _connected ? "connected" : " disconnect";
    }

    public async ValueTask DisposeAsync()
    {
        if (JsModule is not null)
        {
            await JsModule.DisposeAsync();
        }
    }
}
