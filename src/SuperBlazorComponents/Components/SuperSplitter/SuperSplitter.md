# SuperSplitter avec SplitPane

## Vue d'ensemble
Le composant `SuperSplitter` permet de créer une interface divisée en deux panneaux redimensionnables avec une barre de séparation. Les panneaux sont maintenant définis via le composant `SplitPane` qui permet d'ajouter des classes CSS personnalisées.

## Composants

### SuperSplitter
Conteneur principal qui gère la logique de redimensionnement et de persistance.

### SplitPane
Composant enfant qui définit un panneau avec support de classes CSS personnalisées.

## Migration depuis l'ancienne version

### Avant (avec RenderFragment)
```razor
<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal" FirstPaneSize="30">
    <FirstPane>
        <div>Contenu du premier panneau</div>
    </FirstPane>
    <SecondPane>
        <div>Contenu du second panneau</div>
    </SecondPane>
</SuperSplitter>
```

### Après (avec SplitPane)
```razor
<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal" FirstPaneSize="30">
    <SplitPane>
        <div>Contenu du premier panneau</div>
    </SplitPane>
    <SplitPane>
        <div>Contenu du second panneau</div>
    </SplitPane>
</SuperSplitter>
```

## Utilisation avec classes CSS personnalisées

### Exemple 1 : Ajouter une classe CSS
```razor
<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal">
    <SplitPane CssClass="custom-pane-left">
        <h3>Panneau Gauche</h3>
        <p>Contenu avec style personnalisé</p>
    </SplitPane>
    <SplitPane CssClass="custom-pane-right bg-light">
        <h3>Panneau Droit</h3>
        <p>Peut contenir plusieurs classes</p>
    </SplitPane>
</SuperSplitter>

<style>
    .custom-pane-left {
        background-color: #f0f8ff;
        padding: 1rem;
    }
    
    .custom-pane-right {
        border-left: 2px solid #ddd;
    }
</style>
```

### Exemple 2 : Attributs HTML supplémentaires
```razor
<SuperSplitter>
    <SplitPane CssClass="scrollable" data-testid="left-pane">
        <div>Contenu avec attributs personnalisés</div>
    </SplitPane>
    <SplitPane id="right-panel" role="complementary">
        <div>Contenu du panneau droit</div>
    </SplitPane>
</SuperSplitter>
```

### Exemple 3 : Layout vertical avec styles
```razor
<SuperSplitter Orientation="SuperSplitterOrientation.Vertical" FirstPaneSize="40">
    <SplitPane CssClass="header-pane">
        <header>
            <h1>En-tête</h1>
        </header>
    </SplitPane>
    <SplitPane CssClass="content-pane overflow-auto">
        <main>
            <p>Contenu principal avec scroll</p>
        </main>
    </SplitPane>
</SuperSplitter>
```

## Paramètres de SuperSplitter

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `ChildContent` | `RenderFragment?` | - | Contenu contenant les 2 `SplitPane` |
| `Orientation` | `SuperSplitterOrientation` | `Horizontal` | Orientation du splitter (`Horizontal` ou `Vertical`) |
| `FirstPaneSize` | `double` | `50` | Taille du premier panneau en pourcentage (0-100) |
| `FirstPaneSizeChanged` | `EventCallback<double>` | - | Événement déclenché lors du redimensionnement |
| `MinFirstPaneSize` | `double` | `10` | Taille minimale du premier panneau en % |
| `MaxFirstPaneSize` | `double` | `90` | Taille maximale du premier panneau en % |
| `Collapsible` | `bool` | `false` | Permet de réduire complètement un panneau |
| `EnableStatePersistence` | `bool` | `true` | Sauvegarde la taille dans localStorage et restaure automatiquement à chaque changement d'URL |
| `PersistenceKey` | `string?` | - | Clé personnalisée pour la persistance |

## Persistance automatique par URL

Le composant sauvegarde et restaure automatiquement la taille des panneaux :

### Comportement
- ✅ **Sauvegarde automatique** : À chaque redimensionnement, la taille est sauvegardée dans `localStorage`
- ✅ **Restauration au chargement** : La taille est restaurée lors du premier rendu
- ✅ **Restauration lors de la navigation** : Détecte les changements d'URL et restaure automatiquement la position correspondante
- ✅ **Clé par URL** : Chaque page conserve sa propre position de splitter

### Exemple
```razor
<!-- Page /customers -->
<SuperSplitter EnableStatePersistence="true">
    <SplitPane>Liste des clients</SplitPane>
    <SplitPane>Détails</SplitPane>
</SuperSplitter>

<!-- L'utilisateur redimensionne à 30% -->
<!-- Navigation vers /products -->
<!-- Navigation retour vers /customers -->
<!-- Le splitter est automatiquement restauré à 30% -->
```

### Clé de stockage
Par défaut, la clé de stockage est basée sur l'URL relative :
```
SuperBlazorComponents.Components.SuperSplitter:/customers:SuperSplitter
```

### Clé personnalisée
Pour partager la même position entre plusieurs pages :
```razor
<SuperSplitter PersistenceKey="my-custom-key">
    <SplitPane>...</SplitPane>
    <SplitPane>...</SplitPane>
</SuperSplitter>
```

### Désactiver la persistance
```razor
<SuperSplitter EnableStatePersistence="false">
    <SplitPane>...</SplitPane>
    <SplitPane>...</SplitPane>
</SuperSplitter>
```

## Paramètres de SplitPane

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `ChildContent` | `RenderFragment?` | - | Contenu du panneau |
| `CssClass` | `string?` | - | Classes CSS supplémentaires à appliquer |
| `AdditionalAttributes` | `Dictionary<string, object>?` | - | Attributs HTML supplémentaires |

## Fonctionnement technique

### Enregistrement des panneaux
Les composants `SplitPane` s'enregistrent automatiquement auprès du `SuperSplitter` via `CascadingValue` :

```csharp
internal void RegisterPane(SplitPane pane)
{
    if (_panes.Count >= 2)
    {
        throw new InvalidOperationException(...);
    }
    _panes.Add(pane);
}
```

### Application des classes CSS
La méthode `GetCombinedCssClass` combine les classes de base avec les classes personnalisées :

```csharp
internal string GetCombinedCssClass(string baseCssClass)
{
    return string.IsNullOrWhiteSpace(CssClass) 
        ? baseCssClass 
        : $"{baseCssClass} {CssClass}";
}
```

### Rendu final
```html
<div class="super-splitter-pane super-splitter-pane-first custom-class" style="height: 50%;">
    <!-- Contenu du premier panneau -->
</div>
```

## Validation

Le composant valide qu'il y a exactement 2 `SplitPane` :
```csharp
if (_panes.Count >= 2)
{
    throw new InvalidOperationException(
        $"{nameof(SuperSplitter)} ne peut contenir que 2 {nameof(SplitPane)}"
    );
}
```

## Exemple complet avec contexte

```razor
@page "/splitter-demo"

<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal" 
               FirstPaneSize="25" 
               EnableStatePersistence="true"
               PersistenceKey="my-splitter-demo">
    
    <SplitPane CssClass="sidebar-pane bg-light border-end">
        <nav class="p-3">
            <h4>Navigation</h4>
            <ul class="list-unstyled">
                <li><a href="#">Item 1</a></li>
                <li><a href="#">Item 2</a></li>
                <li><a href="#">Item 3</a></li>
            </ul>
        </nav>
    </SplitPane>
    
    <SplitPane CssClass="main-content-pane">
        <div class="p-4">
            <h1>Contenu Principal</h1>
            <p>Le contenu s'adapte automatiquement à la taille du panneau.</p>
        </div>
    </SplitPane>
    
</SuperSplitter>

<style>
    .sidebar-pane {
        overflow-y: auto;
    }
    
    .main-content-pane {
        overflow-y: auto;
        background-color: #ffffff;
    }
</style>

@code {
    // Code supplémentaire si nécessaire
}
```

## Classes CSS par défaut

Les classes suivantes sont automatiquement appliquées :

### Container
- `super-splitter-container` : Conteneur principal
- `super-splitter-horizontal` : Mode horizontal
- `super-splitter-vertical` : Mode vertical

### Panneaux
- `super-splitter-pane` : Classe de base pour tous les panneaux
- `super-splitter-pane-first` : Premier panneau
- `super-splitter-pane-second` : Second panneau

### Barre de séparation
- `super-splitter-bar` : Barre de séparation draggable
- `super-splitter-grip` : Icône de grip
- `dragging` : Classe ajoutée pendant le drag

## Bonnes pratiques

1. **Toujours utiliser 2 SplitPane** : Le composant nécessite exactement 2 panneaux
2. **Overflow** : Ajouter `overflow-auto` ou `overflow-y: auto` pour le scroll
3. **Persistance** : Utiliser `PersistenceKey` unique pour chaque splitter
4. **Classes Bootstrap** : Utiliser les classes Bootstrap natives (`bg-light`, `border`, `p-3`, etc.)
5. **Responsive** : Tester le comportement sur différentes tailles d'écran

## Conventions respectées

✅ Composants dans namespace `SuperBlazorComponents`  
✅ Validation des paramètres avec exceptions explicites  
✅ Support de `CascadingValue` pour la communication parent-enfant  
✅ Capture des attributs non matchés avec `CaptureUnmatchedValues`  
✅ Nommage cohérent : `SplitPane` au lieu de `SuperSplitterPane`  
✅ Documentation inline et XML comments
