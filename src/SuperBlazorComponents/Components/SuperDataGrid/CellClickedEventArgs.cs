namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Event arguments for cell click events.
/// </summary>
/// <typeparam name="TItem">The type of data item.</typeparam>
public class CellClickedEventArgs<TItem>
{
    /// <summary>
    /// The item bound to the row containing the clicked cell.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    /// The property name of the clicked column.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// The value of the cell.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Creates a new instance of CellClickedEventArgs.
    /// </summary>
    public CellClickedEventArgs(TItem item, string propertyName, object? value)
    {
        Item = item;
        PropertyName = propertyName;
        Value = value;
    }
}
