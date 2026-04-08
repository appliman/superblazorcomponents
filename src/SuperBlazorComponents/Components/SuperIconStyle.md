# SuperIconStyle - Guide d'utilisation

## Description

L'enum `SuperIconStyle` permet de spécifier le style d'icône Font Awesome à utiliser dans les composants :
- `SuperButton`
- `SuperMenuItem`
- `SuperSplitButton`
- `SuperSplitLinkItem`

## Valeurs disponibles

- **Solid** (par défaut) : Icônes pleines (`fa-solid`)
- **Regular** : Icônes en contour (`fa-regular`)
- **Brands** : Icônes de marques (`fa-brands`)
- **Duotone** : Icônes bicolores (`fa-duotone`)

## Utilisation

### Avec SuperButton

```razor
<!-- Icône solide (par défaut) -->
<SuperButton Icon="fa-envelope" Text="Envoyer" Click="HandleClick" />

<!-- Icône régulière -->
<SuperButton Icon="fa-envelope" IconStyle="SuperIconStyle.Regular" Text="Envoyer" Click="HandleClick" />

<!-- Icône de marque -->
<SuperButton Icon="fa-github" IconStyle="SuperIconStyle.Brands" Text="GitHub" Click="HandleClick" />

<!-- Icône duotone -->
<SuperButton Icon="fa-envelope" IconStyle="SuperIconStyle.Duotone" Text="Envoyer" Click="HandleClick" />
```

### Avec SuperMenuItem

```razor
<!-- Icône solide (par défaut) -->
<SuperMenuItem Href="/messages" Icon="fa-envelope" Text="Messages" />

<!-- Icône régulière -->
<SuperMenuItem Href="/messages" Icon="fa-envelope" IconStyle="SuperIconStyle.Regular" Text="Messages" />

<!-- Icône de marque -->
<SuperMenuItem Href="/github" Icon="fa-github" IconStyle="SuperIconStyle.Brands" Text="GitHub" />
```

### Avec SuperSplitButton

```razor
<!-- Icône solide (par défaut) -->
<SuperSplitButton Text="Actions" Icon="fa-cog">
    <Menu>
        <SuperSplitLinkItem Text="Option 1" Icon="fa-edit" Path="/edit" />
    </Menu>
</SuperSplitButton>

<!-- Icône régulière -->
<SuperSplitButton Text="Actions" Icon="fa-cog" IconStyle="SuperIconStyle.Regular">
    <Menu>
        <SuperSplitLinkItem Text="Option 1" Icon="fa-edit" Path="/edit" />
    </Menu>
</SuperSplitButton>

<!-- Icône de marque -->
<SuperSplitButton Text="Partager" Icon="fa-share-nodes" IconStyle="SuperIconStyle.Brands">
    <Menu>
        <SuperSplitLinkItem Text="Twitter" Icon="fa-twitter" IconStyle="SuperIconStyle.Brands" Path="/share/twitter" />
    </Menu>
</SuperSplitButton>
```

### Avec SuperSplitLinkItem

```razor
<!-- Icône solide (par défaut) -->
<SuperSplitLinkItem Text="Mon Profil" Icon="fa-user" Path="/profile" />

<!-- Icône régulière -->
<SuperSplitLinkItem Text="Mon Profil" Icon="fa-user" IconStyle="SuperIconStyle.Regular" Path="/profile" />

<!-- Icône Material (legacy, détectée automatiquement) -->
<SuperSplitLinkItem Text="Mon Profil" Icon="manage_accounts" Path="/profile" />
```

## Format des icônes

Il suffit de fournir le nom de l'icône **sans le préfixe de style** :

✅ **Correct** : `Icon="fa-envelope"`  
❌ **Incorrect** : `Icon="fa-solid fa-envelope"`

Le préfixe de style (`fa-solid`, `fa-regular`, etc.) est automatiquement ajouté selon la valeur de `IconStyle`.

## Exemples complets

```razor
<!-- Bouton avec icône solide (défaut) -->
<SuperButton Icon="fa-save" Text="Sauvegarder" Click="Save" />

<!-- Bouton avec icône régulière -->
<SuperButton Icon="fa-calendar" IconStyle="SuperIconStyle.Regular" Text="Calendrier" Click="OpenCalendar" />

<!-- Bouton avec icône de marque -->
<SuperButton Icon="fa-twitter" IconStyle="SuperIconStyle.Brands" Text="Partager" Click="ShareOnTwitter" />

<!-- Menu avec icônes variées -->
<SuperMenuItem Href="/home" Icon="fa-house" Text="Accueil" />
<SuperMenuItem Href="/messages" Icon="fa-envelope" IconStyle="SuperIconStyle.Regular" Text="Messages" />
<SuperMenuItem Href="/github" Icon="fa-github" IconStyle="SuperIconStyle.Brands" Text="GitHub" />

<!-- Split button avec menu -->
<SuperSplitButton Text="@connectedMember.Name" Icon="fa-user">
    <Menu>
        <SuperSplitLinkItem Text="Mon Profil" Icon="fa-user-gear" Path="/profile" />
        <SuperSplitDivider />
        <SuperSplitLinkItem Text="Déconnexion" Icon="fa-right-from-bracket" Path="/logout" />
    </Menu>
</SuperSplitButton>
```

## Notes

- La valeur par défaut est `SuperIconStyle.Solid`
- Assurez-vous que les icônes existent dans le style choisi (toutes les icônes ne sont pas disponibles dans tous les styles)
- Pour Font Awesome Free, certains styles nécessitent une licence Pro
- `SuperSplitLinkItem` supporte également les icônes Material (legacy) - toute icône ne commençant pas par `fa-` sera traitée comme Material Icons
