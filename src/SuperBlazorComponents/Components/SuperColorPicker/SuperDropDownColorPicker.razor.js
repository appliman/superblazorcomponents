export function positionPopup(trigger, popup) {
    if (!trigger || !popup) {
        return "";
    }

    const triggerRect = trigger.getBoundingClientRect();
    const popupRect = popup.getBoundingClientRect();
    const margin = 8;
    const gap = 5;

    let left = triggerRect.left;
    let top = triggerRect.bottom + gap;

    if (left + popupRect.width + margin > window.innerWidth) {
        left = window.innerWidth - popupRect.width - margin;
    }

    if (top + popupRect.height + margin > window.innerHeight) {
        top = triggerRect.top - popupRect.height - gap;
    }

    left = Math.max(margin, left);
    top = Math.max(margin, top);

    return `top:${top}px;left:${left}px;`;
}
