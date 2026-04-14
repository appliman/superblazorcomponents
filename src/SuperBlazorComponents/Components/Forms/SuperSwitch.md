# SuperSwitch

## Description
Composant switch (interrupteur) basé sur le composant Bootstrap Switch. Affiche une case à cocher stylisée en interrupteur avec une valeur bindable.

## Paramètres

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `Value` | `bool` | `false` | Valeur actuelle du switch |
| `ValueChanged` | `EventCallback<bool>` | - | Événement déclenché lors du changement de valeur |
| `ValueExpression` | `Expression<Func<bool>>?` | - | Expression pour la validation Blazor |
| `Label` | `string?` | - | Libellé affiché à droite du switch |
| `Disabled` | `bool` | `false` | Désactive le composant |
| `HelpText` | `string?` | - | Texte d'aide affiché sous le switch |
| `CapturedAttributes` | `Dictionary<string, object>?` | - | Attributs HTML supplémentaires transmis à l'input |

## Utilisation

### Exemple 1 : Binding simple
```razor
@code {
    bool isActive;
}

<SuperSwitch Label="Activer les notifications" @bind-Value="isActive" />
```

### Exemple 2 : Avec texte d'aide et désactivé
```razor
@code {
    bool isDarkMode;
}

<SuperSwitch Label="Mode sombre"
             @bind-Value="isDarkMode"
             HelpText="Applique un thème sombre à l'interface."
             Disabled="true" />
```

### Exemple 3 : Dans un formulaire avec validation
```razor
@code {
    MyModel model = new();

    class MyModel
    {
        [Required]
        public bool AcceptTerms { get; set; }
    }
}

<EditForm Model="model">
    <DataAnnotationsValidator />
    <SuperSwitch Label="J'accepte les conditions d'utilisation"
                 @bind-Value="model.AcceptTerms"
                 ValueExpression="() => model.AcceptTerms" />
</EditForm>
```

### Exemple 4 : Callback explicite
```razor
@code {
    bool isEnabled;

    async Task OnToggle(bool value)
    {
        isEnabled = value;
        await SaveSettingsAsync();
    }
}

<SuperSwitch Label="Activer le service"
             Value="@isEnabled"
             ValueChanged="OnToggle" />
```
