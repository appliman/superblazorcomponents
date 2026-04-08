const splitterInstances = new Map();

export function initSplitter(container, dotNetRef, orientation) {
    if (!container) return;

    // Sélectionner uniquement la barre de splitter qui est enfant direct de ce container
    const splitterBar = container.querySelector(':scope > .super-splitter-bar');
    if (!splitterBar) return;

    const state = {
        isDragging: false,
        startPosition: 0,
        startSize: 0,
        orientation: orientation
    };

    const getPosition = (e) => {
        const currentOrientation = state.orientation;
        if (e.touches && e.touches.length > 0) {
            return currentOrientation === 'horizontal' ? e.touches[0].clientY : e.touches[0].clientX;
        }
        return currentOrientation === 'horizontal' ? e.clientY : e.clientX;
    };

    const getContainerSize = () => {
        const rect = container.getBoundingClientRect();
        const currentOrientation = state.orientation;
        return currentOrientation === 'horizontal' ? rect.height : rect.width;
    };

    const onMouseDown = (e) => {
        e.preventDefault();
        e.stopPropagation(); // Empêcher la propagation vers les splitters parents
        state.isDragging = true;
        state.startPosition = getPosition(e);

        // Sélectionner uniquement le premier pane qui est enfant direct de ce container
        const firstPane = container.querySelector(':scope > .super-splitter-pane-first');
        const firstPaneRect = firstPane.getBoundingClientRect();
        const currentOrientation = state.orientation;
        state.startSize = currentOrientation === 'horizontal' ? firstPaneRect.height : firstPaneRect.width;

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
        document.addEventListener('touchmove', onMouseMove, { passive: false });
        document.addEventListener('touchend', onMouseUp);
        
        const currentOrientation2 = state.orientation;
        document.body.style.cursor = currentOrientation2 === 'horizontal' ? 'ns-resize' : 'ew-resize';
        document.body.style.userSelect = 'none';
    };

    const onMouseMove = (e) => {
        if (!state.isDragging) return;
        e.preventDefault();

        const currentPosition = getPosition(e);
        const delta = currentPosition - state.startPosition;
        const containerSize = getContainerSize();
        const newSize = state.startSize + delta;
        const newPercentage = (newSize / containerSize) * 100;

        dotNetRef.invokeMethodAsync('UpdateSize', newPercentage);
    };

    const onMouseUp = () => {
        if (!state.isDragging) return;
        
        state.isDragging = false;
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        document.removeEventListener('touchmove', onMouseMove);
        document.removeEventListener('touchend', onMouseUp);
        
        document.body.style.cursor = '';
        document.body.style.userSelect = '';

        dotNetRef.invokeMethodAsync('StopDragging');
    };

    splitterBar.addEventListener('mousedown', onMouseDown);
    splitterBar.addEventListener('touchstart', onMouseDown, { passive: false });

    splitterInstances.set(container, {
        cleanup: () => {
            splitterBar.removeEventListener('mousedown', onMouseDown);
            splitterBar.removeEventListener('touchstart', onMouseDown);
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            document.removeEventListener('touchmove', onMouseMove);
            document.removeEventListener('touchend', onMouseUp);
        }
    });
}

export function disposeSplitter(container) {
    const instance = splitterInstances.get(container);
    if (instance) {
        instance.cleanup();
        splitterInstances.delete(container);
    }
}
