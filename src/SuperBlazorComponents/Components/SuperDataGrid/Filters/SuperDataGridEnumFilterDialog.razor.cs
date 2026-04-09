using SuperBlazorComponents.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

public partial class SuperDataGridEnumFilterDialog
{
    private readonly string _inputPrefix = $"enum_filter_{Guid.NewGuid():N}";
    private readonly string _selectAllId = $"enum_filter_all_{Guid.NewGuid():N}";
    private IReadOnlyList<SuperDataGridEnumFilterHelper.EnumOption> _options = [];
    private HashSet<string> _selectedValues = new(StringComparer.Ordinal);

    [Inject]
    private SuperDialogService DialogService { get; set; } = default!;

    [Inject]
    private IStringLocalizer Loc { get; set; } = default!;

    [Parameter]
    public string Label { get; set; } = "Valeur";

    [Parameter]
    public Type EnumType { get; set; } = typeof(Enum);

    [Parameter]
    public SuperDataGridEnumFilterSelection Value { get; set; } = SuperDataGridEnumFilterSelection.Empty;

    protected override void OnParametersSet()
    {
        _options = SuperDataGridEnumFilterHelper.GetOptions(EnumType);
        var normalizedSelection = SuperDataGridEnumFilterHelper.NormalizeSelection(Value, _options);
        _selectedValues = normalizedSelection.SelectedValues.ToHashSet(StringComparer.Ordinal);
    }

    private bool IsAllSelected => _options.Count > 0 && _selectedValues.Count == _options.Count;

    private Task CancelAsync()
    {
        return DialogService.Close();
    }

    private Task ApplyAsync()
    {
        var selection = SuperDataGridEnumFilterHelper.NormalizeSelection(
            new SuperDataGridEnumFilterSelection(_selectedValues.ToList()),
            _options);

        return DialogService.Close(selection);
    }

    private void ToggleAllSelection(ChangeEventArgs args)
    {
        if (args.Value is bool isChecked && isChecked)
        {
            _selectedValues = _options.Select(option => option.Value).ToHashSet(StringComparer.Ordinal);
            return;
        }

        _selectedValues.Clear();
    }

    private void ToggleOption(string value, ChangeEventArgs args)
    {
        if (args.Value is bool isChecked && isChecked)
        {
            _selectedValues.Add(value);
            return;
        }

        _selectedValues.Remove(value);
    }
}
