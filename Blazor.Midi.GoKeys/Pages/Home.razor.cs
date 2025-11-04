using Blazor.Midi.GoKeys.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Midi.GoKeys.Pages;

public partial class Home : IDisposable 
{
    [Inject] private IJSInProcessRuntime js { get; set; } = default!;
    [Inject] private IToneService ToneService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSInProcessObjectReference? JsModule { get; set; } = default!;

    private bool _isConnected = false;
    private string? _connectionStatus = "Disconnect";
    private string? _mainContent = @"V-001 Electro Pop<br /><br /><span class='lcd-tall'>Concert Piano</span><br /><span class='lcd-small'>PR--- No style</span>";
    private DotNetObjectReference<Home>? _dotNetRef;
    private List<string> _categories = new();
    private List<Tone> _selectedTones = new();
    public List<Note> _rawMidiKeys = new();
    private Tone? _selectedtone;
    private ComponentMetadata? _selectedComponent;
    private int _activePanelIndex;
    private Note _midiNote;

    /// <summary />
    private Dictionary<string, ComponentMetadata> Components => new()
    {

        [nameof(TonesPanel)] = new ComponentMetadata()
        {
            Type = typeof(TonesPanel),
            Parameters = {
                        [nameof(TonesPanel.Categories)] = _categories,
                        [nameof(TonesPanel.OnToneClickCallback)] = EventCallback.Factory.Create<Tone>(this, OnToneClick)
                     }
        },
        [nameof(TrackerPanel)] = new ComponentMetadata()
        {
            Type = typeof(TrackerPanel),
            Parameters = {
                        [nameof(TrackerPanel.MidiNote)] = _midiNote,
                        [nameof(TrackerPanel.RawMidiKeys)] = _rawMidiKeys,
                        [nameof(TrackerPanel.PlayCallback)] = EventCallback.Factory.Create<List<Note>>(this, OnClickPlay)
                     }
        },
        [nameof(SettingsPanel)] = new ComponentMetadata()
        {
            Type = typeof(SettingsPanel),
        }
    };
    
    protected override async Task OnInitializedAsync()
    {
        // Dont forget that the 'Web MIDI API' is asynchronous
        //JsModule ??= await js.InvokeAsync<IJSInProcessObjectReference>("import", "./js/midi.js");
        JsModule ??= await js.InvokeAsync<IJSInProcessObjectReference>("import", "./Pages/Home.razor.js");

        if (JsModule is null)
        {
            Console.WriteLine("JS module could not be loaded.");
            return;
        }

        // Register the callback
        _dotNetRef = DotNetObjectReference.Create(this);
        JsModule.InvokeVoid("setOnStateChangeCallback", _dotNetRef);

        await ToneService.InitializeAsync();
        _categories = ToneService.GetCategories();

        js.InvokeVoid("gkClick_init");
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
        _midiNote= new Note
        {
            Key = key,
            Velocity = velocity
        };

        if (_selectedComponent?.Type.Name == nameof(TrackerPanel))
        {
            _selectedComponent.Parameters[nameof(TrackerPanel.MidiNote)] = _midiNote;
        }
        
        InvokeAsync(StateHasChanged);
    }

    /// <summary />
    private void SelectPanel(ComponentMetadata component)
    {
        _selectedComponent = component;
        _activePanelIndex = GetActiveMenuIndex();
    }

    /// <summary>
    /// Get index for the LCD Menu according the ComponentMetadata
    /// </summary>
    private int GetActiveMenuIndex()
    {
        if (_selectedComponent == null)
            return -1;

        return _selectedComponent.Type.Name switch
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
        JsModule?.InvokeVoid("sendProgramChange", 4, tone.MSB, tone.LSB, tone.PC);
        UpdateMainContent();
    }

    private async Task OnClickPlay(List<Note> rawMidiKeys)
    {
        int tempo = 20;
        int noteDuration = 500;

        JsModule?.InvokeVoid("playSequence", rawMidiKeys, noteDuration, tempo);
    }

    /// <summary />
    private async Task PreselectClick(string category)
    {
        _selectedTones = ToneService.GetTonesByCategory(category);

        if (_selectedTones.Any())
        {
            await OnToneClick(_selectedTones.First());
        }
    }

    /// <summary />
    private async Task NoYetCode()
    {
        js.InvokeVoid("alert", "Not yet implement");
    }

    /// <summary />
    private void UpdateMainContent()
    {
        if (_selectedtone == null) return;

        _mainContent = $@"{_selectedtone.Num} {_selectedtone.Category}
                        <br /><br />
                        <span class='lcd-tall'>{_selectedtone.Name}</span>
                        <br />
                        <span class='lcd-small'>PR--- Style</span>";
    }

    /// <summary />
    public void Dispose()
    {
        JsModule?.Dispose();
    }
}
