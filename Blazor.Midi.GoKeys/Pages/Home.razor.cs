using Blazor.Midi.GoKeys.Models;
using Blazor.Midi.GoKeys.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.JSInterop;
using System.Xml.Linq;

namespace Blazor.Midi.GoKeys.Pages;

public partial class Home : IAsyncDisposable
{
    [Inject] private IJSInProcessRuntime js { get; set; } = default!;
    [Inject] private IToneService ToneService { get; set; } = default!;

    private bool _isConnected = false;
    private string? _connectionStatus = "Disconnect";
    private string? _mainContent = @"V-001 Electro Pop<br /><br /><span class='lcd-tall'>Concert Piano</span><br /><span class='lcd-small'>PR.108 Electro Pop2</span>";
    private DotNetObjectReference<Home>? _dotNetRef;
    private IJSObjectReference JsModule { get; set; } = default!;
    private List<string> categories = new();
    private List<Tone> selectedTones = new();
    private Tone? _selectedtone;
    private ComponentMetadata? selectedComponent;
    private int _activePanelIndex;
    private int _midiKey;

    /// <summary />
    private Dictionary<string, ComponentMetadata> GetComponents() => new()
    {
        [nameof(TonesPanel)] = new ComponentMetadata()
        {
            Type = typeof(TonesPanel),
            Name = "Tones Panel",
            Parameters = {
                        [nameof(TonesPanel.Categories)] = categories,
                        [nameof(TonesPanel.OnToneClickCallback)] = EventCallback.Factory.Create<Tone>(this, OnToneClick)
                     }
        },
        [nameof(TrackerPanel)] = new ComponentMetadata()
        {
            Type = typeof(TrackerPanel),
            Name = "Tracker Panel",
            Parameters = {
                        [nameof(TrackerPanel.MidiKey)] = _midiKey
                     }
        },
        [nameof(SettingsPanel)] = new ComponentMetadata()
        {
            Type = typeof(SettingsPanel),
            Name = "Settings Panel"
        }
    };

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

    /// <summary />
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

    /// <summary />
    [JSInvokable]
    public void OnMidiStateChanged(string state, string portName)
    {
        _connectionStatus = $"{portName}: {state}";
        _isConnected = state == "connected";
        InvokeAsync(StateHasChanged);
    }

    /// <summary />
    [JSInvokable]
    public void OnMidiKeyPress(int key, int velocity)
    {
        _midiKey = key;

        // idée de Copilot pour rafraichir le TrackerPanel :
        // Si TrackerPanel est sélectionné, créer une nouvelle instance
        if (selectedComponent?.Type == typeof(TrackerPanel))
        {
            selectedComponent = GetComponents()[nameof(TrackerPanel)];
        }

        InvokeAsync(StateHasChanged);
    }

    /// <summary />
    private void SelectPanel(ComponentMetadata component)
    {
        selectedComponent = component;
        _activePanelIndex = GetActiveMenuIndex();
    }

    /// <summary>
    /// Get index for the LCD Menu according the ComponentMetadata
    /// </summary>
    private int GetActiveMenuIndex()
    {
        if (selectedComponent == null)
            return -1;

        return selectedComponent.Type.Name switch
        {
            nameof(TonesPanel) => 0, 
            nameof(TrackerPanel) => 1,
            "ListPanel" => 2,
            nameof(SettingsPanel) => 3,
            _ => -1
        };
    }

    /// <summary />
    private async Task OnToneClick(Tone tone)
    {
        _selectedtone = tone;
        await JsModule.InvokeVoidAsync("sendProgramChange", 4, tone.MSB, tone.LSB, tone.PC);
        UpdateMainContent();
    }

    /// <summary />
    private async Task PreselectClick(string category)
    {
        selectedTones = ToneService.GetTonesByCategory(category);

        if (selectedTones.Any())
        {
            await OnToneClick(selectedTones.First());
        }
    }

    /// <summary />
    private async Task NoYetCode()
    {
        await js.InvokeVoidAsync("alert", "Not yet implement");
    }

    /// <summary />
    private void UpdateMainContent()
    {
        if (_selectedtone == null) return;

        _mainContent = $@"{_selectedtone.Num} {_selectedtone.Category}
                        <br /><br />
                        <span class='lcd-tall'>{_selectedtone.Name}</span>
                        <br />
                        <span class='lcd-small'>PR.108 Electro Pop2</span>";
    }

    /// <summary />
    public async ValueTask DisposeAsync()
    {
        if (JsModule is not null)
        {
            await JsModule.DisposeAsync();
        }
    }
}
