export function ensurePopover(element) {
    if (!element) {
        return;
    }

    if (!window.bootstrap || !window.bootstrap.Popover) {
        return;
    }

    try {
        window.bootstrap.Popover.getOrCreateInstance(element);
    } catch {
        // ignore
    }
}

export function disposePopover(element) {
    if (!element) {
        return;
    }

    if (!window.bootstrap || !window.bootstrap.Popover) {
        return;
    }

    try {
        const instance = window.bootstrap.Popover.getInstance(element);
        if (instance) {
            instance.dispose();
        }
    } catch {
        // ignore
    }
}
