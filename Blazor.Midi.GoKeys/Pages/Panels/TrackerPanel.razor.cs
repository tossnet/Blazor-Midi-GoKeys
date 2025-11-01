using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Pages;

public partial class TrackerPanel
{
    /// <summary />
    [Parameter]
    public Note MidiNote { get; set; }

    /// <summary />
    [Parameter]
    public List<Note> RawMidiKeys { get; set; } = new();

    /// <summary />
    [Parameter]
    public EventCallback<List<Note>> PlayCallback { get; set; }

    public List<string>? Pattern { get; set; } = new();

    /// <summary />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        RawMidiKeys ??= new List<Note>();

        if (parameters.TryGetValue<Note>(nameof(MidiNote), out Note? newMidiNote) && newMidiNote != null)
        {

            RawMidiKeys?.Add(newMidiNote);

            var note = GetNoteName(newMidiNote.Key);
            Console.WriteLine($" Key: {newMidiNote.Key} {newMidiNote.Velocity} note: {note}");

            var velocity = GetVelocity(newMidiNote.Velocity);
            Pattern?.Add($"{note} --{velocity}");
        }
    }

    /// <summary />
    public string GetNoteName(int key)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        // Calculer l'octave (chaque octave contient 12 notes)
        int octave = (key / 12) + 1;

        // Calculer quelle note dans l'octave (0-11)
        int noteIndex = key % 12;

        if (noteNames[noteIndex].Length == 1)
        {
            return $"{noteNames[noteIndex]}-{octave}";
        }

        return $"{noteNames[noteIndex]}{octave}";
    }

    /// <summary />
    public string GetVelocity(int velocity)
    {
        // C is the code from ProTracker for volume note ^^
        return $"C{velocity:X2}";
    }

    private void OnClickNew()
    {
        RawMidiKeys?.Clear();
        Pattern?.Clear();
    }

    private async Task OnClickPlay()
    {
        await PlayCallback.InvokeAsync(RawMidiKeys);
    }
}
