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

    private string? _category { get; set; }
    private List<Tone> selectedTones = new();
    private string? _searchText;

    private void PreselectClick(string category)
    {
        _category = category;
        GetTones();
    }

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
        _category = e.Value?.ToString();
        GetTones();
    }

    /// <summary />
    private void GetTones()
    {
        if (!string.IsNullOrEmpty(_category))
        {
            selectedTones = ToneService.GetTonesByCategory(_category);
        }
    }

    /// <summary />
    private async Task OnToneClick(Tone tone)
    {
        await OnToneClickCallback.InvokeAsync(tone);
    }
}
