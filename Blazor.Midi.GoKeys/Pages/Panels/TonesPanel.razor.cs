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
    public string? Category { get; set; }

    /// <summary />
    [Parameter]
    public EventCallback<Tone> OnToneClickCallback { get; set; }

    private List<Tone> selectedTones = new();

    private string? _searchText;

    /// <summary />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Take the previous value before assignment
        var previousCategory = Category;

        await base.SetParametersAsync(parameters);

        if (parameters.TryGetValue(nameof(Category), out string? newCategory) && previousCategory != newCategory)
        {
            GetTones();
        }
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
        Category = e.Value?.ToString();
        GetTones();
    }

    /// <summary />
    private void GetTones()
    {
        if (!string.IsNullOrEmpty(Category))
        {
            selectedTones = ToneService.GetTonesByCategory(Category);
        }
    }

    /// <summary />
    private async Task OnToneClick(Tone tone)
    {
        await OnToneClickCallback.InvokeAsync(tone);
    }
}
