using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Blazor.Midi.GoKeys.Components;

public partial class GKButton : GKComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Command executed when the user clicks on the button.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary />
    [Parameter]
    public Appearance Appearance { get; set; } = Appearance.Round;

    /// <summary />
    [Parameter]
    public bool ShowIndicator { get; set; } = false;

    /// <summary />
    [Parameter]
    public bool IsIndicatorOn { get; set; } = false;

    /// <summary />
    [Parameter]
    public string? Caption { get; set; }

    /// <summary />
    [Parameter]
    public CaptionPosition CaptionPosition { get; set; } = CaptionPosition.Above;

    /// </summary>
    protected async Task OnClickHandlerAsync(MouseEventArgs e)
    {
        if (!Disabled && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    /// </summary>
    private string GetComponentClass()
    {
        return Appearance switch
        {
            Appearance.Round => "round",
            Appearance.Extended => "extended",
            _ => "round"
        };
    }

    private string GetClassPosition()
    {
        if (CaptionPosition == CaptionPosition.Above)
        {
            return "caption-above";
        }
        else if (CaptionPosition == CaptionPosition.Inside)
        {
            return "caption-inside";
        }

        return null;
    }
}
