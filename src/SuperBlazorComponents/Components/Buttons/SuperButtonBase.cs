using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SuperBlazorComponents.Components.Buttons;

public abstract class SuperButtonBase : ComponentBase
{
    [Parameter]
    public EventCallback<MouseEventArgs> Click { get; set; }

    [Parameter]
    public string? BusyText { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> CapturedAttributes { get; set; } = new();

    [Parameter]
    public bool IsBusy { get; set; } = false;

    private bool _isBusy;

	protected bool IsDisabled => _isBusy || IsBusy || Disabled || CapturedAttributes.ContainsKey("disabled");

    protected async Task OnClick(MouseEventArgs e)
    {
        if (!Click.HasDelegate)
        {
            return;
        }

        var useBusy = !string.IsNullOrWhiteSpace(BusyText);
        if (!useBusy)
        {
            await Click.InvokeAsync(e);
            return;
        }

        if (_isBusy || IsBusy)
        {
            return;
        }

		_isBusy = true;
        StateHasChanged();

        try
        {
            await Click.InvokeAsync(e);
        }
        finally
        {
			_isBusy = false;
            StateHasChanged();
        }
    }
}
