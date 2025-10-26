using Blazor.Midi.GoKeys.Models;
using Blazor.Midi.GoKeys.Services;
using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Pages;

public partial class TonesPanel
{
    [Inject] private IToneService ToneService { get; set; } = default!;

    /// <summary />
    [Parameter]
    public List<string>? Categories { get; set; }

    /// <summary />
    [Parameter]
    public EventCallback<Tone> OnToneClickCallback { get; set; }

    private List<Tone> selectedTones = new();

    private string? _searchText;
    private Tone? _selectedtone;

    /// <summary />
    private void FilterTones(string? value)
    {
        _searchText = value;

        if (!string.IsNullOrWhiteSpace(value))
        {
            selectedTones = ToneService.SearchTones(value);
        }
    }

    /// <summary />
    private void OnCategoryChanged(ChangeEventArgs e)
    {
        var category = e.Value?.ToString();
        if (!string.IsNullOrEmpty(category))
        {
            selectedTones = ToneService.GetTonesByCategory(category);
        }
    }

    /// <summary />
    private async Task OnToneClick(Tone tone)
    {
        Console.WriteLine("panel OnToneClick");
        await OnToneClickCallback.InvokeAsync(tone);
    }
}
