# SuperSplitter - Restauration automatique lors des changements d'URL

## Changements implémentés

### 1. Détection des changements d'URL
Le composant `SuperSplitter` s'abonne maintenant à l'événement `NavigationManager.LocationChanged` pour détecter automatiquement les changements de page.

### 2. Restauration automatique des positions
À chaque changement d'URL, le composant :
- Réinitialise le flag `_restoredFromStorage`
- Restaure la position sauvegardée depuis `localStorage` pour la nouvelle URL
- Applique la position avec `StateHasChanged()`

### 3. Nettoyage approprié
Le composant se désabonne de l'événement lors du `DisposeAsync()` pour éviter les fuites mémoire.

## Code implémenté

### Abonnement à LocationChanged
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await TryRestoreSizeAsync();

        dotNetRef = DotNetObjectReference.Create(this);
        jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SuperBlazorComponents/Components/SuperSplitter/SuperSplitter.razor.js");

        if (jsModule != null)
        {
            await jsModule.InvokeVoidAsync("initSplitter", splitterContainer, dotNetRef, Orientation.ToString().ToLower());
        }

        // ✨ NOUVEAU : Abonnement aux changements d'URL
        NavigationManager.LocationChanged += OnLocationChanged;
    }
}
```

### Handler de changement d'URL
```csharp
private async void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
{
    // Réinitialiser le flag pour permettre une nouvelle restauration
    _restoredFromStorage = false;
    
    // Restaurer la position pour la nouvelle URL
    await TryRestoreSizeAsync();
}
```

### Désabonnement lors du Dispose
```csharp
public async ValueTask DisposeAsync()
{
    // ✨ NOUVEAU : Désabonnement pour éviter les fuites mémoire
    NavigationManager.LocationChanged -= OnLocationChanged;

    if (jsModule != null)
    {
        await jsModule.InvokeVoidAsync("disposeSplitter", splitterContainer);
        await jsModule.DisposeAsync();
    }
    dotNetRef?.Dispose();
}
```

## Comportement utilisateur

### Scénario 1 : Navigation entre pages
1. L'utilisateur est sur `/customers` et redimensionne le splitter à 30%
2. Navigation vers `/products` → le splitter se restaure à sa position pour `/products`
3. Navigation retour vers `/customers` → le splitter revient automatiquement à 30%

### Scénario 2 : Même layout, différentes URLs
```razor
<!-- ListMainLayout.razor utilisé par plusieurs pages -->
<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal" FirstPaneSize="60">
    <SplitPane>
        <main>@Body</main>
    </SplitPane>
    <SplitPane>
        <div><SectionOutlet SectionName="EntityContext" /></div>
    </SplitPane>
</SuperSplitter>
```

- `/customers` → Position A sauvegardée et restaurée
- `/products` → Position B sauvegardée et restaurée
- `/invoices` → Position C sauvegardée et restaurée

Chaque page conserve sa propre position !

### Scénario 3 : Clé personnalisée pour partager la position
```razor
<SuperSplitter PersistenceKey="shared-layout">
    <SplitPane>...</SplitPane>
    <SplitPane>...</SplitPane>
</SuperSplitter>
```

Toutes les pages utilisant cette clé partagent la même position.

## Clés de stockage dans localStorage

### Format par défaut
```
SuperBlazorComponents.Components.SuperSplitter:{URL}:SuperSplitter
```

### Exemples
```javascript
// Page /customers
"SuperBlazorComponents.Components.SuperSplitter:/customers:SuperSplitter" → "30"

// Page /products
"SuperBlazorComponents.Components.SuperSplitter:/products:SuperSplitter" → "45"

// Page /invoices
"SuperBlazorComponents.Components.SuperSplitter:/invoices:SuperSplitter" → "60"
```

### Avec clé personnalisée
```javascript
"SuperBlazorComponents.Components.SuperSplitter:my-custom-key" → "50"
```

## Avantages

✅ **Expérience utilisateur améliorée** : Les préférences de taille sont conservées par page  
✅ **Aucune action requise** : Fonctionne automatiquement avec `EnableStatePersistence="true"` (défaut)  
✅ **Performance** : Restauration instantanée depuis localStorage  
✅ **Flexibilité** : Possibilité de désactiver ou de partager les positions entre pages  
✅ **Pas de fuites mémoire** : Désabonnement correct dans `DisposeAsync()`

## Migration

### Aucun changement requis !
Si vous utilisez déjà `SuperSplitter` avec `EnableStatePersistence="true"` (valeur par défaut), la restauration automatique lors des changements d'URL fonctionne immédiatement.

### Exemple existant
```razor
<!-- Avant : Fonctionne déjà ! -->
<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal" FirstPaneSize="60">
    <SplitPane>
        <main>@Body</main>
    </SplitPane>
    <SplitPane>
        <div>Contexte</div>
    </SplitPane>
</SuperSplitter>

<!-- Maintenant avec restauration automatique sur changement d'URL ✨ -->
```

## Désactivation si nécessaire

Pour désactiver complètement la persistance :
```razor
<SuperSplitter EnableStatePersistence="false">
    <SplitPane>...</SplitPane>
    <SplitPane>...</SplitPane>
</SuperSplitter>
```

## Tests

### Test 1 : Vérifier la restauration
1. Ouvrir `/customers`
2. Redimensionner le splitter à 25%
3. Naviguer vers `/products`
4. Revenir à `/customers`
5. ✅ Vérifier que le splitter est à 25%

### Test 2 : Vérifier l'indépendance des URLs
1. `/customers` → redimensionner à 30%
2. `/products` → redimensionner à 70%
3. Naviguer entre les deux pages
4. ✅ Chaque page conserve sa propre taille

### Test 3 : Vérifier la clé personnalisée
1. Utiliser `PersistenceKey="shared"` sur plusieurs pages
2. Redimensionner sur une page
3. Naviguer vers une autre page avec la même clé
4. ✅ La taille est partagée entre les pages

## Conventions respectées

✅ Abonnement à `LocationChanged` uniquement dans `OnAfterRenderAsync(firstRender)`  
✅ Désabonnement dans `DisposeAsync()` pour éviter les fuites  
✅ Utilisation du flag `_restoredFromStorage` pour contrôler la restauration  
✅ Gestion des exceptions `JSException` pour les environnements sans localStorage  
✅ Handler `async void` approprié pour un event handler  
✅ Documentation complète avec exemples
