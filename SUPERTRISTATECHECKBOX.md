# SuperTriStateCheckbox

Namespace:

```csharp
SuperBlazorComponents.Components.SuperTriStateCheckbox
```

`SuperTriStateCheckbox` is a Bootstrap-friendly checkbox component for nullable boolean values.

Each click cycles the value in this order:

```text
null -> true -> false -> null
```

## Example

```razor
@using SuperBlazorComponents.Components.SuperTriStateCheckbox

<SuperTriStateCheckbox @bind-Value="_value"
                       Label="Validated"
                       CssClass="border-primary"
                       HelpText="Click to cycle through null, true, and false." />

@code {
    private bool? _value;
}
```

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `bool?` | `null` | Current tri-state value |
| `ValueChanged` | `EventCallback<bool?>` | - | Raised when the value changes |
| `ValueExpression` | `Expression<Func<bool?>>?` | `null` | Supports Blazor form validation |
| `Label` | `string?` | `null` | Label displayed next to the checkbox |
| `HelpText` | `string?` | `null` | Help text displayed below the checkbox |
| `Id` | `string?` | generated | Input id |
| `Name` | `string?` | `null` | Input name |
| `Disabled` | `bool` | `false` | Disables the input |
| `CssClass` | `string` | `""` | Extra CSS classes for the input |
| `WrapperCssClass` | `string` | `""` | Extra CSS classes for the wrapper |
| `LabelCssClass` | `string` | `""` | Extra CSS classes for the label |
| `HelpTextCssClass` | `string` | `""` | Extra CSS classes for the help text |

The component uses Bootstrap `form-check` and `form-check-input` classes by default.
