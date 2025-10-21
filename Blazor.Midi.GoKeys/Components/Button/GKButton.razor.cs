//namespace Blazor.Midi.GoKeys.Components.Button;

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


    /// </summary>
    protected async Task OnClickHandlerAsync(MouseEventArgs e)
    {
        if (!Disabled && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

}
