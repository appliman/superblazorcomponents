using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.Notifications;

public sealed class NotificationMessage
{
	public DateTimeOffset? CreatedAt { get; set; }
	//
	// Summary:
	//     Gets or sets the summary.
	//
	// Value:
	//     The summary.
	public string Summary { get; set; } = null!;
	//
	// Summary:
	//     Gets or sets if progress should be shown during duration.
	//
	// Value:
	//     If true, then the progress will be shown during duration.
	public bool ShowProgress { get; set; }
	//
	// Summary:
	//     Gets or sets notification payload.
	//
	// Value:
	//     Used to store a custom payload that can be retreived later in the click event
	//     handler.
	public object Payload { get; set; } = default!;
	//
	// Summary:
	//     Gets or sets click on close action.
	//
	// Value:
	//     If true, then the notification will be closed when clicked on.
	public bool CloseOnClick { get; set; }
	//
	// Summary:
	//     Get or set the event for when the notification is closed
	public Action<NotificationMessage> Close { get; set; } = default!;
	//
	// Summary:
	//     Gets or sets the click event.
	//
	// Value:
	//     This event handler is called when the notification is clicked on.
	public Action<NotificationMessage> Click { get; set; } = default!;
	//
	// Summary:
	//     Gets or sets the style.
	//
	// Value:
	//     The style.
	public string? Style { get; set; }
	//
	// Summary:
	//     Gets or sets the detail.
	//
	// Value:
	//     The detail.
	public string? Detail { get; set; }
	//
	// Summary:
	//     Gets or sets the summary content.
	//
	// Value:
	//     The summary content.
	public RenderFragment SummaryContent { get; set; } = default!;
	//
	// Summary:
	//     Gets or sets the severity.
	//
	// Value:
	//     The severity.
	public NotificationSeverity Severity { get; set; }
	//
	// Summary:
	//     Gets or sets the duration.
	//
	// Value:
	//     The duration.
	public double? Duration { get; set; }
	//
	// Summary:
	//     Gets or sets the detail content.
	//
	// Value:
	//     The detail content.
	public RenderFragment DetailContent { get; set; } = default!;
}