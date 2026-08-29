# SuperDataGrid CSV and Excel exporter

The **SuperBlazorComponents.DataGridExporter** extension exports the complete
filtered and sorted view of a **SuperDataGrid**, including rows that are not
currently rendered by virtualization.

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

Only currently visible columns are exported, in their current order. The
exporter captures the grid filters and sort order once, then reads the complete
result from **ItemsProvider** in batches. Hierarchical grids export root items
only. The default batch size is 200 rows and can be changed with
**SuperDataGridExporterOptions.BatchSize**.

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
- The grid must use **ItemsProvider**; the currently rendered virtualized items
  alone are intentionally never treated as the complete dataset.
