using Blazor.Midi.GoKeys.Models;
using Blazor.Midi.GoKeys.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Midi.GoKeys.Pages;

public partial class Home : IAsyncDisposable
{
    [Inject] private IJSRuntime js { get; set; } = default!;
    [Inject] private IToneService ToneService { get; set; } = default!;

    private readonly HttpClient _http;
    private bool _isConnected = false;
    private string? _connectionStatus = "Disconnect";
    private string? _mainContent = @"V-001 Electro Pop<br /><br /><span class='lcd-tall'>Concert Piano</span><br /><span class='lcd-small'>PR.108 Electro Pop2</span>";
    private DotNetObjectReference<Home>? _dotNetRef;
    private IJSObjectReference JsModule { get; set; } = default!;
    private List<string> categories = new();
    private List<Tone> selectedTones = new();
    private Tone? _selectedtone; 
    private string? _searchText;

    protected override async Task OnInitializedAsync()
    {
        JsModule ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/midi.js");

        // Register the callback
        _dotNetRef = DotNetObjectReference.Create(this);
        await JsModule.InvokeVoidAsync("setOnStateChangeCallback", _dotNetRef);

        await ToneService.InitializeAsync();
        categories = ToneService.GetCategories();

        await js.InvokeVoidAsync("gkClick_init");
    }

    private async Task PowerClick()
    {
        if (_isConnected)
        {
            _isConnected = await JsModule.InvokeAsync<bool>("disconnectMIDI");
        }
        else
        {
            _isConnected = await JsModule.InvokeAsync<bool>("connectMIDI");
        }

         _connectionStatus = _isConnected ? "Donnected" : "Disconnect";
    }

    [JSInvokable]
    public void OnMidiStateChanged(string state, string portName)
    {
        _connectionStatus = $"{portName}: {state}";
        _isConnected = state == "connected";
        InvokeAsync(StateHasChanged);
    }

    private void OnCategoryChanged(ChangeEventArgs e)
    {
        var category = e.Value?.ToString();
        if (!string.IsNullOrEmpty(category))
        {
            selectedTones = ToneService.GetTonesByCategory(category);
        }
    }

    private async Task OnToneClick(Tone tone)
    {
        _selectedtone = tone;
        await JsModule.InvokeVoidAsync("sendProgramChange", 4, tone.MSB, tone.LSB, tone.PC);
        UpdateMainContent();
    }

    private async Task PreselectClick(string category)
    {
        selectedTones = ToneService.GetTonesByCategory(category);

        if (selectedTones.Any())
        {
            await OnToneClick(selectedTones.First());
        }
    }
    
    private async Task NoYetCode()
    {
        await js.InvokeVoidAsync("alert", "Not yet implement");
    }

    
    private void FilterTones(string? value)
    {
        _searchText = value;

        if (!string.IsNullOrWhiteSpace(value))
        {
            selectedTones = ToneService.SearchTones(value);
        }
    }


    private void UpdateMainContent()
    {
        if (_selectedtone == null) return;

        _mainContent = $@"{_selectedtone.Num} {_selectedtone.Category}
                        <br /><br />
                        <span class='lcd-tall'>{_selectedtone.Name}</span>
                        <br />
                        <span class='lcd-small'>PR.108 Electro Pop2</span>";
    }

    public async ValueTask DisposeAsync()
    {
        if (JsModule is not null)
        {
            await JsModule.DisposeAsync();
        }
    }
}
