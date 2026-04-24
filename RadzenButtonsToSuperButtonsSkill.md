---
name: RadzenButtonsToSuperButtonsSkill
description: >
  Migration guide from Radzen Blazor button components (RadzenButton, RadzenSplitButton,
  RadzenToggleButton) to SuperBlazorComponents button components (SuperButton,
  SuperSplitButton, SuperToggleButton, SuperLinkButton, SuperConfirmationButton).
  Also covers icon conversion from Material Icons (Google Fonts, used by Radzen)
  to Font Awesome 7 icons (used by SuperBlazorComponents).
applyTo: "**/*.razor"
---

# Skill: Migrate Radzen Buttons → SuperButtons

## Overview

SuperBlazorComponents provides a complete set of button components using **Bootstrap 5**
styling and **Font Awesome 7** icons, while Radzen uses its own styling system and
**Google Material Icons**. The API is intentionally similar but has important differences
in icon names, style variants, sizing, and event signatures.

---

## 1. Required Namespace

### Radzen
```razor
@using Radzen.Blazor
@using Radzen
```

### SuperButtons
```razor
@using SuperBlazorComponents.Components.Buttons
@using SuperBlazorComponents.Components   @* for SuperIconStyle *@
```

---

## 2. Component Mapping

| Radzen component | SuperBlazorComponents component |
|---|---|
| `<RadzenButton>` | `<SuperButton>` |
| `<RadzenSplitButton>` + `<RadzenSplitButtonItem>` | `<SuperSplitButton>` + `<SuperSplitButtonItem>` |
| `<RadzenToggleButton>` | `<SuperToggleButton>` |
| *(no equivalent)* | `<SuperLinkButton>` (button styled as `<a>`) |
| *(no equivalent)* | `<SuperConfirmationButton>` (button with confirmation dialog) |
| `<RadzenStack>` / manual `div.btn-group` | `<SuperButtonGroup>` |

---

## 3. Parameter Mapping — `RadzenButton` → `SuperButton`

### Direct equivalents

| Radzen | SuperButton | Notes |
|---|---|---|
| `Text="Save"` | `Text="Save"` | Identical |
| `ChildContent` | `ChildContent` | Identical; overrides `Text` when set |
| `Icon="save"` | `Icon="fa-floppy-disk"` | Material → FA7 — see §7 |
| `Disabled="true"` | `Disabled="true"` | Identical |
| `IsBusy="true"` | `IsBusy="true"` | Identical |
| `BusyText="Saving..."` | `BusyText="Saving..."` | Identical |
| `Click="@OnClick"` | `Click="@OnClick"` | Identical — same `EventCallback<MouseEventArgs>` |
| `class="my-class"` | `class="my-class"` | Captured via `CapturedAttributes` in both |
| `style="..."` | `class="..."` | Prefer a CSS class over inline style |

### Style / Variant mapping

Radzen uses two orthogonal parameters — `ButtonStyle` (color) and `Variant` (fill vs outline):

| Radzen `ButtonStyle` | Radzen `Variant` | SuperButton equivalent |
|---|---|---|
| `ButtonStyle.Primary` | `Variant.Filled` (default) | `Style="SuperButtonStyle.Primary"` |
| `ButtonStyle.Primary` | `Variant.Outlined` | `Style="SuperButtonStyle.Primary" Outline="true"` |
| `ButtonStyle.Primary` | `Variant.Text` | `Style="SuperButtonStyle.Link"` |
| `ButtonStyle.Secondary` | `Variant.Filled` | `Style="SuperButtonStyle.Secondary"` |
| `ButtonStyle.Secondary` | `Variant.Outlined` | `Style="SuperButtonStyle.Secondary" Outline="true"` |
| `ButtonStyle.Light` | `Variant.Filled` | `Style="SuperButtonStyle.Light"` |
| `ButtonStyle.Dark` | `Variant.Filled` | `Style="SuperButtonStyle.Dark"` |
| `ButtonStyle.Success` | `Variant.Filled` | `Style="SuperButtonStyle.Success"` |
| `ButtonStyle.Success` | `Variant.Outlined` | `Style="SuperButtonStyle.Success" Outline="true"` |
| `ButtonStyle.Danger` | `Variant.Filled` | `Style="SuperButtonStyle.Danger"` |
| `ButtonStyle.Danger` | `Variant.Outlined` | `Style="SuperButtonStyle.Danger" Outline="true"` |
| `ButtonStyle.Warning` | `Variant.Filled` | `Style="SuperButtonStyle.Warning"` |
| `ButtonStyle.Warning` | `Variant.Outlined` | `Style="SuperButtonStyle.Warning" Outline="true"` |
| `ButtonStyle.Info` | `Variant.Filled` | `Style="SuperButtonStyle.Info"` |
| `ButtonStyle.Info` | `Variant.Outlined` | `Style="SuperButtonStyle.Info" Outline="true"` |
| Any style | `Variant.Flat` | No direct equivalent — use `Outline="false"` (default) |

> **Default:** `SuperButtonStyle.Primary` and `Outline="false"` (same as Radzen defaults).

### Size mapping

| Radzen `ButtonSize` | SuperButton `SuperButtonSize` |
|---|---|
| `ButtonSize.ExtraSmall` | `Size="SuperButtonSize.SuperSmall"` |
| `ButtonSize.Small` | `Size="SuperButtonSize.Small"` |
| `ButtonSize.Medium` *(default)* | `Size="SuperButtonSize.Default"` *(default)* |
| `ButtonSize.Large` | `Size="SuperButtonSize.Large"` |

### Icon style

Radzen renders icons as `<i class="rzi">save</i>` (Material Icons ligature). SuperButton
renders icons using Font Awesome 7 CSS classes. Control the FA style with `IconStyle`:

| `SuperIconStyle` value | FA7 prefix applied | When to use |
|---|---|---|
| `Configuration` *(default)* | Uses `DefaultSuperIconeStyle` from `Program.cs` | Default — follows app-wide setting |
| `Solid` | `fa-solid` | Filled icons |
| `Regular` | `fa-regular` | Outline icons (requires FA Pro or free if available) |
| `Brands` | `fa-brands` | Brand logos (GitHub, Google, etc.) |
| `Duotone` | `fa-duotone` | Two-tone icons (FA Pro) |

### Parameters with no SuperButton equivalent

| Radzen parameter | Recommended workaround |
|---|---|
| `IconColor="@Colors.Primary"` | Apply color via CSS class using `class=` attribute |
| `MouseEnter` / `MouseLeave` | Use standard `@onmouseenter` / `@onmouseleave` HTML attributes (captured by `CapturedAttributes`) |
| `ButtonType.Submit` | Use native `<button type="submit">` or add `type="submit"` via captured attributes |
| `ButtonType.Reset` | Use `type="reset"` via captured attributes |
| `AlwaysShowLabel` | `AllowCollapse="false"` (default is `true`, which hides text in collapsed sidebar) |
| `ImageStyle` | Use the `Image` parameter directly with CSS on a wrapper |

---

## 4. Before / After Example — `RadzenButton`

### Before (Radzen)

```razor
<RadzenButton Text="Save"
              Icon="save"
              ButtonStyle="ButtonStyle.Success"
              Variant="Variant.Outlined"
              Size="ButtonSize.Small"
              IsBusy="@isSaving"
              BusyText="Saving..."
              Disabled="@isDisabled"
              Click="@OnSave" />

<RadzenButton Text="Delete"
              Icon="delete"
              ButtonStyle="ButtonStyle.Danger"
              Click="@OnDelete" />

<RadzenButton Icon="add"
              Text="New"
              ButtonStyle="ButtonStyle.Primary" />
```

### After (SuperButton)

```razor
<SuperButton Text="Save"
             Icon="fa-floppy-disk"
             Style="SuperButtonStyle.Success"
             Outline="true"
             Size="SuperButtonSize.Small"
             IsBusy="@isSaving"
             BusyText="Saving..."
             Disabled="@isDisabled"
             Click="@OnSave" />

<SuperButton Text="Delete"
             Icon="fa-trash"
             Style="SuperButtonStyle.Danger"
             Click="@OnDelete" />

<SuperButton Icon="fa-plus"
             Text="New"
             Style="SuperButtonStyle.Primary" />
```

---

## 5. Split Button Migration

### Radzen split button

```razor
<RadzenSplitButton Text="Actions"
                   Icon="more_vert"
                   ButtonStyle="ButtonStyle.Secondary"
                   Click="@OnMainAction">
    <ChildContent>
        <RadzenSplitButtonItem Text="Edit" Icon="edit" Value="edit" />
        <RadzenSplitButtonItem Text="Delete" Icon="delete" Value="delete" Disabled="true" />
        <RadzenSplitButtonItem Separator="true" />
        <RadzenSplitButtonItem Text="Export" Icon="file_download" Value="export" />
    </ChildContent>
</RadzenSplitButton>

@code {
    void OnMainAction(RadzenSplitButtonItem? item)
    {
        if (item is null) { /* main button click */ return; }
        switch (item.Value)
        {
            case "edit": EditItem(); break;
            case "delete": DeleteItem(); break;
            case "export": ExportItem(); break;
        }
    }
}
```

### SuperSplitButton equivalent

```razor
<SuperSplitButton Text="Actions"
                  Icon="fa-ellipsis-vertical"
                  Style="SuperButtonStyle.Secondary"
                  Click="@OnMainClick"
                  ActionSelected="@OnActionSelected">
    <Menu>
        <SuperSplitButtonItem Text="Edit" Icon="fa-pen" ActionName="edit" />
        <SuperSplitButtonItem Text="Delete" Icon="fa-trash" ActionName="delete" Disabled="true" />
        <SuperSplitDivider />
        <SuperSplitButtonItem Text="Export" Icon="fa-file-arrow-down" ActionName="export" />
    </Menu>
</SuperSplitButton>

@code {
    async Task OnMainClick(MouseEventArgs e)
    {
        // main button clicked directly
    }

    async Task OnActionSelected(SuperSplitButtonActionEventArgs args)
    {
        switch (args.ActionName)
        {
            case "edit": EditItem(); break;
            case "delete": DeleteItem(); break;
            case "export": ExportItem(); break;
        }
    }
}
```

### Split button parameter mapping

| Radzen (`RadzenSplitButton`) | SuperSplitButton | Notes |
|---|---|---|
| `Text` | `Text` | Identical |
| `Icon` *(Material)* | `Icon` *(FA7)* | Convert icon name — see §7 |
| `ButtonStyle` | `Style` (SuperButtonStyle) | See §3 style table |
| `Variant` | `Outline` | `Outlined` → `Outline="true"` |
| `Size` | `Size` (SuperButtonSize) | See §3 size table |
| `Disabled` | `Disabled` | Identical |
| `Click` → `RadzenSplitButtonItem?` | `Click` → `MouseEventArgs` + `ActionSelected` → `SuperSplitButtonActionEventArgs` | Split into two events |
| `ChildContent` (items) | `Menu` (items) | Renamed render fragment |

### Split button item mapping

| Radzen (`RadzenSplitButtonItem`) | SuperSplitButtonItem | Notes |
|---|---|---|
| `Text` | `Text` | Identical |
| `Icon` *(Material)* | `Icon` *(FA7)* | Convert icon name — see §7 |
| `Value` (string identifier) | `ActionName` (string identifier) | Renamed |
| `Disabled` | `Disabled` | Identical |
| `Separator="true"` | `<SuperSplitDivider />` | Separate component |
| `ChildContent` | `ChildContent` | Identical |

---

## 6. Toggle Button Migration

### Radzen toggle button

```razor
<RadzenToggleButton Text="Bold"
                    Icon="format_bold"
                    ToggleIcon="format_bold"
                    ButtonStyle="ButtonStyle.Secondary"
                    @bind-Value="@isBold"
                    Change="@OnBoldChanged" />

@code {
    bool isBold;
    void OnBoldChanged(bool value) { }
}
```

### SuperToggleButton equivalent

```razor
<SuperToggleButton Text="Bold"
                   Icon="fa-bold"
                   Style="SuperButtonStyle.Secondary"
                   @bind-Active="@isBold"
                   Click="@OnBoldClicked" />

@code {
    bool isBold;
    async Task OnBoldClicked(MouseEventArgs e)
    {
        // isBold is already updated via @bind-Active
    }
}
```

### Toggle button parameter mapping

| Radzen (`RadzenToggleButton`) | SuperToggleButton | Notes |
|---|---|---|
| `Text` | `Text` | Identical |
| `Icon` | `Icon` *(FA7)* | Single icon — no separate toggle icon |
| `ToggleIcon` | *(no equivalent)* | Use one icon; show/hide text via `ChildContent` if needed |
| `ToggleText` | *(no equivalent)* | Swap text manually using `@bind-Active` state |
| `@bind-Value` | `@bind-Active` | Renamed; both `bool` binding |
| `Value` / `ValueChanged` | `Active` / `ActiveChanged` | Renamed |
| `Change` | `Click` | Super uses base `Click` from `SuperButtonBase` |
| `ButtonStyle` | `Style` (SuperButtonStyle) | See §3 style table |
| `ToggleButtonStyle` | *(no equivalent)* | Use CSS class on `ChildContent` based on `Active` state |
| `Variant` | `Outline` | Same mapping as `SuperButton` |
| `Size` | `Size` | Same mapping as `SuperButton` |
| `Disabled` | `Disabled` (inherited from `SuperButtonBase`) | Identical |

---

## 7. SuperLinkButton (New in Super, no Radzen equivalent)

When a Radzen `RadzenButton` is used for navigation (wrapping a link or calling `NavigationManager.NavigateTo`), replace it with `SuperLinkButton`:

```razor
@* Radzen pattern — navigation inside Click handler *@
<RadzenButton Text="View Details"
              Icon="open_in_new"
              Click="@(() => nav.NavigateTo($"/orders/{id}"))" />

@* SuperLinkButton — renders as <a> for correct semantics *@
<SuperLinkButton Text="View Details"
                 Icon="fa-arrow-up-right-from-square"
                 Href="@($"/orders/{id}")"
                 OpenInNewTab="true" />
```

### SuperLinkButton parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | — | Button label |
| `Href` | `string?` | `null` | Navigation URL |
| `Icon` | `string?` | `null` | FA7 icon class (e.g., `fa-house`) |
| `Image` | `string?` | `null` | Image URL instead of icon |
| `IconStyle` | `SuperIconStyle` | `Configuration` | FA7 style prefix |
| `Outline` | `bool` | `false` | Outline variant |
| `Style` | `SuperButtonStyle` | `Primary` | Color style |
| `Size` | `SuperButtonSize` | `Default` | Size |
| `Disabled` | `bool` | `false` | Disabled state |
| `OpenInNewTab` | `bool` | `false` | Adds `target="_blank"` |
| `AllowCollapse` | `bool` | `true` | Collapses to icon when sidebar collapses |
| `BadgeText` | `string?` | `null` | Badge content |
| `BadgeCssClass` | `string` | `"badge text-bg-secondary"` | Badge CSS |

---

## 8. SuperConfirmationButton (New in Super, no Radzen equivalent)

Replace Radzen confirmation dialog patterns with `SuperConfirmationButton`:

```razor
@* Radzen pattern — manual confirmation dialog *@
<RadzenButton Text="Delete"
              Icon="delete"
              ButtonStyle="ButtonStyle.Danger"
              Click="@AskConfirmation" />
@if (showConfirm)
{
    <RadzenDialog ... />
}

@* SuperConfirmationButton — built-in confirmation *@
<SuperConfirmationButton Text="Delete"
                         Icon="fa-trash"
                         Style="SuperButtonStyle.Danger"
                         ConfirmationTitle="Confirm deletion"
                         ConfirmationContent="Are you sure you want to delete this item? This action cannot be undone."
                         Click="@OnDelete" />
```

The `Click` handler is **only invoked after the user confirms** the dialog.

---

## 9. Button Group Migration

```razor
@* Radzen *@
<div class="d-flex gap-1">
    <RadzenButton Text="Left" />
    <RadzenButton Text="Center" />
    <RadzenButton Text="Right" />
</div>

@* SuperButtonGroup *@
<SuperButtonGroup AriaLabel="Text alignment">
    <Buttons>
        <SuperButton Text="Left" />
        <SuperButton Text="Center" />
        <SuperButton Text="Right" />
    </Buttons>
</SuperButtonGroup>

@* Vertical group *@
<SuperButtonGroup Vertical="true" AriaLabel="Actions">
    <Buttons>
        <SuperButton Text="Save" Icon="fa-floppy-disk" />
        <SuperButton Text="Cancel" Icon="fa-xmark" Style="SuperButtonStyle.Secondary" />
    </Buttons>
</SuperButtonGroup>
```

---

## 10. Icon Conversion: Material Icons → Font Awesome 7

Radzen uses Google Material Icons (ligature-based, e.g. `Icon="edit"`).
SuperButton uses Font Awesome 7 CSS classes (e.g. `Icon="fa-pen"`).

The `Icon` value passed to SuperButton is the FA7 icon name **without the style prefix**
(e.g. `"fa-pen"`, not `"fa-solid fa-pen"`). The prefix is controlled by `IconStyle`.

### Common icon mapping table

| Material Icon (Radzen) | FA7 Icon (`Icon=`) | Notes |
|---|---|---|
| `edit` | `fa-pen` | |
| `delete` / `delete_outline` | `fa-trash` | Use `IconStyle=Regular` for outline |
| `delete_forever` | `fa-trash-can` | |
| `add` | `fa-plus` | |
| `add_circle` / `add_circle_outline` | `fa-circle-plus` | |
| `add_box` | `fa-square-plus` | |
| `remove` | `fa-minus` | |
| `remove_circle` / `remove_circle_outline` | `fa-circle-minus` | |
| `save` | `fa-floppy-disk` | |
| `search` | `fa-magnifying-glass` | |
| `zoom_in` | `fa-magnifying-glass-plus` | |
| `zoom_out` | `fa-magnifying-glass-minus` | |
| `close` / `cancel` | `fa-xmark` | |
| `check` / `done` / `check_circle` | `fa-check` / `fa-circle-check` | |
| `info` / `info_outline` | `fa-circle-info` | |
| `warning` / `warning_amber` | `fa-triangle-exclamation` | |
| `error` / `error_outline` | `fa-circle-xmark` | |
| `help` / `help_outline` | `fa-circle-question` | |
| `home` | `fa-house` | |
| `settings` / `settings_outline` | `fa-gear` | |
| `tune` | `fa-sliders` | |
| `build` | `fa-wrench` | |
| `person` | `fa-user` | |
| `account_circle` | `fa-circle-user` | |
| `people` / `group` | `fa-users` | |
| `group_add` / `person_add` | `fa-user-plus` | |
| `person_remove` | `fa-user-minus` | |
| `manage_accounts` | `fa-user-gear` | |
| `account_box` | `fa-address-card` | |
| `badge` | `fa-id-badge` | |
| `contact_page` | `fa-address-book` | |
| `email` / `mail` | `fa-envelope` | |
| `phone` | `fa-phone` | |
| `send` | `fa-paper-plane` | |
| `reply` | `fa-reply` | |
| `forward` | `fa-share` | |
| `share` | `fa-share-nodes` | |
| `print` | `fa-print` | |
| `download` / `cloud_download` | `fa-download` / `fa-cloud-arrow-down` | |
| `upload` / `cloud_upload` | `fa-upload` / `fa-cloud-arrow-up` | |
| `file_download` | `fa-file-arrow-down` | |
| `file_upload` | `fa-file-arrow-up` | |
| `refresh` / `sync` / `cached` | `fa-rotate` | |
| `autorenew` | `fa-arrows-rotate` | |
| `filter_list` | `fa-filter` | |
| `sort` | `fa-sort` | |
| `swap_horiz` | `fa-arrows-left-right` | |
| `swap_vert` | `fa-arrows-up-down` | |
| `visibility` | `fa-eye` | |
| `visibility_off` | `fa-eye-slash` | |
| `lock` | `fa-lock` | |
| `lock_open` | `fa-lock-open` | |
| `security` | `fa-shield` | |
| `verified` / `verified_user` | `fa-shield-check` | |
| `vpn_key` / `key` | `fa-key` | |
| `star` | `fa-star` (Solid) | Use `IconStyle=Regular` for `star_border` |
| `favorite` | `fa-heart` (Solid) | Use `IconStyle=Regular` for `favorite_border` |
| `thumb_up` | `fa-thumbs-up` | |
| `thumb_down` | `fa-thumbs-down` | |
| `bookmark` / `bookmarks` | `fa-bookmark` | |
| `tag` / `label` | `fa-tag` | |
| `arrow_back` | `fa-arrow-left` | |
| `arrow_forward` | `fa-arrow-right` | |
| `arrow_upward` | `fa-arrow-up` | |
| `arrow_downward` | `fa-arrow-down` | |
| `chevron_left` / `navigate_before` | `fa-chevron-left` | |
| `chevron_right` / `navigate_next` | `fa-chevron-right` | |
| `expand_more` | `fa-chevron-down` | |
| `expand_less` | `fa-chevron-up` | |
| `first_page` | `fa-angles-left` | |
| `last_page` | `fa-angles-right` | |
| `skip_previous` | `fa-backward-step` | |
| `skip_next` | `fa-forward-step` | |
| `play_arrow` | `fa-play` | |
| `pause` | `fa-pause` | |
| `stop` | `fa-stop` | |
| `menu` | `fa-bars` | |
| `more_vert` | `fa-ellipsis-vertical` | |
| `more_horiz` | `fa-ellipsis` | |
| `notifications` / `notifications_outline` | `fa-bell` | |
| `shopping_cart` | `fa-cart-shopping` | |
| `shopping_bag` | `fa-bag-shopping` | |
| `shopping_basket` | `fa-basket-shopping` | |
| `storefront` | `fa-shop` | |
| `store` | `fa-store` | |
| `calendar_today` / `event` / `date_range` | `fa-calendar-days` | |
| `calendar_month` | `fa-calendar` | |
| `attach_file` | `fa-paperclip` | |
| `link` | `fa-link` | |
| `open_in_new` | `fa-arrow-up-right-from-square` | |
| `image` | `fa-image` | |
| `photo_camera` | `fa-camera` | |
| `content_copy` / `file_copy` / `copy_all` | `fa-copy` | |
| `content_paste` | `fa-paste` | |
| `content_cut` | `fa-scissors` | |
| `undo` | `fa-rotate-left` | |
| `redo` | `fa-rotate-right` | |
| `fullscreen` | `fa-expand` | |
| `fullscreen_exit` | `fa-compress` | |
| `list` / `view_list` | `fa-list` | |
| `grid_view` | `fa-table-cells` | |
| `table_chart` | `fa-table` | |
| `dashboard` | `fa-gauge` | |
| `apps` | `fa-grid` | |
| `bar_chart` | `fa-chart-bar` | |
| `show_chart` / `trending_up` | `fa-chart-line` | |
| `pie_chart` | `fa-chart-pie` | |
| `cloud` | `fa-cloud` | |
| `folder` | `fa-folder` | |
| `folder_open` | `fa-folder-open` | |
| `description` / `article` / `note` | `fa-file-lines` | |
| `assignment` | `fa-clipboard` | |
| `assignment_turned_in` / `task_alt` | `fa-clipboard-check` | |
| `task` / `checklist` | `fa-list-check` | |
| `summarize` | `fa-file-lines` | |
| `report` | `fa-flag` | |
| `logout` / `exit_to_app` | `fa-right-from-bracket` | |
| `login` | `fa-right-to-bracket` | |
| `power_settings_new` | `fa-power-off` | |
| `location_on` / `place` | `fa-location-dot` | |
| `map` | `fa-map` | |
| `business` / `apartment` | `fa-building` | |
| `warehouse` | `fa-warehouse` | |
| `local_shipping` / `local_shipping_outline` | `fa-truck` | |
| `inventory` / `inbox` | `fa-box` / `fa-inbox` | |
| `inventory_2` | `fa-boxes-stacked` | |
| `category` | `fa-layer-group` | |
| `sell` | `fa-tag` | |
| `receipt` | `fa-receipt` | |
| `payments` / `payment` | `fa-credit-card` | |
| `account_balance` | `fa-landmark` | |
| `calculate` | `fa-calculator` | |
| `price_check` | `fa-money-check` | |
| `euro` | `fa-euro-sign` | |
| `attach_money` | `fa-dollar-sign` | |
| `support_agent` | `fa-headset` | |
| `gavel` | `fa-gavel` | |
| `handshake` | `fa-handshake` | |
| `format_bold` | `fa-bold` | |
| `format_italic` | `fa-italic` | |
| `format_underlined` | `fa-underline` | |
| `format_list_bulleted` | `fa-list-ul` | |
| `format_list_numbered` | `fa-list-ol` | |

### Brand icons (use `IconStyle="SuperIconStyle.Brands"`)

| Material approach | FA7 Brand icon | `IconStyle` |
|---|---|---|
| *(custom SVG)* | `fa-github` | `Brands` |
| *(custom SVG)* | `fa-google` | `Brands` |
| *(custom SVG)* | `fa-microsoft` | `Brands` |
| *(custom SVG)* | `fa-linkedin` | `Brands` |
| *(custom SVG)* | `fa-twitter` / `fa-x-twitter` | `Brands` |

### Looking up icons you can't find in this table

1. Find the Material icon name in Radzen `Icon` parameter (e.g., `"task_alt"`)
2. Search for its visual equivalent at **https://fontawesome.com/search** (use the English description)
3. Copy the icon name from the FA7 site (e.g., `fa-circle-check`)
4. Set `Icon="fa-circle-check"` — the style prefix is automatically added based on `IconStyle`

---

## 11. Badge on Button

```razor
@* Radzen — uses separate notification component or custom template *@
<RadzenButton Icon="notifications" ButtonStyle="ButtonStyle.Light" />

@* SuperButton — built-in badge *@
<SuperButton Icon="fa-bell"
             Style="SuperButtonStyle.Light"
             BadgeText="@notifCount.ToString()"
             BadgeCssClass="badge text-bg-danger rounded-pill" />
```

---

## 12. Image Instead of Icon

```razor
@* SuperButton supports an image URL in place of an icon *@
<SuperButton Text="Company"
             Image="/images/logo.png"
             Style="SuperButtonStyle.Light" />
```

---

## 13. Popover (Tooltip/Help)

```razor
@* SuperButton has built-in Bootstrap 5 popover support *@
<SuperButton Icon="fa-circle-info"
             Style="SuperButtonStyle.Info"
             Outline="true"
             PopoverTitle="Help"
             PopoverContent="Click here to learn more about this feature."
             PopoverPlacement="top" />
```

Radzen uses separate `<RadzenTooltip>` service / `TooltipService`. SuperButton handles it inline.

---

## 14. Sidebar Collapse Behavior

SuperButton and SuperLinkButton automatically collapse to **icon-only** when placed inside
a `<SuperLayout>` with a collapsible sidebar. Disable this with `AllowCollapse="false"`:

```razor
@* Always shows text + icon even when sidebar is collapsed *@
<SuperButton Text="Save" Icon="fa-floppy-disk" AllowCollapse="false" />
```

Radzen has no equivalent — this is a SuperButton-specific feature.

---

## 15. Step-by-Step Migration Checklist

1. **Replace namespace**: `@using Radzen.Blazor` + `@using Radzen` → `@using SuperBlazorComponents.Components.Buttons`
2. **Replace component tags**: `<RadzenButton` → `<SuperButton`, `<RadzenSplitButton` → `<SuperSplitButton`, etc.
3. **Convert icons**: Every `Icon="material_name"` → `Icon="fa-icon-name"` using §10 table
4. **Map styles**: `ButtonStyle="ButtonStyle.X"` → `Style="SuperButtonStyle.X"`
5. **Map variant**: `Variant="Variant.Outlined"` → `Outline="true"`; `Variant.Text` → `Style="SuperButtonStyle.Link"`
6. **Map size**: `Size="ButtonSize.X"` → `Size="SuperButtonSize.Y"` using §3 size table
7. **Split button items**: rename `Value` → `ActionName`; replace `Separator="true"` with `<SuperSplitDivider />`
8. **Split button event**: replace single `Click` handler (receiving `RadzenSplitButtonItem?`) with `Click` + `ActionSelected`
9. **Toggle button**: rename `@bind-Value` → `@bind-Active`; rename `Change` → `Click`
10. **Navigation buttons**: replace `Click="@(() => nav.NavigateTo(...))"` with `<SuperLinkButton Href="...">`
11. **Confirmation dialogs**: replace manual dialog pattern with `<SuperConfirmationButton>`
12. **Remove `IsBusy` loading state management**: SuperButton handles spinner automatically when `IsBusy="true"` + `BusyText` is set
13. **Button groups**: wrap grouped buttons in `<SuperButtonGroup>` using `<Buttons>` render fragment
