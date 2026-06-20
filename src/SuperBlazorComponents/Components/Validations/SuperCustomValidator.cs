using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SuperBlazorComponents.Components.Validations;

public class SuperCustomValidator : ComponentBase, IDisposable
{
	private ValidationMessageStore? messageStore;

	[CascadingParameter] 
	public EditContext CurrentEditContext { get; set; } = default!;

	public bool HasError { get; protected set; }

	protected override void OnInitialized()
	{
		ArgumentNullException.ThrowIfNull(CurrentEditContext);
		messageStore = new(CurrentEditContext);
		CurrentEditContext.OnValidationRequested += HandleValidationRequested;
	}

	public void AddError(string fieldName, string errorMessage)
	{
		EnsureMessageStore().Clear();
		EnsureMessageStore().Add(CurrentEditContext.Field(fieldName), errorMessage);
		HasError = true;
		NotifyErrors();
	}

	public void DisplayError(string error)
	{
		EnsureMessageStore().Clear();
		EnsureMessageStore().Add(CurrentEditContext.Field("All"), error);
		HasError = true;
		NotifyErrors();
	}

	public void DisplayErrors(IReadOnlyDictionary<string, List<string>> errors)
	{
		EnsureMessageStore().Clear();
		foreach (var error in errors)
		{
			EnsureMessageStore().Add(CurrentEditContext.Field(error.Key), error.Value);
		}
		HasError = errors.Count > 0;
		NotifyErrors();
	}

	public void NotifyErrors() => CurrentEditContext.NotifyValidationStateChanged();

	public void Reset()
	{
		EnsureMessageStore().Clear();
		HasError = false;
		NotifyErrors();
		StateHasChanged();
	}

	protected ValidationMessageStore EnsureMessageStore() =>
		messageStore ?? throw new InvalidOperationException("The validator must be placed inside an EditForm.");

	private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
	{
		EnsureMessageStore().Clear();
		HasError = false;
	}

	public void Dispose()
	{
		if (CurrentEditContext is not null)
		{
			CurrentEditContext.OnValidationRequested -= HandleValidationRequested;
		}
		GC.SuppressFinalize(this);
	}
}
