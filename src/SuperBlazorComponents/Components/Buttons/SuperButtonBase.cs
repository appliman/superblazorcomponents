using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SuperBlazorComponents.Components.Buttons;

public abstract class SuperButtonBase : ComponentBase
{
    protected string? CapturedHtmlStyleAttribute { get; private set; }

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

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var hasHtmlStyle = false;
        CapturedHtmlStyleAttribute = null;

        foreach (var parameter in parameters)
        {
            if (string.Equals(parameter.Name, "style", StringComparison.OrdinalIgnoreCase)
                && (parameter.Value is string || parameter.Value is null))
            {
                hasHtmlStyle = true;
                CapturedHtmlStyleAttribute = parameter.Value as string;
                continue;
            }

            normalized[parameter.Name] = parameter.Value;
        }

        if (!hasHtmlStyle)
        {
            await base.SetParametersAsync(parameters);
            return;
        }

        await base.SetParametersAsync(ParameterView.FromDictionary(normalized));
    }

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
