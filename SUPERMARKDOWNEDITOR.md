# 📝 SuperMarkdownEditor

> Markdown editor for Blazor — rendered view by default, toggle to Markdown source, integrated help dialog powered by `SuperDialogService`, and `@bind-Value` synchronization.

[← Back to README](README.md)

---

## 📑 Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Integrated Markdown Help](#integrated-markdown-help)
- [Markdown Syntax Shown in Help](#markdown-syntax-shown-in-help)
- [Usage Examples](#usage-examples)
- [CSS Customization](#css-customization)
- [Best Practices](#best-practices)
- [Known Limitations](#known-limitations)

---

## Overview

`SuperMarkdownEditor` is a lightweight Markdown editor designed for business forms, notes, comments, and short editorial content.

The component starts in **rendered mode**. The toolbar is aligned to the right and displays:

- a **Help** button, which opens a Markdown help dialog through `SuperDialogService`;
- a **Markdown / Rendered** toggle.

In Markdown mode, users edit the raw source directly in a `textarea`. In rendered mode, the content is displayed as HTML.

**Key Features**

- 📝 **Native Markdown value** — the bound value remains Markdown, not HTML
- 👁️ **Rendered by default** — comfortable reading on initial display
- 🔁 **Markdown / Rendered toggle** — switch between source and preview
- ❔ **Integrated help** — Markdown examples in a scrollable dialog
- 📐 **Configurable height** — `MinHeight` and `MaxHeight`
- ♿ **Disabled state** — `Disabled`
- 🎨 **Scoped CSS** — classes are prefixed with `sme-`

---

## Quick Start

### Namespace

```razor
@using SuperBlazorComponents.Components.SuperMarkdownEditor
```

### Service Registration

```csharp
// Program.cs
builder.Services.AddSuperComponents();
```

### Dialog Host

The **Help** button uses `SuperDialogService`. Place a `<SuperDialog />` host in your layout or in a persistent parent component.

```razor
@using SuperBlazorComponents.Components.Dialogs

<SuperDialog />
```

### Minimal Example

```razor
@using SuperBlazorComponents.Components.SuperMarkdownEditor

<SuperMarkdownEditor @bind-Value="_markdown" />

@code {
    private string? _markdown = "# Hello\n\nText in **Markdown**.";
}
```

---

## API Reference

### `SuperMarkdownEditor`

| Parameter | Type | Default | Description |
|---|---:|---:|---|
| `Label` | `string?` | `null` | Label displayed above the editor. |
| `Value` | `string?` | `null` | Current Markdown content. |
| `ValueChanged` | `EventCallback<string?>` | - | Callback raised when content changes. |
| `Disabled` | `bool` | `false` | Disables the editor and toolbar. |
| `MinHeight` | `int` | `150` | Minimum height in pixels. |
| `MaxHeight` | `int` | `0` | Maximum height in pixels. `0` means no limit. |
| `ShowToolbar` | `bool` | `true` | Shows or hides the integrated toolbar. |
| `RenderedView` | `bool` | `true` | Current mode: `true` = rendered, `false` = Markdown source. |
| `RenderedViewChanged` | `EventCallback<bool>` | - | Mode change callback. |
| `EditorId` | `string?` | `null` | Optional stable editor identifier. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Additional attributes applied to the `textarea`. |

### `SuperMarkdownEditorToolbar`

| Parameter | Type | Default | Description |
|---|---:|---:|---|
| `Disabled` | `bool` | `false` | Disables toolbar buttons. |
| `RenderedView` | `bool` | `true` | Current mode. |
| `RenderedViewChanged` | `EventCallback<bool>` | - | Markdown / Rendered toggle callback. |

---

## Integrated Markdown Help

The **Help** button opens `SuperMarkdownEditorHelpDialog` through `SuperDialogService`.

The dialog:

- limits its height to `72vh`;
- enables vertical scrolling in the modal body;
- hides the header close icon;
- disables closing by clicking the backdrop;
- displays a single visible button: **Close**.

This help is available from both the integrated toolbar and an external toolbar.

---

## Markdown Syntax Shown in Help

### Text

```markdown
**text**     # bold
*text*      # italic
~~text~~    # strikethrough
```

### Headings

```markdown
# Heading 1
## Heading 2
### Heading 3
```

### Lists

```markdown
- Item 1
- Item 2

1. First
2. Second
```

### Links and Images

```markdown
[text](https://example.com)
![alt](https://example.com/image.png)
```

### Tables

```markdown
| Column 1 | Column 2 |
| --- | --- |
| Value 1 | Value 2 |
| Value 3 | Value 4 |
```

Alignment:

```markdown
| :--- |   # left
| :---: |  # center
| ---: |   # right
```

---

## Usage Examples

### With Label

```razor
<SuperMarkdownEditor Label="Notes"
                     @bind-Value="_notes"
                     MinHeight="180" />
```

### Initial Content

```razor
<SuperMarkdownEditor Label="Draft"
                     @bind-Value="_draft"
                     MinHeight="220" />

@code {
    private string? _draft = "# Title\n\nA paragraph with **bold** and *italic* text.";
}
```

### Controlled Mode

```razor
<SuperMarkdownEditor @bind-Value="_markdown"
                     @bind-RenderedView="_renderedView" />

@code {
    private string? _markdown;
    private bool _renderedView = true;
}
```

### External Toolbar

```razor
<SuperMarkdownEditorToolbar @bind-RenderedView="_renderedView" />

<SuperMarkdownEditor @bind-Value="_markdown"
                     @bind-RenderedView="_renderedView"
                     ShowToolbar="false" />

@code {
    private string? _markdown;
    private bool _renderedView = true;
}
```

### Read-Only

```razor
<SuperMarkdownEditor Label="Read-only"
                     Value="# Locked document"
                     Disabled="true" />
```

---

## CSS Customization

Classes are prefixed with `sme-`.

Main files:

- `SuperMarkdownEditor.razor.css`
- `SuperMarkdownEditorToolbar.razor.css`
- `SuperMarkdownEditorHelpDialog.razor.css`

The component uses Bootstrap variables (`--bs-body-bg`, `--bs-body-color`, `--bs-border-color`, etc.) to stay compatible with light and dark themes.

---

## Best Practices

- Use `@bind-Value` to store raw Markdown in your application.
- Place `<SuperDialog />` once in the layout if you use the **Help** button.
- Use `MaxHeight` in dense forms so the editor does not push the whole page down.
- Prefer rendered mode by default for review screens, and let users switch to Markdown when editing.

---

## Known Limitations

- The built-in Markdown renderer covers common needs, but it is not a full Markdown engine.
- Tables are documented in the help dialog, but built-in rendering may evolve depending on the internal parser.
- The component does not yet automatically notify `EditContext` through `ValueExpression`.

---

[← Back to README](README.md)
