using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Components;

public partial class GKLCD : GKComponentBase
{
    /// <summary />
    [Parameter]
    public string? TextStates { get; set; }

    private string? GetClassDisabled()
    {
        if (Disabled)
        {
            return "lcd-off";
        }

        return null;
    }
}
