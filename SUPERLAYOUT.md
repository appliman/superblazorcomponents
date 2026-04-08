# 🖼️ SuperLayout — Complete Documentation

> A responsive application layout system for Blazor built on Bootstrap 5.3, featuring a collapsible sidebar, sticky header & footer, a slide-in chat panel, and automatic device detection — all with smooth CSS Grid transitions and full dark/light mode support.

**[← Back to main README](README.md)**

---

## Table of Contents

- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Service Registration](#service-registration)
  - [Minimal Example](#minimal-example)
- [Architecture Overview](#architecture-overview)
  - [CSS Grid Layout](#css-grid-layout)
  - [CascadingValue Pattern](#cascadingvalue-pattern)
  - [Responsive Breakpoints](#responsive-breakpoints)
- [SuperLayout Parameters](#superlayout-parameters)
  - [Public Properties](#public-properties)
  - [Public Methods](#public-methods)
  - [Events](#events)
- [SuperHeader Parameters](#superheader-parameters)
- [SuperSidebar Parameters](#supersidebar-parameters)
- [SuperBody Parameters](#superbody-parameters)
- [SuperFooter Parameters](#superfooter-parameters)
- [SuperChat Parameters](#superchat-parameters)
- [SuperChatButton Parameters](#superchatbutton-parameters)
- [Enums Reference](#enums-reference)
  - [SidebarState](#sidebarstate)
  - [ChatState](#chatstate)
- [Device Model](#device-model)
- [Usage Examples](#usage-examples)
  - [1 — Minimal Layout (Header + Body + Footer)](#1--minimal-layout-header--body--footer)
  - [2 — Full Layout with Sidebar Navigation](#2--full-layout-with-sidebar-navigation)
  - [3 — Custom Sidebar Widths](#3--custom-sidebar-widths)
  - [4 — Sidebar with Header, Footer, and Theme](#4--sidebar-with-header-footer-and-theme)
  - [5 — Programmatic Sidebar Control](#5--programmatic-sidebar-control)
  - [6 — Listening to Sidebar State Changes](#6--listening-to-sidebar-state-changes)
  - [7 — Chat Panel Integration](#7--chat-panel-integration)
  - [8 — Chat Panel with Custom Header and Footer](#8--chat-panel-with-custom-header-and-footer)
  - [9 — Header with Brand Logo and End Content](#9--header-with-brand-logo-and-end-content)
  - [10 — SuperMenuItem Navigation in Sidebar](#10--supermenuitem-navigation-in-sidebar)
  - [11 — Nested Submenu Items](#11--nested-submenu-items)
  - [12 — Policy-Based Menu Item Visibility](#12--policy-based-menu-item-visibility)
  - [13 — Body with Custom Background and Padding](#13--body-with-custom-background-and-padding)
  - [14 — Non-Sticky Header and Footer](#14--non-sticky-header-and-footer)
  - [15 — Theme Toggle in Header](#15--theme-toggle-in-header)
  - [16 — Complete Enterprise Application Layout](#16--complete-enterprise-application-layout)
- [CSS Custom Properties](#css-custom-properties)
- [Tips & Best Practices](#tips--best-practices)

---

## Getting Started

### Installation

```bash
dotnet add package SuperBlazorComponents
```

### Service Registration

In your `Program.cs`, register the SuperBlazorComponents services:

```csharp
builder.Services.AddSuperComponents(options =>
{
    options.DefaultSuperIconeStyle = SuperIconStyle.Solid;
});
```

### Minimal Example

```razor
@using SuperBlazorComponents.Components.SuperLayout

<SuperLayout>
    <SuperHeader BrandText="My App" />
    <SuperBody>
        <h1>Welcome!</h1>
        <p>This is the main content area.</p>
    </SuperBody>
    <SuperFooter>
        <span>© 2025 My Company</span>
    </SuperFooter>
</SuperLayout>
```

This produces a full-height layout with a sticky header at the top, a scrollable content area, and a sticky footer at the bottom.

---

## Architecture Overview

### CSS Grid Layout

SuperLayout uses **CSS Grid** with three named areas organized in a 3×3 grid:

```
┌──────────────────────────────────────────────┐
│                   header                      │
├──────────┬──────────────────┬────────────────┤
│ sidebar  │       body       │   chatpanel    │
├──────────┴──────────────────┴────────────────┤
│                   footer                      │
└──────────────────────────────────────────────┘
```

The grid template adapts dynamically based on the sidebar and chat panel states:

| State | `grid-template-columns` |
|---|---|
| Sidebar expanded | `var(--super-sidebar-width) 1fr 0` |
| Sidebar collapsed | `var(--super-sidebar-collapsed-width) 1fr 0` |
| Sidebar hidden | `0 1fr 0` |
| Chat panel open | Last column becomes `var(--super-chatpanel-width)` |

All transitions are animated with a `0.3s ease` timing function.

### CascadingValue Pattern

`SuperLayout` exposes itself as a `CascadingValue`, allowing all child components (`SuperHeader`, `SuperSidebar`, `SuperBody`, `SuperFooter`, `SuperChat`, `SuperChatButton`) to access the parent layout via a `[CascadingParameter]`:

```csharp
[CascadingParameter]
public SuperLayout ParentLayout { get; set; }
```

This enables child components to:
- Read the current `SidebarState` and `ChatPanelState`
- Call `ToggleSidebar()`, `ToggleChatPanel()`, etc.
- Subscribe to state change events

### Responsive Breakpoints

The layout adapts automatically at three breakpoints:

| Breakpoint | Behavior |
|---|---|
| **≥ 992px** (lg+) | Full layout: sidebar expanded/collapsed/hidden as configured |
| **768–991px** (md) | Sidebar forced to collapsed width (`40px`), icons only |
| **< 576px** (sm) | Sidebar hidden entirely, chat panel goes full-screen overlay |

---

## SuperLayout Parameters

### Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Child components: `SuperHeader`, `SuperSidebar`, `SuperBody`, `SuperFooter`, `SuperChat` |
| `CssClass` | `string?` | `null` | Additional CSS class for the root layout container |
| `Style` | `string?` | `null` | Additional inline style for the root container |
| `SidebarWidth` | `int` | `250` | Sidebar width in pixels when expanded |
| `SidebarCollapsedWidth` | `int` | `40` | Sidebar width in pixels when collapsed |
| `ChatPanelWidth` | `int` | `380` | Chat panel width in pixels when open |

### Public Properties

| Property | Type | Description |
|---|---|---|
| `SidebarState` | `SidebarState` | Current sidebar state (`Expanded`, `Collapsed`, `Hidden`) |
| `ChatPanelState` | `ChatState` | Current chat panel state (`Hidden`, `Open`) |
| `CurrentSidebarWidth` | `int` | Computed sidebar width based on current state |
| `CurrentChatPanelWidth` | `int` | Computed chat panel width based on current state |

### Public Methods

| Method | Signature | Description |
|---|---|---|
| `ToggleSidebar` | `void ToggleSidebar()` | Cycles sidebar state: `Expanded` → `Collapsed` → `Hidden` → `Expanded` |
| `SetSidebarState` | `void SetSidebarState(SidebarState state)` | Sets the sidebar to a specific state |
| `ToggleChatPanel` | `void ToggleChatPanel()` | Toggles the chat panel: `Hidden` ↔ `Open` |
| `SetChatPanelState` | `void SetChatPanelState(ChatState state)` | Sets the chat panel to a specific state |

### Events

| Event | Signature | Description |
|---|---|---|
| `OnSidebarStateChanged` | `Action<SidebarState, SidebarState>?` | Fired when sidebar state changes. Arguments: `(previousState, newState)` |
| `OnChatPanelStateChanged` | `Action<ChatState, ChatState>?` | Fired when chat panel state changes. Arguments: `(previousState, newState)` |

---

## SuperHeader Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Main header content (menu, links, etc.) — rendered in the collapsible navbar area |
| `Brand` | `RenderFragment?` | `null` | Custom brand/logo content (overrides `BrandText`) |
| `BrandText` | `string?` | `null` | Simple text for the brand area (used when `Brand` is not set) |
| `EndContent` | `RenderFragment?` | `null` | Content displayed on the far right of the header |
| `Toolbar` | `RenderFragment?` | `null` | Toolbar content |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `Sticky` | `bool` | `true` | If `true`, header sticks to the top when scrolling |
| `ShowToggle` | `bool` | `true` | Shows the hamburger button that toggles the sidebar |
| `NavbarClass` | `string?` | `null` | Bootstrap navbar color class (e.g. `"navbar-dark"`) |
| `Height` | `int` | `56` | Header height in pixels |
| `OnToggle` | `EventCallback` | — | Callback fired when the sidebar toggle button is clicked |

**Header layout structure:**

```
┌─────────────────────────────────────────────────────┐
│ [☰] [Brand]  │  [ChildContent ...]  │ [EndContent] │
│   toggle      │     navbar body      │   right area │
└─────────────────────────────────────────────────────┘
```

The brand area automatically hides its text when the sidebar is collapsed or hidden, keeping only the toggle button visible.

---

## SuperSidebar Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Navigation content (typically `SuperMenuItem` components) |
| `Header` | `RenderFragment?` | `null` | Content rendered at the top of the sidebar (above navigation) |
| `Footer` | `RenderFragment?` | `null` | Content rendered at the bottom of the sidebar (below navigation) |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `Theme` | `string?` | `null` | Sidebar theme name (e.g. `"software"`). Loads the corresponding CSS file and applies custom styling |
| `Style` | `string?` | `null` | Additional inline style |
| `ShowOverlay` | `bool` | `true` | Shows a dark overlay on mobile when the sidebar is visible |
| `OnOverlayClicked` | `EventCallback` | — | Callback fired when the mobile overlay is clicked |

**Sidebar behavior by state:**

| State | Visual | Header/Footer | Nav Items |
|---|---|---|---|
| `Expanded` | Full width (`SidebarWidth` px) | Visible | Text + icon |
| `Collapsed` | Narrow (`SidebarCollapsedWidth` px) | Hidden | Icon only, centered |
| `Hidden` | 0 px, content invisible | Hidden | Hidden |

**Themed sidebar:**

When a `Theme` is set, the sidebar automatically loads the corresponding CSS file (e.g. `super-theme-software.css`) and applies a `data-sc-theme` attribute. This allows full visual customization of the sidebar independently from the rest of the layout.

---

## SuperBody Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Main page content |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `Style` | `string?` | `null` | Additional inline style |
| `Fluid` | `bool` | `true` | If `true`, uses Bootstrap `container-fluid` (100% width). If `false`, uses centered `container` |
| `Padding` | `int` | `0` | Padding in pixels around the content |
| `BackgroundColor` | `string?` | `null` | CSS background color for the body area |

The body area has `overflow-y: auto` by default, providing its own scroll region independent of the header and footer.

---

## SuperFooter Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Footer content |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `Style` | `string?` | `null` | Additional inline style |
| `Sticky` | `bool` | `true` | If `true`, footer sticks to the bottom |
| `Fluid` | `bool` | `true` | If `true`, uses `container-fluid`. If `false`, uses `container` |
| `Height` | `int` | `48` | Minimum footer height in pixels |
| `BackgroundColor` | `string?` | `null` | CSS background color for the footer |

---

## SuperChat Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Main chat content |
| `Header` | `RenderFragment?` | `null` | Custom header (replaces default title + close button) |
| `Footer` | `RenderFragment?` | `null` | Content for the bottom of the chat panel (e.g. input area) |
| `Title` | `string` | `"Chat IA"` | Title displayed in the default header |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `Style` | `string?` | `null` | Additional inline style |

**Chat panel structure:**

```
┌─────────────────┐
│ Title      [✕]  │  ← Header (default or custom)
├─────────────────┤
│                  │
│  ChildContent    │  ← Scrollable body
│                  │
├─────────────────┤
│  Footer          │  ← Optional footer (input area)
└─────────────────┘
```

On small screens (< 576px), the chat panel opens as a **full-screen overlay** with a subtle shadow.

---

## SuperChatButton Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Icon` | `string` | `"fa-comments"` | Font Awesome icon name (without style prefix) |
| `IconStyle` | `SuperIconStyle` | `Solid` | Font Awesome icon style |
| `Text` | `string?` | `null` | Optional text displayed next to the icon |
| `Tooltip` | `string` | `"Chat IA"` | Button tooltip |
| `CssClass` | `string?` | `null` | Additional CSS class |
| `OnToggle` | `EventCallback` | — | Callback fired when the button is clicked |

The button automatically receives a `super-chat-active` CSS class (highlighted in the primary color) when the chat panel is open.

---

## Enums Reference

### SidebarState

```csharp
public enum SidebarState
{
    Expanded,   // Full width sidebar with text and icons
    Collapsed,  // Narrow sidebar with icons only
    Hidden      // Sidebar completely hidden (0 width)
}
```

### ChatState

```csharp
public enum ChatState
{
    Hidden,  // Chat panel not visible
    Open     // Chat panel visible
}
```

---

## Device Model

The `Device` class is automatically populated via JavaScript interop on first render. It provides client-side device information:

| Property | Type | Description |
|---|---|---|
| `UserAgent` | `string` | Browser user agent string |
| `Name` | `string?` | Device name (default: `"notdetected"`) |
| `Platform` | `string?` | Platform from `navigator.userAgentData` |
| `Os` | `string?` | Detected OS: `"Windows"`, `"MacOS"`, `"Linux"`, `"Android"`, `"iOS"`, or `"Unknown OS"` |
| `IsMobile` | `bool` | `true` if the device is detected as mobile |
| `ScreenWidth` | `int` | `screen.width` value |
| `ScreenHeight` | `int` | `screen.height` value |
| `AvailableWidth` | `int` | `screen.availWidth` (excluding taskbar, etc.) |
| `AvailableHeight` | `int` | `screen.availHeight` |
| `WindowInnerWidth` | `int` | `window.innerWidth` (viewport width) |
| `WindowInnerHeight` | `int` | `window.innerHeight` (viewport height) |

---

## Usage Examples

### 1 — Minimal Layout (Header + Body + Footer)

The simplest layout with just a header, content area, and footer. No sidebar.

```razor
@using SuperBlazorComponents.Components.SuperLayout

<SuperLayout SidebarWidth="0">
    <SuperHeader BrandText="My App" ShowToggle="false" />
    <SuperBody>
        <h1>Welcome to My App</h1>
        <p>This is a simple layout with no sidebar.</p>
    </SuperBody>
    <SuperFooter>
        <span>© 2025 My Company</span>
    </SuperFooter>
</SuperLayout>
```

---

### 2 — Full Layout with Sidebar Navigation

A complete layout with sidebar navigation using `SuperMenuItem`.

```razor
@using SuperBlazorComponents.Components.SuperLayout
@using SuperBlazorComponents.Components

<SuperLayout>
    <SuperHeader BrandText="Dashboard" />
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
        <SuperMenuItem Icon="fa-chart-bar" Text="Analytics" Href="/analytics" />
        <SuperMenuItem Icon="fa-users" Text="Users" Href="/users" />
        <SuperMenuItem Icon="fa-gear" Text="Settings" Href="/settings" />
    </SuperSidebar>
    <SuperBody Padding="16">
        @Body
    </SuperBody>
    <SuperFooter>
        <span class="text-muted">v1.0.0</span>
    </SuperFooter>
</SuperLayout>
```

---

### 3 — Custom Sidebar Widths

Configure the sidebar expanded and collapsed widths.

```razor
<SuperLayout SidebarWidth="300"
             SidebarCollapsedWidth="60">
    <SuperHeader BrandText="Wide Sidebar App" />
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
        <SuperMenuItem Icon="fa-inbox" Text="Inbox" Href="/inbox"
                       BadgeText="5" BadgeCssClass="badge text-bg-danger" />
    </SuperSidebar>
    <SuperBody>
        <p>The sidebar is 300px when expanded and 60px when collapsed.</p>
    </SuperBody>
</SuperLayout>
```

---

### 4 — Sidebar with Header, Footer, and Theme

A sidebar with a custom header, footer, and the built-in "software" dark theme.

```razor
<SuperLayout>
    <SuperHeader BrandText="Studio" />
    <SuperSidebar Theme="software">
        <Header>
            <div class="text-center py-2">
                <img src="/logo.svg" alt="Logo" width="40" />
                <div class="small text-muted mt-1">Studio v2</div>
            </div>
        </Header>

        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
        <SuperMenuItem Icon="fa-code" Text="Editor" Href="/editor" />
        <SuperMenuItem Icon="fa-terminal" Text="Console" Href="/console" />

        <Footer>
            <div class="text-center small text-muted py-2">
                <i class="fa-solid fa-circle-info me-1"></i>Help & Support
            </div>
        </Footer>
    </SuperSidebar>
    <SuperBody>
        @Body
    </SuperBody>
</SuperLayout>
```

The `Theme="software"` parameter applies a dark sidebar with custom colors (`#2f3c44` background, white text, blue accent). You can create your own themes by adding CSS files following the same convention.

---

### 5 — Programmatic Sidebar Control

Control the sidebar state from code using a reference to the `SuperLayout` component.

```razor
<SuperLayout @ref="_layout">
    <SuperHeader BrandText="My App">
        <button class="btn btn-sm btn-outline-secondary" @onclick="ExpandSidebar">
            <i class="fa-solid fa-angles-right"></i> Expand
        </button>
        <button class="btn btn-sm btn-outline-secondary" @onclick="CollapseSidebar">
            <i class="fa-solid fa-angles-left"></i> Collapse
        </button>
        <button class="btn btn-sm btn-outline-secondary" @onclick="HideSidebar">
            <i class="fa-solid fa-eye-slash"></i> Hide
        </button>
    </SuperHeader>
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        <p>Current sidebar state: <strong>@_layout?.SidebarState</strong></p>
        <p>Current sidebar width: <strong>@(_layout?.CurrentSidebarWidth)px</strong></p>
    </SuperBody>
</SuperLayout>

@code {
    private SuperLayout? _layout;

    private void ExpandSidebar() => _layout?.SetSidebarState(SidebarState.Expanded);
    private void CollapseSidebar() => _layout?.SetSidebarState(SidebarState.Collapsed);
    private void HideSidebar() => _layout?.SetSidebarState(SidebarState.Hidden);
}
```

---

### 6 — Listening to Sidebar State Changes

Subscribe to sidebar state change events to trigger custom logic.

```razor
<SuperLayout @ref="_layout">
    <SuperHeader BrandText="My App" />
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        <p>Last transition: <strong>@_lastTransition</strong></p>
    </SuperBody>
</SuperLayout>

@code {
    private SuperLayout? _layout;
    private string _lastTransition = "—";

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && _layout is not null)
        {
            _layout.OnSidebarStateChanged += (prev, next) =>
            {
                _lastTransition = $"{prev} → {next}";
                InvokeAsync(StateHasChanged);
            };
        }
    }
}
```

---

### 7 — Chat Panel Integration

Add a slide-in chat panel to the layout with a toggle button in the header.

```razor
<SuperLayout ChatPanelWidth="400">
    <SuperHeader BrandText="Support App">
        <EndContent>
            <SuperChatButton Tooltip="Open assistant" />
        </EndContent>
    </SuperHeader>
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
        <SuperMenuItem Icon="fa-ticket" Text="Tickets" Href="/tickets" />
    </SuperSidebar>
    <SuperBody>
        <p>Click the chat button in the header to open the assistant panel.</p>
    </SuperBody>
    <SuperChat Title="AI Assistant">
        <p>Hello! How can I help you today?</p>
    </SuperChat>
    <SuperFooter>
        <span>Support Center v3.0</span>
    </SuperFooter>
</SuperLayout>
```

---

### 8 — Chat Panel with Custom Header and Footer

Override the default chat header with a custom one and add a footer input area.

```razor
<SuperLayout @ref="_layout">
    <SuperHeader BrandText="My App">
        <EndContent>
            <SuperChatButton Icon="fa-robot" Text="Ask AI" />
        </EndContent>
    </SuperHeader>
    <SuperBody>
        <p>Chat panel with custom header and footer.</p>
    </SuperBody>
    <SuperChat>
        <Header>
            <div class="d-flex align-items-center justify-content-between w-100 px-3 py-2">
                <div>
                    <i class="fa-solid fa-robot me-2"></i>
                    <strong>AI Copilot</strong>
                    <span class="badge text-bg-success ms-2">Online</span>
                </div>
                <button class="btn btn-sm btn-link" @onclick="CloseChat">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>
        </Header>

        <div class="chat-messages">
            <p>Welcome! Ask me anything.</p>
        </div>

        <Footer>
            <div class="input-group">
                <input type="text" class="form-control" placeholder="Type your message..." />
                <button class="btn btn-primary">
                    <i class="fa-solid fa-paper-plane"></i>
                </button>
            </div>
        </Footer>
    </SuperChat>
</SuperLayout>

@code {
    private SuperLayout? _layout;

    private void CloseChat() => _layout?.SetChatPanelState(ChatState.Hidden);
}
```

---

### 9 — Header with Brand Logo and End Content

Use a custom `Brand` render fragment for a logo, and `EndContent` for user actions.

```razor
<SuperLayout>
    <SuperHeader>
        <Brand>
            <a href="/" class="d-flex align-items-center text-decoration-none">
                <img src="/logo.png" alt="Logo" height="32" class="me-2" />
                <span class="fw-bold">Acme Corp</span>
            </a>
        </Brand>
        <EndContent>
            <div class="d-flex align-items-center gap-3">
                <SuperChatButton />
                <ThemeToggle />
                <div class="dropdown">
                    <button class="btn btn-link text-body-secondary p-0"
                            data-bs-toggle="dropdown">
                        <i class="fa-solid fa-circle-user fa-lg"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="/profile">Profile</a></li>
                        <li><a class="dropdown-item" href="/logout">Logout</a></li>
                    </ul>
                </div>
            </div>
        </EndContent>
    </SuperHeader>
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        @Body
    </SuperBody>
</SuperLayout>
```

---

### 10 — SuperMenuItem Navigation in Sidebar

Use `SuperMenuItem` with icons, badges, and different icon styles.

```razor
<SuperSidebar>
    <SuperMenuItem Icon="fa-house" Text="Dashboard" Href="/"
                   Match="NavLinkMatch.All" />
    <SuperMenuItem Icon="fa-chart-line" Text="Analytics" Href="/analytics" />
    <SuperMenuItem Icon="fa-inbox" Text="Messages" Href="/messages"
                   BadgeText="12" BadgeCssClass="badge text-bg-danger" />
    <SuperMenuItem Icon="fa-calendar" Text="Calendar" Href="/calendar"
                   IconStyle="SuperIconStyle.Regular" />
    <SuperMenuItem Icon="fa-file-lines" Text="Documents" Href="/documents" />
    <SuperMenuItem Icon="fa-gear" Text="Settings" Href="/settings" />
</SuperSidebar>
```

Key `SuperMenuItem` parameters:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Icon` | `string?` | `null` | Font Awesome icon name |
| `IconStyle` | `SuperIconStyle` | `Configuration` | Icon style (falls back to global config) |
| `Text` | `string?` | `null` | Menu item label |
| `Href` | `string?` | `null` | Navigation URL |
| `Match` | `NavLinkMatch` | `Prefix` | URL matching mode for active state |
| `BadgeText` | `string?` | `null` | Badge text |
| `BadgeCssClass` | `string` | `"badge text-bg-success"` | CSS class for the badge |
| `Theme` | `string?` | `null` | Per-item theme class |
| `PolicyName` | `string?` | `null` | Authorization policy — hides item if unauthorized |
| `Items` | `RenderFragment?` | `null` | Submenu items |
| `ChildContent` | `RenderFragment?` | `null` | Custom content (used when `Text` is not set) |

---

### 11 — Nested Submenu Items

Create expandable submenus with the `Items` parameter.

```razor
<SuperSidebar>
    <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />

    <SuperMenuItem Icon="fa-chart-bar" Text="Reports">
        <Items>
            <SuperMenuItem Icon="fa-chart-pie" Text="Sales Report" Href="/reports/sales" />
            <SuperMenuItem Icon="fa-chart-line" Text="Traffic Report" Href="/reports/traffic" />
            <SuperMenuItem Icon="fa-file-export" Text="Export" Href="/reports/export" />
        </Items>
    </SuperMenuItem>

    <SuperMenuItem Icon="fa-sliders" Text="Administration">
        <Items>
            <SuperMenuItem Icon="fa-users" Text="Users" Href="/admin/users" />
            <SuperMenuItem Icon="fa-shield" Text="Roles" Href="/admin/roles" />
            <SuperMenuItem Icon="fa-database" Text="Database" Href="/admin/database" />
        </Items>
    </SuperMenuItem>
</SuperSidebar>
```

Submenus automatically collapse when the sidebar is in `Collapsed` or `Hidden` state. The parent item displays a chevron indicator.

---

### 12 — Policy-Based Menu Item Visibility

Use the `PolicyName` parameter to show menu items only to authorized users. This leverages Blazor's `AuthorizeView` component internally.

```razor
<SuperSidebar>
    <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    <SuperMenuItem Icon="fa-chart-bar" Text="Dashboard" Href="/dashboard" />

    @* Only visible to users with the "Admin" policy *@
    <SuperMenuItem Icon="fa-shield" Text="Administration"
                   Href="/admin"
                   PolicyName="Admin" />

    @* Only visible to users with the "Manager" policy *@
    <SuperMenuItem Icon="fa-users-gear" Text="Team Management"
                   Href="/team"
                   PolicyName="Manager" />
</SuperSidebar>
```

---

### 13 — Body with Custom Background and Padding

Customize the body area with padding, background color, and a centered container.

```razor
<SuperBody Padding="24"
           BackgroundColor="var(--bs-tertiary-bg)"
           Fluid="false">
    <div class="card">
        <div class="card-body">
            <h5 class="card-title">Centered Content</h5>
            <p class="card-text">The body uses a Bootstrap <code>container</code> (not fluid)
               with padding and a custom background color.</p>
        </div>
    </div>
</SuperBody>
```

---

### 14 — Non-Sticky Header and Footer

Disable sticky behavior so header and footer scroll with the content.

```razor
<SuperLayout>
    <SuperHeader BrandText="Scrollable App" Sticky="false" />
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        <p>The header and footer scroll with the content instead of staying fixed.</p>
        @for (int i = 0; i < 100; i++)
        {
            <p>Content line @i</p>
        }
    </SuperBody>
    <SuperFooter Sticky="false">
        <span>End of page</span>
    </SuperFooter>
</SuperLayout>
```

---

### 15 — Theme Toggle in Header

Add a dark/light theme toggle button in the header.

```razor
@using SuperBlazorComponents.Components.Themes

<SuperLayout>
    <SuperHeader BrandText="Themed App">
        <EndContent>
            <ThemeToggle />
        </EndContent>
    </SuperHeader>
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        <p>Click the sun/moon icon in the header to toggle the theme.</p>
    </SuperBody>
</SuperLayout>
```

The entire layout automatically respects Bootstrap's `data-bs-theme` attribute, so all components adapt to dark/light mode without any additional configuration.

---

### 16 — Complete Enterprise Application Layout

A production-ready layout combining all features.

```razor
@using SuperBlazorComponents.Components.SuperLayout
@using SuperBlazorComponents.Components
@using SuperBlazorComponents.Components.Themes

<SuperLayout @ref="_layout"
             SidebarWidth="260"
             SidebarCollapsedWidth="50"
             ChatPanelWidth="400"
             CssClass="my-app-layout">
    <SuperHeader Height="56">
        <Brand>
            <a href="/" class="d-flex align-items-center text-decoration-none">
                <img src="/logo.svg" alt="Acme" height="28" class="me-2" />
                <span class="fw-semibold">Acme Platform</span>
            </a>
        </Brand>
        <EndContent>
            <div class="d-flex align-items-center gap-3">
                <SuperChatButton Icon="fa-robot" Tooltip="AI Assistant" />
                <ThemeToggle />
                <span class="text-body-secondary">
                    <i class="fa-solid fa-circle-user fa-lg"></i>
                </span>
            </div>
        </EndContent>
    </SuperHeader>

    <SuperSidebar Theme="software">
        <Header>
            <div class="text-center py-2">
                <small class="text-muted">NAVIGATION</small>
            </div>
        </Header>

        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" Match="NavLinkMatch.All" />
        <SuperMenuItem Icon="fa-chart-line" Text="Analytics" Href="/analytics" />

        <SuperMenuItem Icon="fa-database" Text="Data">
            <Items>
                <SuperMenuItem Icon="fa-table" Text="Tables" Href="/data/tables" />
                <SuperMenuItem Icon="fa-file-import" Text="Import" Href="/data/import" />
                <SuperMenuItem Icon="fa-file-export" Text="Export" Href="/data/export" />
            </Items>
        </SuperMenuItem>

        <SuperMenuItem Icon="fa-inbox" Text="Messages" Href="/messages"
                       BadgeText="3" BadgeCssClass="badge text-bg-danger" />
        <SuperMenuItem Icon="fa-users" Text="Team" Href="/team" />

        <SuperMenuItem Icon="fa-shield" Text="Admin" PolicyName="Admin">
            <Items>
                <SuperMenuItem Icon="fa-users-gear" Text="Users" Href="/admin/users" />
                <SuperMenuItem Icon="fa-key" Text="Roles" Href="/admin/roles" />
                <SuperMenuItem Icon="fa-scroll" Text="Logs" Href="/admin/logs" />
            </Items>
        </SuperMenuItem>

        <Footer>
            <div class="text-center small opacity-75 py-2">
                <i class="fa-solid fa-circle-info me-1"></i> Help & Docs
            </div>
        </Footer>
    </SuperSidebar>

    <SuperBody Padding="16">
        @Body
    </SuperBody>

    <SuperChat Title="AI Assistant">
        <Header>
            <div class="d-flex align-items-center justify-content-between w-100 px-3 py-2">
                <div>
                    <i class="fa-solid fa-robot me-2 text-primary"></i>
                    <strong>AI Assistant</strong>
                    <span class="badge text-bg-success ms-2">Online</span>
                </div>
                <button class="btn btn-sm btn-link text-body-secondary" @onclick="CloseChat">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>
        </Header>

        <div class="p-2">
            <p class="text-muted">Hello! How can I help you today?</p>
        </div>

        <Footer>
            <div class="input-group">
                <input type="text" class="form-control form-control-sm"
                       placeholder="Ask a question..." />
                <button class="btn btn-primary btn-sm">
                    <i class="fa-solid fa-paper-plane"></i>
                </button>
            </div>
        </Footer>
    </SuperChat>

    <SuperFooter Height="40">
        <div class="d-flex justify-content-between w-100 small text-muted">
            <span>© 2025 Acme Corp</span>
            <span>v2.1.0</span>
        </div>
    </SuperFooter>
</SuperLayout>

@code {
    private SuperLayout? _layout;

    private void CloseChat() => _layout?.SetChatPanelState(ChatState.Hidden);
}
```

---

## CSS Custom Properties

SuperLayout exposes CSS custom properties that can be overridden for global customization:

| Property | Default | Description |
|---|---|---|
| `--super-sidebar-width` | Set via `SidebarWidth` parameter | Sidebar expanded width |
| `--super-sidebar-collapsed-width` | Set via `SidebarCollapsedWidth` parameter | Sidebar collapsed width |
| `--super-chatpanel-width` | Set via `ChatPanelWidth` parameter | Chat panel width |
| `--super-header-height` | `50px` | Header height |
| `--super-footer-height` | `48px` | Footer height |
| `--super-transition-speed` | `0.3s` | Animation duration for all transitions |

These properties are set as inline styles on the root `.super-layout` element and are inherited by all child components.

**Override example:**

```css
/* Slow down all layout transitions */
.super-layout {
    --super-transition-speed: 0.5s;
}
```

---

## Tips & Best Practices

### Layout as a Blazor Layout Component

Use `SuperLayout` as your app's base layout by wrapping it in a `.razor` layout file:

```razor
@inherits LayoutComponentBase
@using SuperBlazorComponents.Components.SuperLayout
@using SuperBlazorComponents.Components

<SuperLayout>
    <SuperHeader BrandText="My App" />
    <SuperSidebar>
        <SuperMenuItem Icon="fa-house" Text="Home" Href="/" />
    </SuperSidebar>
    <SuperBody>
        @Body
    </SuperBody>
    <SuperFooter>
        <span>© 2025</span>
    </SuperFooter>
</SuperLayout>
```

### Component Order

Child components can be placed in any order inside `<SuperLayout>`. The CSS Grid `grid-area` property ensures each component renders in the correct position regardless of declaration order.

### Sidebar Toggle Cycle

The default `ToggleSidebar()` cycles through three states: **Expanded → Collapsed → Hidden → Expanded**. If you only want two states (e.g. Expanded ↔ Collapsed), use `SetSidebarState()` directly:

```csharp
private void ToggleTwoState()
{
    var newState = _layout!.SidebarState == SidebarState.Expanded
        ? SidebarState.Collapsed
        : SidebarState.Expanded;
    _layout.SetSidebarState(newState);
}
```

### Mobile Responsiveness

- **Tablets (< 992px):** Sidebar automatically reduces to `40px` (icons only).
- **Phones (< 576px):** Sidebar is completely hidden. Consider adding a hamburger menu or drawer pattern.
- **Chat panel on phones:** Opens as a full-screen overlay to maximize usable space.

No additional configuration is needed — the responsive behavior is built into the CSS.

### Dark Mode

The entire layout uses Bootstrap 5.3 CSS variables (`--bs-body-bg`, `--bs-body-color`, `--bs-border-color`), so it automatically adapts when the `data-bs-theme` attribute changes. Combine with the `ThemeToggle` component for one-click dark/light switching.

### Performance

- The layout uses a single `CascadingValue` for parent-child communication — minimal overhead.
- State changes trigger `StateHasChanged()` only on affected components (sidebar subscribes to sidebar events, chat subscribes to chat events).
- The JS module (`SuperLayout.razor.js`) is loaded lazily on first render and provides device info once.

### Creating Custom Sidebar Themes

1. Create a CSS file following the naming convention `super-theme-{name}.css`:

```css
[data-sc-theme="mytheme"] {
    --bs-body-bg: #1a1a2e;
    --bs-body-color: #e0e0e0;
    --bs-border-color: rgba(255, 255, 255, 0.1);
    --bs-tertiary-bg: rgba(255, 255, 255, 0.05);
    --bs-primary: #e94560;
    --bs-primary-rgb: 233, 69, 96;
}
```

2. Place it in `wwwroot/css/` and reference it in your sidebar:

```razor
<SuperSidebar Theme="mytheme">
    ...
</SuperSidebar>
```

The sidebar automatically loads the theme CSS file via `<HeadContent>` and applies the `data-sc-theme` attribute.
