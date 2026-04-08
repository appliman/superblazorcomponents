const floatingPanels = new Map();
const VIEWPORT_MARGIN = 8;
const PANEL_MARGIN = 8;

export function attachFloatingPanel(hostElement, buttonElement, panelElement) {
    if (!hostElement || !buttonElement || !panelElement) {
        return;
    }

    detachFloatingPanel(hostElement);

    const originalParent = panelElement.parentElement;
    const placeholder = document.createComment('super-date-range-panel-placeholder');
    originalParent?.insertBefore(placeholder, panelElement);
    document.body.appendChild(panelElement);

    const updatePosition = () => positionPanel(buttonElement, panelElement);
    const onResize = () => updatePosition();
    const onScroll = () => updatePosition();

    window.addEventListener('resize', onResize);
    window.addEventListener('scroll', onScroll, true);

    updatePosition();

    floatingPanels.set(hostElement, {
        panelElement,
        originalParent,
        placeholder,
        onResize,
        onScroll
    });
}

export function detachFloatingPanel(hostElement) {
    if (!hostElement) {
        return;
    }

    const instance = floatingPanels.get(hostElement);
    if (!instance) {
        return;
    }

    window.removeEventListener('resize', instance.onResize);
    window.removeEventListener('scroll', instance.onScroll, true);

    if (instance.originalParent) {
        instance.originalParent.insertBefore(instance.panelElement, instance.placeholder);
        instance.placeholder.remove();
    }

    clearPanelStyle(instance.panelElement);
    floatingPanels.delete(hostElement);
}

function positionPanel(buttonElement, panelElement) {
    const buttonRect = buttonElement.getBoundingClientRect();
    if (buttonRect.width === 0 || buttonRect.height === 0) {
        return;
    }

    panelElement.style.position = 'fixed';
    panelElement.style.top = '0';
    panelElement.style.left = '0';
    panelElement.style.maxWidth = `calc(100vw - ${VIEWPORT_MARGIN * 2}px)`;
    panelElement.style.zIndex = '5000';

    const panelRect = panelElement.getBoundingClientRect();
    const left = clamp(buttonRect.left, VIEWPORT_MARGIN, window.innerWidth - panelRect.width - VIEWPORT_MARGIN);

    let top = buttonRect.bottom + PANEL_MARGIN;
    const canDisplayAbove = buttonRect.top - panelRect.height - PANEL_MARGIN >= VIEWPORT_MARGIN;
    const exceedsBottom = top + panelRect.height > window.innerHeight - VIEWPORT_MARGIN;

    if (exceedsBottom && canDisplayAbove) {
        top = buttonRect.top - panelRect.height - PANEL_MARGIN;
    }

    top = Math.max(VIEWPORT_MARGIN, top);

    panelElement.style.left = `${left}px`;
    panelElement.style.top = `${top}px`;
}

function clearPanelStyle(panelElement) {
    if (!panelElement) {
        return;
    }

    panelElement.style.removeProperty('position');
    panelElement.style.removeProperty('top');
    panelElement.style.removeProperty('left');
    panelElement.style.removeProperty('max-width');
    panelElement.style.removeProperty('z-index');
}

function clamp(value, min, max) {
    if (max < min) {
        return min;
    }

    return Math.min(Math.max(value, min), max);
}
