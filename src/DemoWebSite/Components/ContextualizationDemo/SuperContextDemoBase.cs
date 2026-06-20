using Microsoft.AspNetCore.Components;

using SuperBlazorComponents.Components.Contextualization;

namespace DemoWebSite.Components.ContextualizationDemo;

public abstract class SuperContextDemoBase : ComponentBase
{
	protected readonly DemoCustomer[] Customers =
	[
		new(1, "Alice Martin", "alice@example.com", "Active"),
		new(2, "Bob Dupont", "bob@example.com", "Prospect"),
		new(3, "Chloé Bernard", "chloe@example.com", "Active")
	];

	protected readonly DemoInvoice[] Invoices =
	[
		new(101, "INV-2026-0101", 1290m, "Paid"),
		new(102, "INV-2026-0102", 745.50m, "Pending")
	];

	protected DemoCustomer SelectedCustomer { get; set; } = default!;
	protected DemoInvoice SelectedInvoice { get; set; } = default!;

	protected override void OnInitialized()
	{
		SelectedCustomer = Customers[0];
		SelectedInvoice = Invoices[0];
	}

	protected static string SelectedClass(bool selected) =>
		selected ? "list-group-item list-group-item-action active" : "list-group-item list-group-item-action";
}
