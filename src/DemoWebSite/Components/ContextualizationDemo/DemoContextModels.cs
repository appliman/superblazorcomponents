namespace DemoWebSite.Components.ContextualizationDemo;

public sealed record DemoCustomer(int Id, string Name, string Email, string Status);

public sealed record DemoInvoice(int Id, string Number, decimal Amount, string Status);
