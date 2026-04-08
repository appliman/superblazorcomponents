namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Service pour la communication bidirectionnelle avec le composant SuperTabs
/// Permet de contrôler les onglets depuis n'importe quel composant
/// </summary>
public class SuperTabsService
{
    private readonly Dictionary<string, SuperTabsInstance> instances = new();
    
    /// <summary>
    /// Ajoute ou met à jour un onglet à partir d'une déclaration
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tab">Onglet à ajouter ou à mettre à jour</param>
    internal void AddOrUpdateTabFromDeclarative(string instanceId, SuperTabItem tab)
    {
        if (!instances.TryGetValue(instanceId, out var instance))
        {
            return;
        }

        var existingIndex = instance.Tabs.FindIndex(t => t.Id == tab.Id);
        if (existingIndex >= 0)
        {
            instance.Tabs[existingIndex] = tab;
        }
        else
        {
            instance.Tabs.Add(tab);
        }

        instance.RefreshCallback?.Invoke();
    }

    /// <summary>
    /// Événement déclenché lors d'un changement d'onglet
    /// </summary>
    public event EventHandler<SuperTabServiceEventArgs>? TabChanged;

    /// <summary>
    /// Événement déclenché lors de la demande de sélection d'un onglet
    /// </summary>
    public event EventHandler<SuperTabSelectRequestEventArgs>? SelectTabRequested;

    /// <summary>
    /// Événement déclenché lors de l'ajout d'un onglet
    /// </summary>
    public event EventHandler<SuperTabAddRequestEventArgs>? AddTabRequested;

    /// <summary>
    /// Événement déclenché lors de la suppression d'un onglet
    /// </summary>
    public event EventHandler<SuperTabRemoveRequestEventArgs>? RemoveTabRequested;

    /// <summary>
    /// Événement déclenché lors de la mise à jour d'un badge
    /// </summary>
    public event EventHandler<SuperTabBadgeUpdateEventArgs>? BadgeUpdateRequested;

    /// <summary>
    /// Événement déclenché lors de la réinitialisation
    /// </summary>
    public event EventHandler<string>? ResetRequested;

    /// <summary>
    /// Enregistre une instance de SuperTabs
    /// </summary>
    /// <param name="instanceId">Identifiant unique de l'instance</param>
    /// <param name="instance">Instance du service</param>
    public void RegisterInstance(string instanceId, SuperTabsInstance instance)
    {
        instances[instanceId] = instance;
    }

    /// <summary>
    /// Désenregistre une instance de SuperTabs
    /// </summary>
    /// <param name="instanceId">Identifiant unique de l'instance</param>
    public void UnregisterInstance(string instanceId)
    {
        instances.Remove(instanceId);
    }

    /// <summary>
    /// Notifie qu'un onglet a été changé
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tabTitle">Titre de l'onglet</param>
    /// <param name="tabIndex">Index de l'onglet</param>
    public void NotifyTabChanged(string instanceId, string tabTitle, int tabIndex)
    {
        TabChanged?.Invoke(this, new SuperTabServiceEventArgs
        {
            InstanceId = instanceId,
            TabTitle = tabTitle,
            TabIndex = tabIndex
        });
    }

    /// <summary>
    /// Sélectionne un onglet par son index
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance (null pour toutes)</param>
    /// <param name="index">Index de l'onglet</param>
    public void SelectTab(string? instanceId, int index)
    {
        SelectTabRequested?.Invoke(this, new SuperTabSelectRequestEventArgs
        {
            InstanceId = instanceId,
            Index = index
        });
    }

    /// <summary>
    /// Sélectionne un onglet par son titre
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance (null pour toutes)</param>
    /// <param name="title">Titre de l'onglet</param>
    public void SelectTabByTitle(string? instanceId, string title)
    {
        SelectTabRequested?.Invoke(this, new SuperTabSelectRequestEventArgs
        {
            InstanceId = instanceId,
            Title = title
        });
    }

    /// <summary>
    /// Sélectionne un onglet par son ID
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance (null pour toutes)</param>
    /// <param name="tabId">ID de l'onglet</param>
    public void SelectTabById(string? instanceId, string tabId)
    {
        SelectTabRequested?.Invoke(this, new SuperTabSelectRequestEventArgs
        {
            InstanceId = instanceId,
            TabId = tabId
        });
    }

    /// <summary>
    /// Ajoute un nouvel onglet
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tab">Onglet à ajouter</param>
    /// <param name="selectAfterAdd">Sélectionner l'onglet après l'ajout</param>
    public void AddTab(string instanceId, SuperTabItem tab, bool selectAfterAdd = true)
    {
        AddTabRequested?.Invoke(this, new SuperTabAddRequestEventArgs
        {
            InstanceId = instanceId,
            Tab = tab,
            SelectAfterAdd = selectAfterAdd
        });
    }

    /// <summary>
    /// Supprime un onglet par son index
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="index">Index de l'onglet</param>
    public void RemoveTab(string instanceId, int index)
    {
        RemoveTabRequested?.Invoke(this, new SuperTabRemoveRequestEventArgs
        {
            InstanceId = instanceId,
            Index = index
        });
    }

    /// <summary>
    /// Supprime un onglet par son ID
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tabId">ID de l'onglet</param>
    public void RemoveTabById(string instanceId, string tabId)
    {
        RemoveTabRequested?.Invoke(this, new SuperTabRemoveRequestEventArgs
        {
            InstanceId = instanceId,
            TabId = tabId
        });
    }

    /// <summary>
    /// Met à jour le badge d'un onglet
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tabId">ID de l'onglet</param>
    /// <param name="badgeText">Texte du badge</param>
    /// <param name="badgeClass">Classe CSS du badge</param>
    /// <param name="badgeIcon">Icône du badge</param>
    public void UpdateBadge(string instanceId, string tabId, string? badgeText, string? badgeClass = null, string? badgeIcon = null)
    {
        BadgeUpdateRequested?.Invoke(this, new SuperTabBadgeUpdateEventArgs
        {
            InstanceId = instanceId,
            TabId = tabId,
            BadgeText = badgeText,
            BadgeClass = badgeClass,
            BadgeIcon = badgeIcon
        });
    }

    /// <summary>
    /// Met à jour le badge d'un onglet par son index
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="index">Index de l'onglet</param>
    /// <param name="badgeText">Texte du badge</param>
    /// <param name="badgeClass">Classe CSS du badge</param>
    public void UpdateBadgeByIndex(string instanceId, int index, string? badgeText, string? badgeClass = null)
    {
        BadgeUpdateRequested?.Invoke(this, new SuperTabBadgeUpdateEventArgs
        {
            InstanceId = instanceId,
            Index = index,
            BadgeText = badgeText,
            BadgeClass = badgeClass
        });
    }

    /// <summary>
    /// Change la visibilité d'un onglet
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tabId">ID de l'onglet</param>
    /// <param name="visible">Visibilité</param>
    public void SetTabVisibility(string instanceId, string tabId, bool visible)
    {
        if (instances.TryGetValue(instanceId, out var instance))
        {
            var tab = instance.Tabs.FirstOrDefault(t => t.Id == tabId);
            if (tab is not null)
            {
                tab.Visible = visible;
                instance.RefreshCallback?.Invoke();
            }
        }
    }

    /// <summary>
    /// Active ou désactive un onglet
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    /// <param name="tabId">ID de l'onglet</param>
    /// <param name="disabled">État désactivé</param>
    public void SetTabDisabled(string instanceId, string tabId, bool disabled)
    {
        if (instances.TryGetValue(instanceId, out var instance))
        {
            var tab = instance.Tabs.FirstOrDefault(t => t.Id == tabId);
            if (tab is not null)
            {
                tab.Disabled = disabled;
                instance.RefreshCallback?.Invoke();
            }
        }
    }

    /// <summary>
    /// Réinitialise les onglets (sélectionne le premier)
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance (null pour toutes)</param>
    public void Reset(string? instanceId = null)
    {
        ResetRequested?.Invoke(this, instanceId ?? string.Empty);
    }

    /// <summary>
    /// Obtient le titre de l'onglet actuellement sélectionné
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    public string? GetCurrentTabTitle(string instanceId)
    {
        if (instances.TryGetValue(instanceId, out var instance))
        {
            var selectedIndex = instance.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < instance.Tabs.Count)
            {
                return instance.Tabs[selectedIndex].Title;
            }
        }
        return null;
    }

    /// <summary>
    /// Obtient l'onglet actuellement sélectionné
    /// </summary>
    /// <param name="instanceId">Identifiant de l'instance</param>
    public SuperTabItem? GetCurrentTab(string instanceId)
    {
        if (instances.TryGetValue(instanceId, out var instance))
        {
            var selectedIndex = instance.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < instance.Tabs.Count)
            {
                return instance.Tabs[selectedIndex];
            }
        }
        return null;
    }
}
