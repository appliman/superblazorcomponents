# SuperDataGrid CSV and Excel exporter

The **SuperBlazorComponents.DataGridExporter** extension exports the rows that
are checked in a **SuperDataGrid**. A checked row is captured when generation
starts, so it remains exportable even if a later filter hides it.

## Installation

Reference both packages/projects:

~~~bash
dotnet add package SuperBlazorComponents
dotnet add package SuperBlazorComponents.DataGridExporter
~~~

Register the exporter and map its download endpoint in **Program.cs**:

~~~csharp
using SuperBlazorComponents.DataGridExporter;

builder.Services.AddSuperComponents();
builder.Services.AddSuperDataGridExporter(options =>
{
    options.TemporaryDirectory = Path.Combine(
        builder.Environment.ContentRootPath, "_temp", "grid-exports");
    options.FileLifetime = TimeSpan.FromHours(24);
    options.CleanupInterval = TimeSpan.FromDays(1);
});

var app = builder.Build();
app.MapSuperDataGridExporter();
~~~

The endpoint uses an unguessable 256-bit token and is anonymous by design.
Files expire after **FileLifetime**; cleanup runs at startup and then at
**CleanupInterval**.

## Usage

Keep a component reference to the grid and pass it to either export button:

~~~razor
@using SuperBlazorComponents.DataGridExporter.Components

<SuperDataGrid @ref="_grid"
               TItem="Product"
               ItemsProvider="@LoadProducts">
    <HeaderTemplate>
        <div class="d-flex align-items-center gap-2 w-100">
            <div class="ms-auto d-flex align-items-center gap-2">
                <SuperDataGridExcelExportButton TItem="Product"
                                                Grid="@_grid"
                                                DefaultFileName="products"
                                                IconOnly="true" />
                <SuperDataGridCsvExportButton TItem="Product"
                                              Grid="@_grid"
                                              DefaultFileName="products"
                                              IconOnly="true" />
            </div>
        </div>
    </HeaderTemplate>
    <DataGridColumn For="@(p => p.Name)" Title="Product" />
    <DataGridColumn For="@(p => p.Price)" Title="Price" FormatString="{0:F2}" />
</SuperDataGrid>

@code {
    private SuperDataGrid<Product>? _grid;
}
~~~

Set **IconOnly="true"** to hide the text while preserving it as the tooltip
and accessible label. The Bootstrap **ms-auto** wrapper aligns the export
actions to the right of the grid header. The grid keeps the custom header area
separated from its built-in actions. Omit **IconOnly** to display the icon and
text together.

Only currently visible columns are exported, in their current order. If rows
are selected individually, those exact objects are exported in selection order.
If **Tout sélectionner** is used, the exporter reads all rows matching the
captured filters and sort order from **ItemsProvider**, in batches of 200 by
default, and skips rows explicitly unchecked afterwards. The batch size can be
changed with **SuperDataGridExporterOptions.BatchSize**.

The dialog is still opened when nothing is checked, but immediately displays:
“Veuillez cocher au moins une ligne pour effectuer l’export.” Check a row and
choose **Réessayer** to continue; no file is created while the selection is
empty. Selection is frozen at the start of generation.

For hierarchical grids, every checked row is exported, including checked child
rows. Unchecked or unexpanded children are not invented by the exporter.
Virtualized providers should implement **IDataItem.KeyValue** with a stable,
unique key. The same key is used to apply exclusions and deduplicate rows when
the provider materializes a new object instance for each batch.

## Custom columns

Headers are resolved in this order:

1. **ExportHeader**
2. plain text found directly in **HeaderTemplate**
3. **Title**
4. **Property**

Values use **ExportValue** first, then **Property** or **For**. Configure both
overrides when a column is entirely template-driven:

~~~razor
<DataGridColumn Title="Status"
                ExportHeader="Current status"
                ExportValue="@GetExportedStatus">
    <Template>
        ...
    </Template>
</DataGridColumn>

@code {
    private static object GetExportedStatus(Product item)
        => item.IsActive ? "Active" : "Inactive";
}
~~~

Set **Exportable="false"** to exclude a visible column.

## Formats and limits

- CSV defaults to UTF-8 without BOM, comma delimiter and invariant culture.
  These defaults are configurable through **SuperDataGridExporterOptions**.
- CSV strings beginning with formula control characters are prefixed safely by
  default.
- Excel preserves native number, date and boolean cell types, freezes the
  header, reproduces the grid's left frozen columns, enables filtering and
  sizes columns to their content. Excel cannot freeze columns from the right.
- One worksheet supports at most 1,048,575 exported data rows because the
  header occupies the first Excel row.
- **ItemsProvider** is required for **Tout sélectionner** so the exporter can
  retrieve every filtered row; it never treats the currently rendered
  virtualized items alone as the complete dataset. Individually checked objects
  are exported from the immutable selection captured at the start.
