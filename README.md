# 🚀 SuperBlazorComponents

[![NuGet](https://img.shields.io/nuget/v/SuperBlazorComponents)](https://www.nuget.org/packages/SuperBlazorComponents)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-purple)](https://dotnet.microsoft.com)

**High-performance, open-source Blazor component library** for admin and line-of-business applications — built with Bootstrap 5.3 and zero third-party JS dependencies (except Google Charts).

---

## ✨ Components

| Component | Description | Docs |
|---|---|---|
| **SuperDataGrid** | Virtualized data grid — frozen columns/rows, reordering, resizing, filtering, sorting, inline editing, row selection, settings persistence | [📖 SUPERDATAGRID.md](SUPERDATAGRID.md) |
| **SuperLayout** | Responsive app layout — header, sidebar, body, footer, chat panel with collapsible sidebar | |
| **SuperTabs** | Dynamic tabbed interface — badges, closable tabs, lazy loading, persistence (URL + localStorage), keyboard navigation, service-driven management | |
| **SuperSplitter** | Resizable split panels — horizontal/vertical, collapsible, state persistence | [📖 SuperSplitter.md](src/SuperBlazorComponents/Components/SuperSplitter/SuperSplitter.md) |
| **SuperDateRangePicker** | Calendar-based date range picker with presets | |
| **SuperButtons** | Buttons, split buttons, toggle buttons, link buttons, confirmation buttons | |
| **SuperDialog** | Modal dialog system with dynamic component rendering | |
| **SuperConfirmDialog** | Confirmation dialog with customizable buttons | |
| **SuperNotifications** | Toast notifications with auto-dismiss and severity levels | |
| **SuperBreadCrumb** | Breadcrumb navigation with back-navigation support | |
| **SuperMenuItem** | Sidebar menu items with icons, badges, and nested submenus | |
| **ThemeToggle** | Dark/light theme toggle with system preference detection and localStorage persistence | |
| **Google Charts** | Combo charts, pie charts, and pure SVG time series charts | |

---

## 📦 Installation

```bash
dotnet add package SuperBlazorComponents
```

## 🔧 Setup

Register the services in your `Program.cs`:

```csharp
builder.Services.AddSuperComponents();
```

---

## 🚀 Quick Start

```razor
@using SuperBlazorComponents.Components.SuperDataGrid

<SuperDataGrid TItem="Product"
               ItemsProvider="LoadProducts"
               Height="500px"
               AllowSorting="true"
               AllowFiltering="true"
               FreezeHeader="true"
               SelectionMode="SuperDataGridSelectionMode.Multiple"
               GridId="products-grid">
    <DataGridColumn Title="Name" For="@(c => c.Name)" />
    <DataGridColumn Title="Price" For="@(c => c.Price)" Width="120" />
    <DataGridColumn Title="Category" For="@(c => c.Category)" Width="150" />
</SuperDataGrid>
```

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues, suggest features, or submit pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🌐 Live Demo

A live demo site is available at **[blazor.appliman.com](https://blazor.appliman.com/)**

---

## 🔗 Links

- **Live Demo:** [blazor.appliman.com](https://blazor.appliman.com/)
- **GitHub:** [github.com/appliman/superblazorcomponents](https://github.com/appliman/superblazorcomponents)
- **NuGet:** [nuget.org/packages/SuperBlazorComponents](https://www.nuget.org/packages/SuperBlazorComponents)
