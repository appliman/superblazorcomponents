# ✏️ SuperHtmlEditor

> Éditeur HTML WYSIWYG pour Blazor — barre d'outils complète (police, taille, gras/italique/souligné, couleurs, alignement, listes), zéro dépendance JS tierce pour le mode visuel, et basculement vers l'édition HTML source via **Monaco Editor** chargé à la volée depuis le CDN.

[← Retour au README](README.md)

---

## 📑 Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Démarrage rapide](#démarrage-rapide)
- [Architecture](#architecture)
- [Référence API](#référence-api)
- [Barre d'outils — détail des actions](#barre-doutils--détail-des-actions)
- [Mode édition HTML (Monaco)](#mode-édition-html-monaco)
- [Exemples d'utilisation](#exemples-dutilisation)
  - [Liaison minimale](#liaison-minimale)
  - [Avec label](#avec-label)
  - [Contenu initial](#contenu-initial)
  - [Taille personnalisée](#taille-personnalisée)
  - [Désactivé](#désactivé)
  - [Rendu en direct](#rendu-en-direct)
  - [Dans un formulaire Blazor](#dans-un-formulaire-blazor)
- [Format de la valeur](#format-de-la-valeur)
- [Sauvegarde et restauration de la sélection](#sauvegarde-et-restauration-de-la-sélection)
- [Personnalisation CSS](#personnalisation-css)
- [Bonnes pratiques](#bonnes-pratiques)
- [Limitations connues](#limitations-connues)

---

## Vue d'ensemble

`SuperHtmlEditor` est un éditeur de texte enrichi **WYSIWYG** (What You See Is What You Get) basé sur un `div[contenteditable]` natif du navigateur.

**Fonctionnalités clés**

- 🖊️ **Édition visuelle** — zone `contenteditable` native, performances maximales
- 🔤 **Police & taille** — sélecteur de fonte (6 familles) + sélecteur de taille (7 niveaux)
- **B** **I** <u>S</u> **Gras, Italique, Souligné** — avec état actif reflété en temps réel
- 🎨 **Couleur du texte & couleur de fond** — color pickers natifs `<input type="color">`
- ↔️ **Alignement** — gauche, centre, droite
- 📋 **Listes** — numérotée et à puces
- 🧹 **Effacer la mise en forme** — retire tous les styles de la sélection
- `</>` **Mode HTML source** — bascule vers Monaco Editor chargé **à la volée** (lazy-load CDN)
- 🌗 **Thème Monaco adaptatif** — détecte automatiquement `data-bs-theme="dark"`
- ♿ **Accessible** — labels, `title` sur chaque bouton, `disabled` propagé
- 🎨 **CSS scoped** — aucune pollution de styles globaux
- 📐 **Hauteur configurable** — `MinHeight`, `MaxHeight`, `MonacoHeight`

---

## Démarrage rapide

### Namespace

```razor
@using SuperBlazorComponents.Components.SuperHtmlEditor
```

### Enregistrement des services

Aucun enregistrement spécifique au-delà du service de base :

```csharp
// Program.cs
builder.Services.AddSuperComponents();
```

### Exemple minimal

```razor
@using SuperBlazorComponents.Components.SuperHtmlEditor

<SuperHtmlEditor @bind-Value="_html" />

@code {
    private string? _html;
}
```

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  she-toolbar                                                         │
│  ┌────────┐ ┌──────┐ │ B I S │ │ 🎨 🖌️ │ │ ← → ↔ │ │ 1. • │ 🧹  </>│
│  │ Fonte  │ │Taille│ │       │ │       │ │       │ │     │          │
└─────────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────────┐
│  she-content  (div[contenteditable])          ← mode WYSIWYG        │
│                                                                      │
│  Texte <strong>riche</strong> éditable ici…                         │
└─────────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────────┐
│  she-monaco-container  (Monaco Editor)        ← mode HTML source    │
│                                                                      │
│  <p>Texte <strong>riche</strong> éditable ici…</p>                  │
└─────────────────────────────────────────────────────────────────────┘
```

**Flux de données**

```
Utilisateur frappe / formate
        │
        ▼
contenteditable (DOM)
        │  event: input
        ▼
SuperHtmlEditor.razor.js → invokeMethodAsync('OnContentChanged', html)
        │
        ▼
SuperHtmlEditor.razor.cs → ValueChanged.InvokeAsync(html)
        │
        ▼
Composant parent (@bind-Value)
```

**Interop JS (module ES)**

Le fichier `.razor.js` est chargé comme module ES via `import()` au premier rendu. Il expose les fonctions publiques suivantes :

| Fonction JS | Rôle |
|---|---|
| `initialize(el, toolbar, dotnetRef, html)` | Initialise le `contenteditable`, attache les listeners |
| `execCommand(el, command, value)` | Restaure la sélection puis exécute `document.execCommand` |
| `getHtml(el)` | Retourne `el.innerHTML` |
| `setHtml(el, html)` | Met à jour `el.innerHTML` et notifie Blazor |
| `loadMonaco(container, html)` | Lazy-charge le loader AMD de Monaco depuis le CDN puis crée l'éditeur |
| `setMonacoValue(html)` | Met à jour le contenu de l'instance Monaco existante |
| `getMonacoValue()` | Lit le contenu de Monaco |
| `dispose(el)` | Nettoie tous les listeners et dispose Monaco |

---

## Référence API

### Paramètres

| Paramètre | Type | Défaut | Description |
|---|---|---|---|
| `Value` | `string?` | `null` | Contenu HTML courant |
| `ValueChanged` | `EventCallback<string?>` | — | Déclenché à chaque modification du contenu |
| `Label` | `string?` | `null` | Label affiché au-dessus de l'éditeur |
| `Disabled` | `bool` | `false` | Désactive toutes les interactions (toolbar + zone éditable) |
| `MinHeight` | `int` | `150` | Hauteur minimale de la zone WYSIWYG en pixels |
| `MaxHeight` | `int` | `0` | Hauteur maximale de la zone WYSIWYG en pixels (`0` = illimitée) |
| `MonacoHeight` | `int` | `300` | Hauteur du panneau Monaco Editor en pixels |

### Callbacks .NET invocables depuis JS

| Méthode | Signature | Déclencheur |
|---|---|---|
| `OnContentChanged` | `Task OnContentChanged(string html)` | Événement `input` sur le `contenteditable` |
| `OnFocusChanged` | `void OnFocusChanged(bool focused)` | `focus` / `blur` sur le `contenteditable` |
| `OnSelectionStateChanged` | `void OnSelectionStateChanged(bool bold, bool italic, bool underline)` | `keyup` / `mouseup` sur le `contenteditable` |

---

## Barre d'outils — détail des actions

| Bouton | Commande `execCommand` | Notes |
|---|---|---|
| Fonte (select) | `fontName` | 6 familles : Arial, Georgia, Courier New, Times New Roman, Verdana, Trebuchet MS |
| Taille (select) | `fontSize` | Valeurs 1–7 (équivalent HTML font size : ~8 à 36 pt) |
| **B** Gras | `bold` | Toggle — état actif reflété par `.she-active` |
| *I* Italique | `italic` | Toggle — état actif reflété |
| <u>S</u> Souligné | `underline` | Toggle — état actif reflété |
| 🎨 Couleur texte | `foreColor` | Color picker natif `<input type="color">` |
| 🖌️ Couleur fond | `hiliteColor` | Color picker natif — surlignage de la sélection |
| Aligner gauche | `justifyLeft` | — |
| Centrer | `justifyCenter` | — |
| Aligner droite | `justifyRight` | — |
| Liste numérotée | `insertOrderedList` | Toggle `<ol>` |
| Liste à puces | `insertUnorderedList` | Toggle `<ul>` |
| Effacer format | `removeFormat` | Retire tous les styles inline de la sélection |
| `</>` HTML | — | Bascule vers Monaco (voir ci-dessous) |

> **Note** : `document.execCommand` est marqué *deprecated* dans la spec W3C mais reste **supporté et fonctionnel** dans tous les navigateurs actuels. Il n'existe pas d'API standardisée de remplacement à ce jour.

---

## Mode édition HTML (Monaco)

### Fonctionnement

1. L'utilisateur clique sur le bouton **`</> HTML`**
2. Le composant lit l'HTML courant depuis le `contenteditable`
3. Si Monaco n'est pas encore chargé, un **spinner** s'affiche et le loader AMD est injecté dynamiquement depuis le CDN jsDelivr
4. Monaco Editor s'initialise en mode `html` avec coloration syntaxique, numéros de ligne, retour à la ligne automatique et `automaticLayout`
5. Quand l'utilisateur reclique sur **`</> HTML`**, la valeur Monaco est réinjectée dans le `contenteditable` et `ValueChanged` est déclenché

### Chargement à la volée

```
Premier clic sur </> HTML
        │
        ▼
<script src="https://cdn.jsdelivr.net/.../loader.js"> injecté dynamiquement
        │
        ▼
require(['vs/editor/editor.main'], callback)
        │
        ▼
monaco.editor.create(container, { language: 'html', ... })
        │
        ▼
_monacoReady = true  →  spinner masqué, éditeur visible
```

Les clics suivants réutilisent l'instance existante (singleton par composant).

### Thème Monaco

Le thème est sélectionné automatiquement à la création de l'éditeur :

```js
theme: document.documentElement.getAttribute('data-bs-theme') === 'dark'
    || document.documentElement.classList.contains('dark')
    ? 'vs-dark' : 'vs'
```

### Version Monaco

La version utilisée est **0.52.0** (jsDelivr). Pour pointer une autre version, modifier l'URL dans `SuperHtmlEditor.razor.js` :

```js
// Remplacer 0.52.0 par la version souhaitée
loaderScript.src = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.0/min/vs/loader.js';
```

---

## Exemples d'utilisation

### Liaison minimale

```razor
<SuperHtmlEditor @bind-Value="_html" />

@code {
    private string? _html;
}
```

---

### Avec label

```razor
<SuperHtmlEditor Label="Corps du message" @bind-Value="_body" />
```

---

### Contenu initial

```razor
<SuperHtmlEditor Label="Description"
                 @bind-Value="_description" />

@code {
    private string? _description =
        "<p>Bienvenue dans <strong>SuperHtmlEditor</strong> !</p>" +
        "<p>Modifiez ce texte librement.</p>";
}
```

---

### Taille personnalisée

```razor
<!-- Zone compacte avec défilement au-delà de 200 px -->
<SuperHtmlEditor Label="Note"
                 @bind-Value="_note"
                 MinHeight="80"
                 MaxHeight="200"
                 MonacoHeight="250" />

<!-- Grande zone pour un éditeur de contenu long -->
<SuperHtmlEditor Label="Article"
                 @bind-Value="_article"
                 MinHeight="400"
                 MonacoHeight="500" />
```

---

### Désactivé

```razor
<SuperHtmlEditor Label="Contenu (lecture seule)"
                 Value="@_readonlyHtml"
                 Disabled="true" />
```

---

### Rendu en direct

Utilisez `(MarkupString)` pour afficher le HTML produit dans la page :

```razor
<div class="row">
    <div class="col-md-6">
        <SuperHtmlEditor Label="Éditeur" @bind-Value="_html" MinHeight="250" />
    </div>
    <div class="col-md-6">
        <label class="form-label">Aperçu</label>
        <div class="border rounded p-3">
            @((MarkupString)(_html ?? ""))
        </div>
    </div>
</div>

@code {
    private string? _html = "<h3>Titre</h3><p>Contenu…</p>";
}
```

---

### Dans un formulaire Blazor

```razor
<EditForm Model="_model" OnValidSubmit="Save">
    <DataAnnotationsValidator />

    <div class="mb-3">
        <SuperHtmlEditor Label="Description *"
                         @bind-Value="_model.Description"
                         MinHeight="200" />
        <ValidationMessage For="@(() => _model.Description)" />
    </div>

    <button type="submit" class="btn btn-primary">Enregistrer</button>
</EditForm>

@code {
    private ArticleModel _model = new();

    private async Task Save()
    {
        // _model.Description contient le HTML
    }

    public class ArticleModel
    {
        [Required(ErrorMessage = "La description est obligatoire.")]
        public string? Description { get; set; }
    }
}
```

> **Note** : `SuperHtmlEditor` ne déclenche pas encore `EditContext` automatiquement (pas de `ValueExpression`). L'appel à `ValueChanged` met cependant à jour le modèle lié, ce qui suffit pour la validation `DataAnnotationsValidator`.

---

## Format de la valeur

`Value` / `ValueChanged` transportent du **HTML brut** tel que produit par le navigateur depuis le `contenteditable`. Exemples :

```html
<!-- Gras -->
<b>Texte</b>

<!-- Couleur de texte -->
<font color="#e03333">Rouge</font>

<!-- Police -->
<font face="Georgia">Georgia</font>

<!-- Taille -->
<font size="5">Grand</font>

<!-- Alignement -->
<div style="text-align: center;">Centré</div>

<!-- Liste -->
<ul><li>Élément 1</li><li>Élément 2</li></ul>
```

> Le HTML est généré par `document.execCommand` et peut varier légèrement selon le navigateur. Pour un affichage fidèle, utilisez toujours `(MarkupString)` et non un binding texte.

---

## Sauvegarde et restauration de la sélection

Quand l'utilisateur interagit avec la barre d'outils (clic sur un bouton, ouverture d'un `<select>`), le `contenteditable` peut perdre le focus et donc la sélection de texte.

Le module JS résout ce problème en deux temps :

1. **Sauvegarde** — à chaque `keyup`, `mouseup` et `blur` sur l'éditeur, un clone du `Range` courant est mémorisé dans `state.savedRange`
2. **Restauration** — avant chaque `execCommand`, le focus est remis sur l'éditeur et la sélection sauvegardée est restaurée via `sel.addRange(state.savedRange)`

```js
// Sauvegarde
const saveRange = () => {
    const sel = window.getSelection();
    if (sel && sel.rangeCount > 0) {
        state.savedRange = sel.getRangeAt(0).cloneRange();
    }
};

// Restauration avant commande
el.focus();
const sel = window.getSelection();
sel.removeAllRanges();
sel.addRange(state.savedRange);
document.execCommand(command, false, value);
```

---

## Personnalisation CSS

Toutes les classes CSS sont préfixées `she-` et définies dans `SuperHtmlEditor.razor.css` (scoped). Les variables Bootstrap 5 sont utilisées systématiquement pour la compatibilité dark/light.

| Variable CSS | Usage |
|---|---|
| `--bs-border-color` | Bordures de la boîte et de la toolbar |
| `--bs-body-bg` | Fond de la zone éditable et des selects |
| `--bs-tertiary-bg` | Fond de la toolbar |
| `--bs-body-color` | Couleur du texte |
| `--bs-primary` | Fond des boutons actifs (`she-active`) |
| `--bs-secondary-bg` | Fond au survol des boutons |

### Exemples de personnalisation

```css
/* Arrondir davantage le composant */
.she-editor-box {
    border-radius: 12px;
}

/* Fond légèrement coloré pour la toolbar */
.she-toolbar {
    background: #f0f4ff;
}

/* Focus plus visible */
.she-editor-box.she-focused {
    border-color: #6610f2;
    box-shadow: 0 0 0 .25rem rgba(102, 16, 242, .25);
}
```

---

## Bonnes pratiques

- **Sanitiser le HTML côté serveur** avant de le stocker en base ou de le ré-afficher. Utilisez une librairie comme [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer) (NuGet) pour prévenir les attaques XSS.
- **Ne pas utiliser** `@Html.Raw()` / `(MarkupString)` sur du contenu non fiable sans sanitisation préalable.
- Pour les **formulaires** avec validation, gérez la propriété liée directement (`@bind-Value`) — `EditContext` n'est pas nécessaire pour la validation côté modèle.
- Pour les **contenus très longs** (articles, pages), préférez `MaxHeight` + `MonacoHeight` élevé afin de donner de l'espace à l'éditeur source.
- Le **lazy-load Monaco** nécessite une connexion internet au premier clic. Dans un contexte offline ou intranet strict, hébergez Monaco en local et ajustez l'URL dans `SuperHtmlEditor.razor.js`.

---

## Limitations connues

| Limitation | Détail |
|---|---|
| Pas d'insertion d'images | Choix délibéré — utiliser un composant dédié si nécessaire |
| `execCommand` déprécié | Toujours fonctionnel dans tous les navigateurs actuels ; aucun remplacement standard n'existe encore |
| Monaco CDN requis | La première utilisation du mode HTML source nécessite internet (ou hébergement local) |
| Thème Monaco fixé à la création | Le thème Monaco ne se met pas à jour dynamiquement si l'utilisateur change de thème pendant qu'il est ouvert |
| Pas d'`EditContext` automatique | Pas de `ValueExpression` → pas de marquage d'invalidité natif dans un `EditForm` |
