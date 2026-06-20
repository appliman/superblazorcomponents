namespace SuperBlazorComponents.Components.Contextualization;

public interface ISuperContextRefreshable
{
	Task RefreshFromContextAsync(object context);
}
