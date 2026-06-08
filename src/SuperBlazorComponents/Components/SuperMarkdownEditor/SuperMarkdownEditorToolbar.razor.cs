using Microsoft.AspNetCore.Components;
using SuperBlazorComponents.Components.Dialogs;
using SuperBlazorComponents.Services;

namespace SuperBlazorComponents.Components.SuperMarkdownEditor;

public partial class SuperMarkdownEditorToolbar : ComponentBase
{
    [Inject]
    private SuperDialogService DialogService { get; set; } = default!;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool RenderedView { get; set; } = true;

    [Parameter]
    public EventCallback<bool> RenderedViewChanged { get; set; }

    private async Task ToggleRenderedView()
    {
        if (Disabled)
        {
            return;
        }

        if (RenderedViewChanged.HasDelegate)
        {
            await RenderedViewChanged.InvokeAsync(!RenderedView);
        }
    }

    private Task OpenHelpDialog()
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        return DialogService.OpenAsync<SuperMarkdownEditorHelpDialog>(
            "Markdown Help",
            options: new DialogOptions
            {
                Width = "640px",
                Height = "72vh",
                ShowCloseButton = false,
                CloseOnBackdropClick = false
            });
    }
}
