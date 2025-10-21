using Microsoft.AspNetCore.Components;

namespace Blazor.Midi.GoKeys.Components;

public abstract  class GKComponentBase : ComponentBase
{
    /// </summary>
    [CascadingParameter(Name = "Disabled")]
    public virtual bool? ParentDisabled { get; set; }

    /// <summary>
    /// Disables the form control, ensuring it doesn't participate in form submission.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Optional CSS class names. If given, these will be included in the class attribute of the component.
    /// </summary>
    [Parameter]
    public virtual string? Class { get; set; } = null;

    /// <summary>
    /// Optional in-line styles. If given, these will be included in the style attribute of the component.
    /// </summary>
    [Parameter]
    public virtual string? Style { get; set; } = null;

    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? Attributes { get; set; }
}
