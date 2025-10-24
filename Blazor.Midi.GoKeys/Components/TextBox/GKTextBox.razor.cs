using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Components;

public partial class GKTextBox : GKComponentBase
{
    /// <summary />
    [Parameter]
    public string? Placeholder { get; set; }
    /// <summary />
    [Parameter]
    public string? Value { get; set; }

    /// <summary />
    [Parameter] 
    public EventCallback<string> ValueChanged { get; set; }

    private async Task OnValueSet(string newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
    }
}
