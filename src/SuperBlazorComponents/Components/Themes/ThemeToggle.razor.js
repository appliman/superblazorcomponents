// Module d'isolation JavaScript pour le composant ThemeToggle
export function initialize() {
    // Initialiser le thème dès le chargement
    const theme = getTheme();
    setTheme(theme);
    return theme;
}

export function getTheme() {
    const storedTheme = localStorage.getItem('theme');
    if (storedTheme) {
        return storedTheme;
    }
    // Détection automatique du thème système
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

export function setTheme(theme) {
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-bs-theme', theme);
}

export function toggleTheme() {
    const currentTheme = getTheme();
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    setTheme(newTheme);
    return newTheme;
}
