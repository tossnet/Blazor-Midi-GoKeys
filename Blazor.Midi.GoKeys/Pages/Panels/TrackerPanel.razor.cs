using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.JSInterop;

namespace Blazor.Midi.GoKeys.Pages;

public partial class TrackerPanel
{
    /// <summary />
    [Parameter]
    public int MidiKey { get; set; }

    public List<string>? Pattern { get; set; } = new();

    protected override void OnInitialized()
    {
        Pattern = new List<string>
        { 
            "000 -----"
        };
    }

    /// <summary />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (parameters.TryGetValue<int>(nameof(MidiKey), out var newMidiKey))
        {
            var note = GetNoteName(newMidiKey);
            Console.WriteLine($" Key: {newMidiKey} note: {note}");
            Pattern.Add(note);
        }
    }

    /// <summary />
    public string GetNoteName(int key)
    {
        // Les noms des notes dans l'ordre chromatique
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        // Calculer la position relative à partir de 36
        int relativePosition = key - 36;

        // Calculer l'octave (chaque octave contient 12 notes)
        int octave = (relativePosition / 12) + 1;

        // Calculer quelle note dans l'octave (0-11)
        int noteIndex = relativePosition % 12;

        return $"{noteNames[noteIndex]}{octave} -----";
    }
}
