using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Components.Dialogs;

public class ConfirmOptions
{
	public ConfirmOptions()
	{
		CancelButtonText = "Cancel";
		OkButtonText = "OK";
	}

	public ConfirmOptions(IStringLocalizer localizer)
	{
		CancelButtonText = localizer["Dialog.Confirm.Cancel"];
		OkButtonText = localizer["Dialog.Confirm.Ok"];
	}

	public string CancelButtonText { get; set; }
	public string OkButtonText { get; set; }

	public static ConfirmOptions Default => new();

	public static ConfirmOptions CreateDefault(IStringLocalizer localizer) => new(localizer);
}
