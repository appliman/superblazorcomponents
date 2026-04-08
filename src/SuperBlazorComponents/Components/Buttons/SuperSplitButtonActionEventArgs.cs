namespace SuperBlazorComponents.Components.Buttons;

public sealed class SuperSplitButtonActionEventArgs(string actionName) : EventArgs
{
    public string ActionName { get; } = actionName;
}
