namespace Blazor.Midi.GoKeys.Models;

public class Tone
{
    public string? Category { get; set; }
    public string? Num { get; set; }
    public string? Name { get; set; }
    public int MSB { get; set; }
    public int LSB { get; set; }
    public int PC { get; set; }
}