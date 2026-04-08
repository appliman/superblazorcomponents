using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SuperBlazorComponents.Components;
using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Components.SuperTabs;

public partial class SuperTabs
{
	[Inject]
	private SuperComponentsConfiguration SuperComponentsConfiguration { get; set; } = default!;

	private ElementReference tabsContainer;
	private int selectedIndex;
	private int previousSelectedIndex = -1;

	/// <summary>
	/// Identifiant unique de l'instance (pour le service)
	/// </summary>
	[Parameter]
	public string InstanceId { get; set; } = Guid.NewGuid().ToString();

	/// <summary>
	/// Liste des onglets
	/// </summary>
	[Parameter]
	public List<SuperTabItem> Tabs { get; set; } = new();

	/// <summary>
	/// Index de l'onglet sélectionné
	/// </summary>
	[Parameter]
	public int SelectedIndex { get; set; }

	/// <summary>
	/// Callback pour le binding bidirectionnel de l'index sélectionné
	/// </summary>
	[Parameter]
	public EventCallback<int> SelectedIndexChanged { get; set; }

	/// <summary>
	/// Position des onglets
	/// </summary>
	[Parameter]
	public SuperTabPosition Position { get; set; } = SuperTabPosition.Top;

	/// <summary>
	/// Style d'icône par défaut appliqué aux onglets lorsque l'onglet ne force pas son style.
	/// </summary>
	[Parameter]
	public SuperIconStyle SuperIconStyle { get; set; } = SuperIconStyle.Configuration;

	/// <summary>
	/// Largeur de la colonne des onglets en mode vertical (Left/Right) (ex: "220px", "16rem").
	/// </summary>
	[Parameter]
	public string? LeftHeaderWidth { get; set; }

	/// <summary>
	/// Permet d'ajouter des onglets dynamiquement
	/// </summary>
	[Parameter]
	public bool AllowAddTab { get; set; }

	/// <summary>
	/// Événement déclenché avant le changement d'onglet (permet d'annuler)
	/// </summary>
	[Parameter]
	public EventCallback<SuperTabChangeEventArgs> OnTabChanging { get; set; }

	/// <summary>
	/// Événement déclenché après le changement d'onglet
	/// </summary>
	[Parameter]
	public EventCallback<SuperTabChangeEventArgs> OnTabChanged { get; set; }

	/// <summary>
	/// Événement déclenché lors de la fermeture d'un onglet
	/// </summary>
	[Parameter]
	public EventCallback<SuperTabCloseEventArgs> OnTabClosing { get; set; }

	/// <summary>
	/// Événement déclenché après la fermeture d'un onglet
	/// </summary>
	[Parameter]
	public EventCallback<SuperTabCloseEventArgs> OnTabClosed { get; set; }

	/// <summary>
	/// Événement déclenché lors de l'ajout d'un onglet
	/// </summary>
	[Parameter]
	public EventCallback OnAddTabClicked { get; set; }

	/// <summary>
	/// Clé pour la persistance de l'onglet sélectionné dans localStorage
	/// </summary>
	[Parameter]
	public string? PersistenceKey { get; set; }

	/// <summary>
	/// Persistance de l'onglet dans l'URL
	/// </summary>
	[Parameter]
	public bool PersistInUrl { get; set; }

	/// <summary>
	/// Hauteur du composant (ex: "100%", "500px")
	/// </summary>
	[Parameter]
	public string Height { get; set; } = "100%";

	/// <summary>
	/// Contenu enfant.
	/// Peut être utilisé pour la composition déclarative: <SuperTabs><Tabs><TabItem>...</TabItem></Tabs></SuperTabs>
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	internal void RegisterDeclarativeTab(SuperTabItem tab)
	{
		TabsService.AddOrUpdateTabFromDeclarative(InstanceId, tab);
	}

	/// <summary>
	/// Animations activées
	/// </summary>
	[Parameter]
	public bool EnableAnimations { get; set; } = true;

	/// <summary>
	/// Navigation au clavier activée (désactivée par défaut)
	/// </summary>
	[Parameter]
	public bool EnableKeyboardNavigation { get; set; } = false;

	protected override async Task OnInitializedAsync()
	{
		TabsService.SelectTabRequested += HandleSelectTabRequested;
		TabsService.AddTabRequested += HandleAddTabRequested;
		TabsService.RemoveTabRequested += HandleRemoveTabRequested;
		TabsService.BadgeUpdateRequested += HandleBadgeUpdateRequested;
		TabsService.ResetRequested += HandleResetRequested;

		TabsService.RegisterInstance(InstanceId, new SuperTabsInstance
		{
			Tabs = Tabs,
			SelectedIndex = SelectedIndex,
			RefreshCallback = () => InvokeAsync(StateHasChanged)
		});
	}

	protected override async Task OnParametersSetAsync()
	{
		if (previousSelectedIndex != SelectedIndex)
		{
			selectedIndex = SelectedIndex;
			previousSelectedIndex = SelectedIndex;
		}

		await base.OnParametersSetAsync();
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && PersistInUrl)
		{
			await RestorePersistedTabAsync();

			var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
			var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
			var tabParam = query["tab"];

			if (!string.IsNullOrEmpty(tabParam) && int.TryParse(tabParam, out var tabIndex))
			{
				await SelectTabAsync(tabIndex);
			}
		}
	}

	/// <summary>
	/// Sélectionne un onglet par son index
	/// </summary>
	public async Task SelectTabAsync(int index)
	{
		if (index < 0 || index >= Tabs.Count)
		{
			return;
		}

		var tab = Tabs[index];
		if (tab.Disabled || !tab.Visible)
		{
			return;
		}

		var previousIndex = selectedIndex;
		var previousTab = previousIndex >= 0 && previousIndex < Tabs.Count ? Tabs[previousIndex] : null;

		if (previousTab?.HasUnsavedChanges == true && !string.IsNullOrEmpty(previousTab.ConfirmLeaveMessage))
		{
			var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", previousTab.ConfirmLeaveMessage);
			if (!confirmed)
			{
				return;
			}
		}

		var changingArgs = new SuperTabChangeEventArgs
		{
			PreviousIndex = previousIndex,
			NewIndex = index,
			PreviousTab = previousTab,
			NewTab = tab
		};

		await OnTabChanging.InvokeAsync(changingArgs);

		if (changingArgs.Cancel)
		{
			return;
		}

		tab.HasBeenLoaded = true;
		await UpdateSelectedIndex(index);
		TabsService.NotifyTabChanged(InstanceId, tab.Title, index);
		await PersistSelectedTabAsync();
		await OnTabChanged.InvokeAsync(changingArgs);
	}

	/// <summary>
	/// Sélectionne un onglet par son ID
	/// </summary>
	public async Task SelectTabByIdAsync(string id)
	{
		var index = Tabs.FindIndex(t => t.Id == id);
		if (index >= 0)
		{
			await SelectTabAsync(index);
		}
	}

	/// <summary>
	/// Sélectionne un onglet par son titre
	/// </summary>
	public async Task SelectTabByTitleAsync(string title)
	{
		var index = Tabs.FindIndex(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
		if (index >= 0)
		{
			await SelectTabAsync(index);
		}
	}

	/// <summary>
	/// Ajoute un nouvel onglet
	/// </summary>
	public async Task AddTabAsync(SuperTabItem tab)
	{
		Tabs.Add(tab);
		StateHasChanged();
		await SelectTabAsync(Tabs.Count - 1);
	}

	/// <summary>
	/// Supprime un onglet par son index
	/// </summary>
	public async Task RemoveTabAsync(int index)
	{
		if (index < 0 || index >= Tabs.Count)
		{
			return;
		}

		var tab = Tabs[index];

		var closingArgs = new SuperTabCloseEventArgs
		{
			Tab = tab,
			Index = index
		};

		await OnTabClosing.InvokeAsync(closingArgs);

		if (closingArgs.Cancel)
		{
			return;
		}

		Tabs.RemoveAt(index);

		if (selectedIndex >= Tabs.Count)
		{
			await UpdateSelectedIndex(Math.Max(0, Tabs.Count - 1));
		}
		else if (selectedIndex > index)
		{
			await UpdateSelectedIndex(selectedIndex - 1);
		}

		StateHasChanged();

		await OnTabClosed.InvokeAsync(closingArgs);
	}

	/// <summary>
	/// Met à jour le badge d'un onglet
	/// </summary>
	public void UpdateBadge(int index, string? text, string? badgeClass = null, string? badgeIcon = null)
	{
		if (index >= 0 && index < Tabs.Count)
		{
			Tabs[index].BadgeText = text;
			if (badgeClass is not null)
			{
				Tabs[index].BadgeClass = badgeClass;
			}
			if (badgeIcon is not null)
			{
				Tabs[index].BadgeIcon = badgeIcon;
			}
			StateHasChanged();
		}
	}

	/// <summary>
	/// Met à jour le badge d'un onglet par son ID
	/// </summary>
	public void UpdateBadgeById(string id, string? text, string? badgeClass = null, string? badgeIcon = null)
	{
		var index = Tabs.FindIndex(t => t.Id == id);
		UpdateBadge(index, text, badgeClass, badgeIcon);
	}

	/// <summary>
	/// Obtient l'onglet actuellement sélectionné
	/// </summary>
	public SuperTabItem? GetSelectedTab()
	{
		return selectedIndex >= 0 && selectedIndex < Tabs.Count ? Tabs[selectedIndex] : null;
	}

	private async Task CloseTabAsync(int index)
	{
		var tab = Tabs[index];

		if (tab.HasUnsavedChanges && !string.IsNullOrEmpty(tab.ConfirmLeaveMessage))
		{
			var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", tab.ConfirmLeaveMessage);
			if (!confirmed)
			{
				return;
			}
		}

		await RemoveTabAsync(index);
	}

	private async Task RequestAddTabAsync()
	{
		await OnAddTabClicked.InvokeAsync();
	}

	private void HandleKeyDown(KeyboardEventArgs e)
	{
		if (!EnableKeyboardNavigation)
		{
			return;
		}

		var visibleTabs = GetVisibleTabs().ToList();
		if (visibleTabs.Count == 0)
		{
			return;
		}

		var currentVisibleIndex = visibleTabs.FindIndex(t => GetActualIndex(t) == SelectedIndex);

		switch (e.Key)
		{
			case "ArrowLeft":
			case "ArrowUp":
				NavigateToPreviousTab(visibleTabs, currentVisibleIndex);
				break;
			case "ArrowRight":
			case "ArrowDown":
				NavigateToNextTab(visibleTabs, currentVisibleIndex);
				break;
			case "Home":
				_ = SelectTabAsync(GetActualIndex(visibleTabs.First()));
				break;
			case "End":
				_ = SelectTabAsync(GetActualIndex(visibleTabs.Last()));
				break;
		}

		if (e.CtrlKey && e.Key.Length == 1 && char.IsDigit(e.Key[0]))
		{
			var tabNumber = int.Parse(e.Key.ToString()) - 1;
			if (tabNumber >= 0 && tabNumber < visibleTabs.Count)
			{
				_ = SelectTabAsync(GetActualIndex(visibleTabs[tabNumber]));
			}
		}
	}

	private void NavigateToPreviousTab(List<SuperTabItem> visibleTabs, int currentIndex)
	{
		for (var i = currentIndex - 1; i >= 0; i--)
		{
			if (!visibleTabs[i].Disabled)
			{
				_ = SelectTabAsync(GetActualIndex(visibleTabs[i]));
				return;
			}
		}
	}

	private void NavigateToNextTab(List<SuperTabItem> visibleTabs, int currentIndex)
	{
		for (var i = currentIndex + 1; i < visibleTabs.Count; i++)
		{
			if (!visibleTabs[i].Disabled)
			{
				_ = SelectTabAsync(GetActualIndex(visibleTabs[i]));
				return;
			}
		}
	}

	private IEnumerable<SuperTabItem> GetVisibleTabs()
	{
		return Tabs.Where(t => t.Visible).OrderBy(t => t.Order);
	}

	private int GetActualIndex(SuperTabItem tab)
	{
		return Tabs.IndexOf(tab);
	}

	private string GetPositionClass()
	{
		return Position switch
		{
			SuperTabPosition.Top => "tabs-top",
			SuperTabPosition.TopRight => "tabs-top-right",
			SuperTabPosition.Bottom => "tabs-bottom",
			SuperTabPosition.BottomRight => "tabs-bottom-right",
			SuperTabPosition.Left => "tabs-left",
			SuperTabPosition.Right => "tabs-right",
			_ => "tabs-top"
		};
	}

	private string GetHeaderPositionClass()
	{
		return Position switch
		{
			SuperTabPosition.TopRight or SuperTabPosition.BottomRight => "justify-end",
			_ => ""
		};
	}

	private string GetIconStyle(SuperTabItem tab)
	{
		return !string.IsNullOrEmpty(tab.IconColor) ? $"color: {tab.IconColor};" : "";
	}

	private string GetIconCssClass(SuperTabItem tab)
	{
		var stylePrefix = ResolveIconStyle(tab) switch
		{
			SuperIconStyle.Regular => "fa-regular",
			SuperIconStyle.Brands => "fa-brands",
			SuperIconStyle.Duotone => "fa-duotone",
			_ => "fa-solid"
		};

		return $"{stylePrefix} {tab.Icon} super-tab-icon";
	}

	private SuperIconStyle ResolveIconStyle(SuperTabItem tab)
	{
		var resolvedStyle = tab.SuperIconStyle == SuperIconStyle.Configuration
			? SuperIconStyle
			: tab.SuperIconStyle;

		if (resolvedStyle == SuperIconStyle.Configuration)
		{
			resolvedStyle = SuperComponentsConfiguration.DefaultSuperIconeStyle;
		}

		return resolvedStyle == SuperIconStyle.Configuration
			? SuperIconStyle.Solid
			: resolvedStyle;
	}

	private string GetHeaderContainerStyle()
	{
		if (string.IsNullOrWhiteSpace(LeftHeaderWidth))
		{
			return string.Empty;
		}

		if (Position is SuperTabPosition.Left or SuperTabPosition.Right)
		{
			return $"min-width: {LeftHeaderWidth}; width: {LeftHeaderWidth}; flex: 0 0 {LeftHeaderWidth};";
		}

		return string.Empty;
	}

	private async Task PersistSelectedTabAsync()
	{
		if (!string.IsNullOrEmpty(PersistenceKey))
		{
			try
			{
				await JSRuntime.InvokeVoidAsync("localStorage.setItem", PersistenceKey, selectedIndex.ToString());
			}
			catch
			{
				// Ignorer les erreurs de localStorage
			}
		}

		if (PersistInUrl)
		{
			try
			{
				var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
				var newUrl = $"{uri.GetLeftPart(UriPartial.Path)}?tab={selectedIndex}";
				await JSRuntime.InvokeVoidAsync("history.replaceState", null, "", newUrl);
			}
			catch
			{
				// Ignorer les erreurs
			}
		}
	}

	private async Task RestorePersistedTabAsync()
	{
		if (!string.IsNullOrEmpty(PersistenceKey))
		{
			try
			{
				var storedValue = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", PersistenceKey);
				if (!string.IsNullOrEmpty(storedValue) && int.TryParse(storedValue, out var index))
				{
					if (index >= 0 && index < Tabs.Count)
					{
						selectedIndex = index;
					}
				}
			}
			catch
			{
				// Ignorer les erreurs de localStorage (ex: SSR)
			}
		}
	}

	private void HandleSelectTabRequested(object? sender, SuperTabSelectRequestEventArgs e)
	{
		if (e.InstanceId is not null && e.InstanceId != InstanceId)
		{
			return;
		}

		if (e.Index.HasValue)
		{
			_ = SelectTabAsync(e.Index.Value);
		}
		else if (!string.IsNullOrEmpty(e.Title))
		{
			_ = SelectTabByTitleAsync(e.Title);
		}
		else if (!string.IsNullOrEmpty(e.TabId))
		{
			_ = SelectTabByIdAsync(e.TabId);
		}
	}

	private void HandleAddTabRequested(object? sender, SuperTabAddRequestEventArgs e)
	{
		if (e.InstanceId != InstanceId)
		{
			return;
		}

		Tabs.Add(e.Tab);
		StateHasChanged();

		if (e.SelectAfterAdd)
		{
			_ = SelectTabAsync(Tabs.Count - 1);
		}
	}

	private void HandleRemoveTabRequested(object? sender, SuperTabRemoveRequestEventArgs e)
	{
		if (e.InstanceId != InstanceId)
		{
			return;
		}

		if (e.Index.HasValue)
		{
			_ = RemoveTabAsync(e.Index.Value);
		}
		else if (!string.IsNullOrEmpty(e.TabId))
		{
			var index = Tabs.FindIndex(t => t.Id == e.TabId);
			if (index >= 0)
			{
				_ = RemoveTabAsync(index);
			}
		}
	}

	private void HandleBadgeUpdateRequested(object? sender, SuperTabBadgeUpdateEventArgs e)
	{
		if (e.InstanceId != InstanceId)
		{
			return;
		}

		if (e.Index.HasValue)
		{
			UpdateBadge(e.Index.Value, e.BadgeText, e.BadgeClass, e.BadgeIcon);
		}
		else if (!string.IsNullOrEmpty(e.TabId))
		{
			UpdateBadgeById(e.TabId, e.BadgeText, e.BadgeClass, e.BadgeIcon);
		}
	}

	private async void HandleResetRequested(object? sender, string instanceId)
	{
		if (!string.IsNullOrEmpty(instanceId) && instanceId != InstanceId)
		{
			return;
		}

		await UpdateSelectedIndex(0);
		StateHasChanged();
	}

	public void Dispose()
	{
		TabsService.SelectTabRequested -= HandleSelectTabRequested;
		TabsService.AddTabRequested -= HandleAddTabRequested;
		TabsService.RemoveTabRequested -= HandleRemoveTabRequested;
		TabsService.BadgeUpdateRequested -= HandleBadgeUpdateRequested;
		TabsService.ResetRequested -= HandleResetRequested;
		TabsService.UnregisterInstance(InstanceId);
	}

	private async Task UpdateSelectedIndex(int newIndex)
	{
		if (selectedIndex != newIndex)
		{
			selectedIndex = newIndex;
			await SelectedIndexChanged.InvokeAsync(newIndex);
		}
	}
}