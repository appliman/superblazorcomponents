# 🎨 SuperColorPicker

> A pair of interactive HSV color-picker components for Blazor — an inline picker and a compact dropdown variant — built with zero third-party dependencies, CSS-isolated scoped styles, and full Bootstrap 5 integration.

[← Back to README](README.md)

---

## 📑 Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Components](#components)
  - [SuperColorPicker](#supercolorpicker-1)
  - [SuperDropDownColorPicker](#superdropdowncolorpicker-1)
- [API Reference](#api-reference)
  - [SuperColorPicker parameters](#supercolorpicker-parameters)
  - [SuperDropDownColorPicker parameters](#superdropdowncolorpicker-parameters)
- [Usage Examples](#usage-examples)
  - [Basic binding](#basic-binding)
  - [With label](#with-label)
  - [Alpha channel](#alpha-channel)
  - [Dropdown variant](#dropdown-variant)
  - [Inside an EditForm](#inside-an-editform)
  - [Disabled / read-only](#disabled--read-only)
  - [Multiple pickers side by side](#multiple-pickers-side-by-side)
- [Value format](#value-format)
- [Input modes](#input-modes)
- [Keyboard navigation](#keyboard-navigation)
- [CSS customization](#css-customization)
- [Tips & Best Practices](#tips--best-practices)

---

## Overview

`SuperColorPicker` provides a full **HSV color selection** experience directly in Blazor — no canvas, no external JS libraries.

**Key features**

- 🎨 **HSV gradient area** — click or drag to pick saturation & value
- 🌈 **Hue slider** — full 360° spectrum
- 💧 **Optional alpha slider** — opacity with real-time checkerboard preview
- 🔢 **Two input modes** — HEX (`#RRGGBB`) and RGB(A) channel inputs, togglable at runtime
- 📦 **Dropdown variant** — compact trigger button that opens the picker in a floating popup
- 🔗 **EditForm integration** — `ValueExpression` + `EditContext` support for validation
- ♿ **Accessible** — semantic labels, `aria-haspopup`, `aria-expanded`, keyboard navigation
- 🎨 **CSS-isolated** — all styles scoped, no global CSS pollution
- 🖥️ **Pointer Events API** — smooth drag on mouse, touch, and stylus

---

## Getting Started

### Namespace

Both components share the same namespace:

```razor
@using SuperBlazorComponents.Components.SuperColorPicker
```

### Service registration

No specific service registration is required beyond the base:

```csharp
// Program.cs
builder.Services.AddSuperComponents();
```

### Minimal example

```razor
@using SuperBlazorComponents.Components.SuperColorPicker

<SuperColorPicker @bind-Value="_color" />

@code {
    private string? _color = "#3498DB";
}
```

---

## Components

### SuperColorPicker

An **inline** color picker displaying the full picker UI at all times.

```
┌──────────────────────────────────┐
│  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  ← HSV gradient area (160 px tall)
│  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │    drag to pick saturation & value
│  ░░░░░░░░░░░░ ○ ░░░░░░░░░░░░░░  │    ○ = current position cursor
│  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │
└──────────────────────────────────┘
  ●  [════════ hue bar ══════════]   ← ● = color preview swatch
     [═══ alpha bar (optional) ══]
  [ #3498DB      HEX ] [⇄]          ← text inputs + toggle button
```

Dimensions: **240 px wide**, height varies with `ShowAlpha`.

---

### SuperDropDownColorPicker

A **compact trigger button** that opens `SuperColorPicker` in a floating popup.

```
[ ● ▾ ]   ← trigger button: colored swatch + caret
    ↓ (click)
┌─────────────┐
│ SuperColor  │  ← floating popup (absolute positioned)
│   Picker    │
└─────────────┘
```

- Clicking anywhere **outside** the popup (via a transparent full-screen backdrop) closes it.
- `Escape` or `Tab` key on the trigger also closes it.
- A `Label` above the button is **clickable** to open the dropdown.

---

## API Reference

### SuperColorPicker parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string?` | `null` | Current color as HEX string — `#RRGGBB` or `#RRGGBBAA` |
| `ValueChanged` | `EventCallback<string?>` | — | Fires on every color change (drag, slider move, input change) |
| `ValueExpression` | `Expression<Func<string?>>?` | `null` | Required for EditForm validation integration |
| `Label` | `string?` | `null` | Optional label rendered above the picker |
| `ShowAlpha` | `bool` | `false` | Shows the alpha slider; value format becomes `#RRGGBBAA` |
| `Disabled` | `bool` | `false` | Disables all interactions (opacity 55 %, pointer events none) |
| `CssClass` | `string?` | `null` | Additional CSS class on the wrapper `<div>` |

> `EditContext` is cascaded automatically when inside an `EditForm`.

---

### SuperDropDownColorPicker parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string?` | `null` | Current color as HEX string — `#RRGGBB` or `#RRGGBBAA` |
| `ValueChanged` | `EventCallback<string?>` | — | Fires on every color change inside the picker |
| `ValueExpression` | `Expression<Func<string?>>?` | `null` | Required for EditForm validation integration |
| `Label` | `string?` | `null` | Optional label above the trigger button (clickable) |
| `ShowAlpha` | `bool` | `false` | Enables the alpha slider in the embedded `SuperColorPicker` |
| `Disabled` | `bool` | `false` | Disables the trigger button |
| `CssClass` | `string?` | `null` | Additional CSS class on the wrapper `<div>` |

> All parameters of `SuperDropDownColorPicker` are forwarded to the inner `SuperColorPicker`.

---

## Usage Examples

### Basic binding

```razor
<SuperColorPicker @bind-Value="_color" />

@code {
    private string? _color = "#E74C3C";
}
```

---

### With label

```razor
<SuperColorPicker Label="Background color" @bind-Value="_bg" />
<SuperColorPicker Label="Text color"       @bind-Value="_fg" />
```

---

### Alpha channel

When `ShowAlpha="true"` the emitted value switches to `#RRGGBBAA` format (8 hex digits). The last two digits encode alpha `00`–`FF`.

```razor
<SuperColorPicker Label="Overlay color" ShowAlpha="true" @bind-Value="_overlay" />

@code {
    private string? _overlay = "#000000BF"; // black at ~75 % opacity
}
```

To convert back to CSS `rgba()` for use in `style` attributes:

```csharp
private string ToCssColor(string? hex)
{
    if (string.IsNullOrWhiteSpace(hex))
    {
        return "transparent";
    }

    var h = hex.TrimStart('#');

    if (h.Length == 8
        && uint.TryParse(h[..6], NumberStyles.HexNumber, null, out var rgb)
        && int.TryParse(h[6..],  NumberStyles.HexNumber, null, out var aa))
    {
        var r = (rgb >> 16) & 0xFF;
        var g = (rgb >> 8)  & 0xFF;
        var b = rgb & 0xFF;
        return $"rgba({r},{g},{b},{aa / 255.0:F2})";
    }

    return hex; // plain #RRGGBB — valid as-is in CSS
}
```

---

### Dropdown variant

```razor
<SuperDropDownColorPicker Label="Accent color" @bind-Value="_accent" />

@code {
    private string? _accent = "#F39C12";
}
```

With alpha:

```razor
<SuperDropDownColorPicker Label="Shadow color" ShowAlpha="true" @bind-Value="_shadow" />
```

---

### Inside an EditForm

Both components integrate with Blazor's EditForm and model validation.

```razor
<EditForm Model="_model" OnValidSubmit="Save">
    <DataAnnotationsValidator />

    <div class="mb-3">
        <SuperColorPicker Label="Brand color"
                          @bind-Value="_model.BrandColor"
                          ValueExpression="@(() => _model.BrandColor)" />
        <ValidationMessage For="@(() => _model.BrandColor)" />
    </div>

    <div class="mb-3">
        <SuperDropDownColorPicker Label="Accent color"
                                  @bind-Value="_model.AccentColor"
                                  ValueExpression="@(() => _model.AccentColor)" />
        <ValidationMessage For="@(() => _model.AccentColor)" />
    </div>

    <button type="submit" class="btn btn-primary">Save</button>
</EditForm>

@code {
    private BrandSettings _model = new();

    private Task Save()
    {
        // persist _model
        return Task.CompletedTask;
    }

    private class BrandSettings
    {
        [Required]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Must be a valid HEX color.")]
        public string? BrandColor { get; set; } = "#3498DB";

        [Required]
        public string? AccentColor { get; set; } = "#E74C3C";
    }
}
```

> **Tip:** `ValueExpression` must target the same model property as `@bind-Value`. Blazor uses it to identify the field in the `EditContext` and trigger validation messages.

---

### Disabled / read-only

```razor
<!-- Inline picker — read-only -->
<SuperColorPicker Value="#2ECC71" Disabled="true" />

<!-- Dropdown — read-only -->
<SuperDropDownColorPicker Label="Theme color" Value="#9B59B6" Disabled="true" />
```

---

### Multiple pickers side by side

```razor
<div class="d-flex flex-wrap gap-4">
    <SuperColorPicker Label="Primary"    @bind-Value="_primary" />
    <SuperColorPicker Label="Secondary"  @bind-Value="_secondary" />
    <SuperColorPicker Label="Accent"     @bind-Value="_accent" />
</div>
```

Or, compact with the dropdown variant:

```razor
<div class="d-flex align-items-center gap-3">
    <SuperDropDownColorPicker Label="Primary"   @bind-Value="_primary" />
    <SuperDropDownColorPicker Label="Secondary" @bind-Value="_secondary" />
    <SuperDropDownColorPicker Label="Accent"    @bind-Value="_accent" />
</div>
```

---

## Value format

| Scenario | Format | Example |
|---|---|---|
| `ShowAlpha = false` (default) | `#RRGGBB` | `#3498DB` |
| `ShowAlpha = true` | `#RRGGBBAA` | `#3498DBFF` (fully opaque) |
| Initial `Value = null` | Picker starts with a default blue | — |

The value is always emitted as **uppercase HEX**. It is safe to use directly in CSS `background-color` or `color` style attributes.

Alpha `FF` = fully opaque, `00` = fully transparent.

---

## Input modes

The bottom row of the picker exposes two input modes, switchable with the **⇄** button:

| Mode | Fields | Notes |
|---|---|---|
| **HEX** (default) | One text input accepting `#RGB`, `#RRGGBB`, `#RRGGBBAA` | Short 3-digit notation is expanded automatically |
| **RGB(A)** | `R`, `G`, `B` (0–255) and optionally `A` (0–255) | Numeric inputs with no spinner arrows |

Switching modes does **not** change the value — only the display of the input fields.

---

## Keyboard navigation

### SuperColorPicker

| Element | Key | Action |
|---|---|---|
| HEX input | Type | Updates color immediately on blur / Enter |
| RGB inputs | Type | Updates color immediately on blur / Enter |
| ⇄ button | `Enter` / `Space` | Toggles HEX ↔ RGB mode |

> The HSV canvas and sliders use the Pointer Events API. Full keyboard navigation of the canvas (arrow keys) is not yet implemented.

### SuperDropDownColorPicker

| Element | Key | Action |
|---|---|---|
| Trigger button | `Enter` / `Space` | Opens or closes the dropdown |
| Trigger button | `Escape` | Closes the dropdown |
| Trigger button | `Tab` | Closes the dropdown and moves focus |
| Backdrop | Click | Closes the dropdown |

---

## CSS customization

Both components use **scoped CSS isolation** (`.razor.css` files). Class names are prefixed to avoid collisions:

- `SuperColorPicker` — prefix `scp-`
- `SuperDropDownColorPicker` — prefix `scpdd-`

### Override the picker width

The picker card is fixed at **240 px** by default. Override via `CssClass`:

```razor
<SuperColorPicker CssClass="my-wide-picker" @bind-Value="_color" />
```

```css
/* In your own (non-isolated) stylesheet */
.my-wide-picker .scp-picker {
    width: 300px;
}
```

### Override trigger button size

```razor
<SuperDropDownColorPicker CssClass="my-trigger" @bind-Value="_color" />
```

```css
.my-trigger .scpdd-trigger {
    padding: 0.5rem 0.75rem;
}

.my-trigger .scpdd-swatch {
    width: 28px;
    height: 28px;
}
```

### Bootstrap CSS variables

Both components consume Bootstrap 5 CSS variables, so they adapt automatically to custom themes:

| Variable | Used for |
|---|---|
| `--bs-body-bg` | Picker card background, trigger button background |
| `--bs-border-color` | All borders |
| `--bs-body-color` | Text and caret color |
| `--bs-secondary-color` | Input labels, caret muted color |

---

## Tips & Best Practices

- **`@bind-Value` vs `Value` + `ValueChanged`** — prefer `@bind-Value` for two-way binding; use the split form only when you need to intercept or transform the value before propagation.

- **EditForm validation** — always set `ValueExpression` when using either component inside an `EditForm`. Without it, field-level validation messages won't work.

- **Alpha and CSS** — `#RRGGBBAA` values are **not** valid CSS directly in older browsers. Use the `ToCssColor()` helper (see [Alpha channel example](#alpha-channel)) when applying the value to a `style` attribute.

- **Multiple pickers** — use `SuperDropDownColorPicker` instead of `SuperColorPicker` when vertical space is limited. Each `SuperDropDownColorPicker` instance manages its own open/close state independently.

- **Performance** — `ValueChanged` fires on every pointer move during a drag. Avoid expensive re-renders in the callback; update a field and let Blazor batch the UI update naturally.

- **Dark mode** — both components inherit Bootstrap 5.3 CSS variables and therefore adapt automatically when `data-bs-theme="dark"` is set on a parent element or `<html>`.
