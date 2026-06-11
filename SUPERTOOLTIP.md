# SuperTooltip

## Description
`SuperTooltip` affiche une infobulle Bootstrap autour de n'importe quel contenu Blazor. Message texte, positions, contenu HTML, delai d'ouverture, duree d'affichage, fermeture au clic dans la page et utilisation sur un element HTML. Il ajoute aussi un rendu Markdown integre, sans dependance externe.

## Prerequis
Le projet hote doit charger Bootstrap 5, notamment `bootstrap.bundle.min.js`, et enregistrer les services SuperBlazorComponents :

```csharp
builder.Services.AddSuperComponents();
```

## Parametres

| Parametre | Type | Defaut | Description |
|-----------|------|--------|-------------|
| `ChildContent` | `RenderFragment?` | - | Element cible enveloppe par le tooltip. |
| `Text` | `string?` | - | Message texte simple. Le contenu est encode HTML. |
| `HtmlContent` | `string?` | - | Contenu HTML brut affiche dans le tooltip. A utiliser avec du contenu de confiance. |
| `Markdown` | `string?` | - | Contenu Markdown converti en HTML par le composant. Prioritaire sur `HtmlContent` et `Text`. |
| `Position` | `SuperTooltipPosition` | `Top` | Position : `Top`, `Right`, `Bottom`, `Left`, `Auto`. |
| `Trigger` | `SuperTooltipTrigger` | `Hover` | Declencheur : `Hover`, `Click`, `Focus`, `Manual`. `Hover` ouvre le tooltip au survol et au focus clavier (comportement Bootstrap `hover focus`). |
| `Delay` | `int` | `0` | Delai d'ouverture en millisecondes. |
| `Duration` | `int` | `0` | Duree d'affichage en millisecondes. `0` laisse le tooltip ouvert selon le trigger. |
| `CloseOnDocumentClick` | `bool` | `false` | Ferme le tooltip lorsqu'un clic intervient ailleurs dans la page. |
| `TooltipCssClass` | `string?` | - | Classe CSS ajoutee au tooltip Bootstrap. |
| `TooltipStyle` | `string?` | - | Style inline applique au tooltip affiche. |
| `Opacity` | `int?` | `null` | Niveau d'opacite du tooltip (0 = totalement transparent, 100 = totalement opaque). `null` correspond a une opacite complete. Fusionne avec `TooltipStyle` si les deux sont definis. |
| `Disabled` | `bool` | `false` | Desactive le tooltip. |
| `AdditionalAttributes` | `Dictionary<string, object>` | - | Attributs HTML supplementaires appliques au wrapper. |

## Markdown supporte
Le parseur integre couvre volontairement un sous-ensemble simple et previsible :

- paragraphes ;
- titres `#` a `######` ;
- listes `-`, `*` et listes numerotees `1.` ;
- gras `**texte**`, italique `*texte*`, code inline `` `code` `` ;
- blocs de code fences ``` ;
- tableaux Markdown avec en-tete et separateur `|---|---|` ;
- liens HTTP/HTTPS `[texte](https://exemple.com)`.

Le Markdown est encode avant la transformation inline afin de limiter les injections HTML. Pour afficher du HTML volontairement, utilisez `HtmlContent`.

## Exemples

### Message texte
```razor
@using SuperBlazorComponents.Components.Tooltips

<SuperTooltip Text="Sauvegarde les modifications">
    <button class="btn btn-primary">Enregistrer</button>
</SuperTooltip>
```

### Positions
```razor
<SuperTooltip Text="A gauche" Position="SuperTooltipPosition.Left">
    <button class="btn btn-outline-secondary">Left</button>
</SuperTooltip>

<SuperTooltip Text="En bas" Position="SuperTooltipPosition.Bottom">
    <button class="btn btn-outline-secondary">Bottom</button>
</SuperTooltip>
```

### Markdown
```razor
<SuperTooltip Markdown="@MarkdownHelp" Position="SuperTooltipPosition.Right">
    <button class="btn btn-info">Aide</button>
</SuperTooltip>

@code {
    private const string MarkdownHelp = """
    ### Regles
    - **Nom** obligatoire
    - `Code` unique
    - Lien : [Appliman](https://www.appliman.com)

    | Champ | Regle |
    |---|---|
    | Nom | Obligatoire |
    | Code | Unique |
    """;
}
```

### HTML
```razor
<SuperTooltip HtmlContent="<strong>Important</strong><br><span>Contenu HTML</span>">
    <button class="btn btn-warning">HTML</button>
</SuperTooltip>
```

### Delai et duree
```razor
<SuperTooltip Text="Ouverture apres 600 ms, fermeture apres 5 s"
              Delay="600"
              Duration="5000">
    <button class="btn btn-secondary">Survoler</button>
</SuperTooltip>
```

### Clic et fermeture au clic de page
```razor
<SuperTooltip Text="Cliquez ailleurs pour fermer"
              Trigger="SuperTooltipTrigger.Click"
              CloseOnDocumentClick="true"
              Position="SuperTooltipPosition.Bottom">
    <button class="btn btn-dark">Click</button>
</SuperTooltip>
```

### Element HTML simple
```razor
<SuperTooltip Text="Fonctionne aussi sur un element HTML">
    <span class="text-decoration-underline">survolez ce texte</span>
</SuperTooltip>
```

### Controle manuel
```razor
<SuperTooltip @ref="_tooltip"
              Text="Tooltip pilote par code"
              Trigger="SuperTooltipTrigger.Manual">
    <button class="btn btn-primary">Cible</button>
</SuperTooltip>

<button class="btn btn-outline-primary" @onclick="_tooltip.ShowAsync">Afficher</button>
<button class="btn btn-outline-secondary" @onclick="_tooltip.HideAsync">Masquer</button>

@code {
    private SuperTooltip _tooltip = default!;
}
```

### Opacite
```razor
<SuperTooltip Text="Tooltip semi-transparent" Opacity="70">
    <button class="btn btn-secondary">Survoler</button>
</SuperTooltip>
```

Compatible avec `TooltipStyle` : les deux styles sont fusionnes.

```razor
<SuperTooltip Text="Style combine" Opacity="80" TooltipStyle="background:red;">
    <button class="btn btn-danger">Survoler</button>
</SuperTooltip>
```

## Bonnes pratiques
- Preferez `Text` pour les messages courts.
- Preferez `Markdown` pour une aide structuree simple, portable et sure.
- Reservez `HtmlContent` aux contenus maitrises par l'application.
- Utilisez `Duration` pour les messages temporaires et `CloseOnDocumentClick` pour les tooltips ouverts au clic.
