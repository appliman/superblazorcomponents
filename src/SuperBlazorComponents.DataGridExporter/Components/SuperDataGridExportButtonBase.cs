using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using SuperBlazorComponents.Components.Buttons;
using SuperBlazorComponents.Components.Dialogs;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Services;

namespace SuperBlazorComponents.DataGridExporter.Components;

public abstract class SuperDataGridExportButtonBase<TItem> : ComponentBase
{
    [Inject]
    private SuperDialogService DialogService { get; set; } = default!;

    [Parameter, EditorRequired]
    public SuperDataGrid<TItem>? Grid { get; set; }

    [CascadingParameter]
    private SuperDataGrid<TItem>? CascadedGrid { get; set; }

    [Parameter]
    public string? DefaultFileName { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public SuperButtonStyle Style { get; set; } = SuperButtonStyle.Primary;

    [Parameter]
    public SuperButtonSize Size { get; set; } = SuperButtonSize.Default;

    [Parameter]
    public bool Outline { get; set; }

    /// <summary>
    /// Displays only the format icon. The resolved text remains available as
    /// the button tooltip and accessible label.
    /// </summary>
    [Parameter]
    public bool IconOnly { get; set; }

    protected abstract SuperDataGridExportFormat Format { get; }
    protected abstract string DefaultText { get; }
    protected abstract string DialogTitle { get; }

    protected string ResolvedText => string.IsNullOrWhiteSpace(Text) ? DefaultText : Text;
    private SuperDataGrid<TItem>? EffectiveGrid => Grid ?? CascadedGrid;
    protected bool IsDisabled => Disabled || EffectiveGrid is null;

    protected async Task OpenDialogAsync(MouseEventArgs _)
    {
        var grid = EffectiveGrid;
        if (grid is null)
            return;

        var parameters = new Dictionary<string, object>
        {
            [nameof(SuperDataGridExportDialog<TItem>.Grid)] = grid,
            [nameof(SuperDataGridExportDialog<TItem>.Format)] = Format,
            [nameof(SuperDataGridExportDialog<TItem>.DefaultFileName)] = DefaultFileName ?? string.Empty
        };

        await DialogService.OpenAsync<SuperDataGridExportDialog<TItem>>(
            DialogTitle,
            parameters,
            new DialogOptions { Width = "560px" });
    }
}
