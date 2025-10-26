using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Components;

public partial class GKLCD : GKComponentBase
{
    /// <summary />
    [Parameter]
    public string? TextStates { get; set; }

    /// <summary />
    [Parameter]
    public string? MainContent { get; set; }

    /// <summary>
    /// Index of the actif menu (0: TONES, 1: TRACKER, 2: LIST, 3: SETTING)
    /// </summary>
    [Parameter]
    public int ActiveMenuIndex { get; set; } = -1;

    private string? GetClassDisabled()
    {
        if (Disabled)
        {
            return "lcd-off";
        }

        return null;
    }

    private string? GetActiveClass(int menuIndex)
    {
        return ActiveMenuIndex == menuIndex ? "active" : null;
    }
}
