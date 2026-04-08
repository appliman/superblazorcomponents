namespace SuperBlazorComponents.Components.SuperDataGrid;

public interface IDataItem
{
    object KeyValue { get; }

    bool IsSelected { get; set; }

    int RowNumber { get; set; }
}
