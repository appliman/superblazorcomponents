# SuperDropDown

## Description
Composant générique de liste déroulante (dropdown/select) qui utilise la réflexion pour accéder dynamiquement aux propriétés des éléments de la collection.

## Paramètres génériques
- `TItem` : Type des éléments dans la collection Data
- `TValue` : Type de la valeur sélectionnée (ex: int, Guid, string)

## Paramètres principaux

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `Value` | `TValue?` | - | Valeur actuellement sélectionnée |
| `ValueChanged` | `EventCallback<TValue?>` | - | Événement déclenché lors du changement de valeur |
| `Data` | `IEnumerable<TItem>?` | - | Collection d'éléments à afficher |
| `TextProperty` | `string` | `"Name"` | Nom de la propriété à afficher comme texte |
| `ValueProperty` | `string` | `"Id"` | Nom de la propriété à utiliser comme valeur |
| `Label` | `string?` | - | Libellé affiché à gauche du champ |
| `Required` | `bool` | `false` | Indique si le champ est obligatoire (affiche `*`) |
| `HelpText` | `string?` | - | Texte d'aide affiché sous le select |
| `Disabled` | `bool` | `false` | Désactive le composant |
| `PlaceHolder` | `string?` | - | Texte pour l'option vide (sinon "-- Sélectionner --") |
| `ValueExpression` | `Expression<Func<TValue?>>?` | - | Expression pour la validation Blazor |
| `CapturedAttributes` | `Dictionary<string, object>?` | - | Attributs HTML supplémentaires |

## Utilisation

### Exemple 1 : Avec SuperEnum
```razor
@code {
    IEnumerable<Appliman.Datas.SuperEnums.Application> applications = 
        Appliman.Datas.SuperEnums.Application.GetValues();
    
    int selectedApplicationId;
}

<SuperDropDown TItem="Appliman.Datas.SuperEnums.Application" 
               TValue="int"
               Value="@selectedApplicationId" 
               ValueChanged="@((int value) => selectedApplicationId = value)"
               Data="@applications" 
               TextProperty="Name" 
               ValueProperty="Id" 
               Label="Application" 
               Required="false" />
```

### Exemple 2 : Avec une classe personnalisée
```razor
@code {
    public class Country
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
    
    List<Country> countries = new()
    {
        new() { Id = Guid.NewGuid(), Name = "France", Code = "FR" },
        new() { Id = Guid.NewGuid(), Name = "Belgique", Code = "BE" }
    };
    
    Guid? selectedCountryId;
}

<SuperDropDown TItem="Country" 
               TValue="Guid?"
               Value="@selectedCountryId" 
               ValueChanged="@((Guid? value) => selectedCountryId = value)"
               Data="@countries" 
               TextProperty="Name" 
               ValueProperty="Id" 
               Label="Pays"
               PlaceHolder="-- Choisissez un pays --"
               HelpText="Sélectionnez votre pays de résidence" />
```

### Exemple 3 : Sans label (mode inline)
```razor
<div class="d-flex gap-2">
    <span>Statut :</span>
    <SuperDropDown TItem="Status" 
                   TValue="int"
                   Value="@statusId" 
                   ValueChanged="@((int value) => statusId = value)"
                   Data="@statusList" 
                   TextProperty="Label" 
                   ValueProperty="Value" />
</div>
```

## Fonctionnement technique

### Réflexion
Le composant utilise `PropertyInfo.GetValue()` pour accéder dynamiquement aux propriétés :
```csharp
PropertyInfo? _textPropertyInfo;
PropertyInfo? _valuePropertyInfo;

protected override void OnInitialized()
{
    var itemType = typeof(TItem);
    _textPropertyInfo = itemType.GetProperty(TextProperty);
    _valuePropertyInfo = itemType.GetProperty(ValueProperty);
    
    // Validation que les propriétés existent
    if (_textPropertyInfo is null)
        throw new InvalidOperationException(...);
}
```

### Conversion de type
```csharp
async Task OnValueChanged(ChangeEventArgs args)
{
    var selectedValue = args.Value?.ToString();
    
    if (string.IsNullOrEmpty(selectedValue))
        Value = default;
    else
        Value = (TValue)Convert.ChangeType(selectedValue, typeof(TValue));
    
    await ValueChanged.InvokeAsync(Value);
}
```

## Layout

Le composant utilise le système de grille Bootstrap :
- **Avec label** : grille 2/10 (`col-md-2` / `col-md-10`)
- **Sans label** : pleine largeur (`col-md-12`)

## Validation

Le composant supporte la validation Blazor via `ValidationMessage` :
```razor
<EditForm Model="model">
    <DataAnnotationsValidator />
    
    <SuperDropDown TItem="Category" 
                   TValue="int?"
                   Value="@model.CategoryId" 
                   ValueChanged="@((int? value) => model.CategoryId = value)"
                   Data="@categories" 
                   TextProperty="Name" 
                   ValueProperty="Id" 
                   ValueExpression="@(() => model.CategoryId)"
                   Required="true" />
</EditForm>
```

## CSS

Le composant utilise les classes Bootstrap 5 :
- `form-select` pour le `<select>`
- `form-group`, `row`, `mb-3` pour le layout
- `form-label` pour le label
- `form-text`, `text-muted` pour le texte d'aide

## Gestion d'erreurs

- **Propriété inexistante** : Lève une `InvalidOperationException` si `TextProperty` ou `ValueProperty` n'existe pas sur `TItem`
- **Conversion impossible** : Retourne `default(TValue)` si la conversion échoue

## Limitations

- Les propriétés `TextProperty` et `ValueProperty` doivent être publiques
- La conversion de valeur utilise `Convert.ChangeType` donc limitée aux types compatibles
- La réflexion est mise en cache dans `OnInitialized()` pour les performances

## Différences avec RadzenDropDown

| Aspect | SuperDropDown | RadzenDropDown |
|--------|---------------|----------------|
| Framework CSS | Bootstrap 5 natif | Radzen custom |
| Rendu HTML | `<select>` natif | Composant Radzen |
| Dépendances | Aucune (Blazor standard) | Package Radzen.Blazor |
| Layout | Grille 2/10 cohérente | Layout Radzen |
| Binding | Value + ValueChanged | @bind-Value |

## Conventions respectées

✅ Nom de fichier : `PascalCase` (SuperDropDown.razor)  
✅ Champs privés : `_camelCase` (_textPropertyInfo)  
✅ Indentation : 4 espaces  
✅ Pas de `#region`  
✅ Utilise `var` pour types évidents  
✅ Async/await  
✅ Aucune dépendance externe non nécessaire  
✅ Layout cohérent avec autres composants Super*
