using SuperBlazorComponents.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Components.SuperDataGrid.Filters;

public partial class SuperDataGridNumberFilterDialog
{
    private SuperDataGridFilterOperator _selectedOperator = SuperDataGridFilterOperator.Equals;
    private long? _value;
    private long? _valueTo;

    [Inject]
    private SuperDialogService DialogService { get; set; } = default!;

    [Inject]
    private IStringLocalizer Loc { get; set; } = default!;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public SuperDataGridNumberFilterSelection Value { get; set; } = SuperDataGridNumberFilterSelection.Empty;

    protected override void OnParametersSet()
    {
        Label ??= Loc["Filter.Number.Quantity"];
        var normalizedValue = NormalizeSelection(Value);
        _selectedOperator = normalizedValue.Operator ?? SuperDataGridFilterOperator.Equals;
        _value = normalizedValue.Value;
        _valueTo = normalizedValue.ValueTo;
    }

    private bool IsRangeOperator => SuperDataGridNumberFilterOperatorHelper.IsRangeOperator(_selectedOperator);

    private Task CancelAsync()
    {
        return DialogService.Close();
    }

    private Task RemoveAsync()
    {
        return DialogService.Close(SuperDataGridNumberFilterSelection.Empty);
    }

    private Task ApplyAsync()
    {
        if (!CanApply())
        {
            return Task.CompletedTask;
        }

        var selection = NormalizeSelection(new SuperDataGridNumberFilterSelection(_selectedOperator, _value, _valueTo));
        return DialogService.Close(selection);
    }

    private bool CanApply()
    {
        return IsRangeOperator
            ? _value is not null && _valueTo is not null
            : _value is not null;
    }

    private static SuperDataGridNumberFilterSelection NormalizeSelection(SuperDataGridNumberFilterSelection? selection)
    {
        if (selection?.Operator is null)
        {
            return SuperDataGridNumberFilterSelection.Empty;
        }

        if (!SuperDataGridNumberFilterOperatorHelper.IsRangeOperator(selection.Operator.Value))
        {
            return new SuperDataGridNumberFilterSelection(selection.Operator, selection.Value, null);
        }

        var fromValue = selection.Value;
        var toValue = selection.ValueTo;

        if (fromValue is not null && toValue is not null && fromValue > toValue)
        {
            (fromValue, toValue) = (toValue, fromValue);
        }

        return new SuperDataGridNumberFilterSelection(selection.Operator, fromValue, toValue);
    }
}
