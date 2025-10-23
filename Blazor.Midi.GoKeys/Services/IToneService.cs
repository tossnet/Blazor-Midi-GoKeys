using Blazor.Midi.GoKeys.Models;

namespace Blazor.Midi.GoKeys.Services;

public interface IToneService
{
    Task InitializeAsync();
    bool IsInitialized { get; }
    List<string> GetCategories();
    List<Tone> GetTonesByCategory(string category);
    List<Tone> SearchTones(string searchTerm);
    Tone? GetToneByNum(string num);
}
