const tools = new WeakMap();

export function initialize(element) {
    if (!element || tools.has(element)) return;
    const toggle = element.querySelector('.dropdown-toggle');
    // The toolbar sits inside a scrolling grid. Use the viewport as the popup
    // boundary so Popper can place the menu above the toolbar when necessary.
    const dropdown = globalThis.bootstrap?.Dropdown?.getOrCreateInstance(toggle, {
        popperConfig: config => ({
            ...config,
            strategy: 'fixed',
            modifiers: [
                ...config.modifiers,
                { name: 'preventOverflow', options: { boundary: document.documentElement } },
                { name: 'flip', options: { boundary: document.documentElement } }
            ]
        })
    });
    const adjust = () => {
        const button = element.querySelector('.dropdown-toggle');
        const menu = element.querySelector('.dropdown-menu');
        if (!button || !menu) return;
        const rect = button.getBoundingClientRect();
        let top = 0;
        let bottom = window.innerHeight;
        // Respect the visible part of a scrolling application shell, including
        // a sticky header above its content area.
        for (let parent = element.parentElement; parent; parent = parent.parentElement) {
            if (/(auto|scroll)/.test(getComputedStyle(parent).overflowY)) {
                const bounds = parent.getBoundingClientRect();
                top = Math.max(top, bounds.top);
                bottom = Math.min(bottom, bounds.bottom);
            }
        }
        const available = Math.max(rect.top - top, bottom - rect.bottom) - 12;
        menu.style.maxHeight = `${Math.max(100, Math.min(720, available))}px`;
    };
    element.addEventListener('show.bs.dropdown', adjust);
    tools.set(element, { adjust, dropdown });
}

export function dispose(element) {
    const state = tools.get(element);
    if (state) {
        element.removeEventListener('show.bs.dropdown', state.adjust);
        state.dropdown?.dispose();
    }
    tools.delete(element);
}

export function downloadConfiguration(json) {
    const url = URL.createObjectURL(new Blob([json], { type: 'application/json;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'column-configuration.json';
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}
