const chatResizerInstances = new Map();

export function initChatResizer(handleElement, dotNetRef, options) {
    if (!handleElement) return;
    const layoutElement = handleElement.closest('.super-layout');
    if (!layoutElement) return;

    const opts = options || {};
    const minWidth = typeof opts.minWidth === 'number' ? opts.minWidth : 200;
    const maxWidth = typeof opts.maxWidth === 'number' ? opts.maxWidth : 1200;

    const state = {
        isDragging: false,
        startX: 0,
        startWidth: 0
    };

    const getX = (e) => {
        if (e.touches && e.touches.length > 0) return e.touches[0].clientX;
        return e.clientX;
    };

    const readCurrentWidth = () => {
        const cssVar = getComputedStyle(layoutElement).getPropertyValue('--super-chatpanel-width').trim();
        const parsed = parseFloat(cssVar);
        if (!isNaN(parsed)) return parsed;

        const aside = layoutElement.querySelector(':scope > .super-chatpanel');
        if (aside) {
            return aside.getBoundingClientRect().width;
        }
        return 380;
    };

    const onDown = (e) => {
        e.preventDefault();
        e.stopPropagation();

        state.isDragging = true;
        state.startX = getX(e);
        state.startWidth = readCurrentWidth();

        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
        document.addEventListener('touchmove', onMove, { passive: false });
        document.addEventListener('touchend', onUp);

        document.body.style.cursor = 'ew-resize';
        document.body.style.userSelect = 'none';

        // Disable layout transition for smooth dragging
        layoutElement.dataset.prevTransition = layoutElement.style.transition || '';
        layoutElement.style.transition = 'none';
        const aside = layoutElement.querySelector(':scope > .super-chatpanel');
        if (aside) {
            aside.dataset.prevTransition = aside.style.transition || '';
            aside.style.transition = 'none';
        }

        handleElement.classList.add('dragging');
    };

    const onMove = (e) => {
        if (!state.isDragging) return;
        e.preventDefault();

        const delta = getX(e) - state.startX;
        // Chat panel is on the right: dragging left increases width
        let newWidth = state.startWidth - delta;
        newWidth = Math.max(minWidth, Math.min(maxWidth, newWidth));

        layoutElement.style.setProperty('--super-chatpanel-width', `${newWidth}px`);
    };

    const onUp = () => {
        if (!state.isDragging) return;
        state.isDragging = false;

        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
        document.removeEventListener('touchmove', onMove);
        document.removeEventListener('touchend', onUp);

        document.body.style.cursor = '';
        document.body.style.userSelect = '';

        // Restore transitions
        layoutElement.style.transition = layoutElement.dataset.prevTransition || '';
        delete layoutElement.dataset.prevTransition;
        const aside = layoutElement.querySelector(':scope > .super-chatpanel');
        if (aside) {
            aside.style.transition = aside.dataset.prevTransition || '';
            delete aside.dataset.prevTransition;
        }

        handleElement.classList.remove('dragging');

        const finalWidth = readCurrentWidth();
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnResizeEnd', finalWidth);
        }
    };

    handleElement.addEventListener('mousedown', onDown);
    handleElement.addEventListener('touchstart', onDown, { passive: false });

    chatResizerInstances.set(handleElement, {
        cleanup: () => {
            handleElement.removeEventListener('mousedown', onDown);
            handleElement.removeEventListener('touchstart', onDown);
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
            document.removeEventListener('touchmove', onMove);
            document.removeEventListener('touchend', onUp);
        }
    });
}

export function setChatPanelWidth(handleElement, width) {
    if (!handleElement) return;
    const layoutElement = handleElement.closest('.super-layout');
    if (!layoutElement) return;
    layoutElement.style.setProperty('--super-chatpanel-width', `${width}px`);
}

export function disposeChatResizer(handleElement) {
    const instance = chatResizerInstances.get(handleElement);
    if (instance) {
        instance.cleanup();
        chatResizerInstances.delete(handleElement);
    }
}
