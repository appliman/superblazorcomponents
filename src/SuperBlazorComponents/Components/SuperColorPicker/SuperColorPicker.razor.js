/**
 * SuperColorPicker.razor.js
 * Handles pointer-based drag interactions for the SV area, hue slider and alpha slider.
 * Returns a numeric instance ID so the Blazor component can call dispose(id) on teardown.
 */

const _instances = new Map();
let _nextId = 1;

/**
 * @param {object}      dotnetRef   - DotNetObjectReference from Blazor
 * @param {HTMLElement} svArea      - The saturation/value gradient area
 * @param {HTMLElement} hueTrack    - The hue slider track
 * @param {HTMLElement} alphaTrack  - The alpha slider track
 * @param {boolean}     showAlpha   - Whether the alpha slider is active
 * @returns {number} Instance ID (pass back to dispose)
 */
export function initialize(dotnetRef, svArea, hueTrack, alphaTrack, showAlpha) {
    const id = _nextId++;
    const cleanups = [];

    attachDrag(svArea, 'sv', true, dotnetRef, cleanups);
    attachDrag(hueTrack, 'hue', false, dotnetRef, cleanups);

    if (showAlpha && alphaTrack && alphaTrack.__internalId) {
        attachDrag(alphaTrack, 'alpha', false, dotnetRef, cleanups);
    }

    _instances.set(id, cleanups);
    return id;
}

/**
 * @param {number} id - Instance ID returned by initialize
 */
export function dispose(id) {
    const cleanups = _instances.get(id);
    if (cleanups) {
        cleanups.forEach(fn => fn());
        _instances.delete(id);
    }
}

// ── Internals ────────────────────────────────────────────────────────────────

function attachDrag(el, type, is2D, dotnetRef, cleanups) {
    let active = false;

    const onPointerDown = (e) => {
        if (e.button !== 0) return;
        active = true;
        el.setPointerCapture(e.pointerId);
        sendPosition(el, type, is2D, e, dotnetRef);
        e.preventDefault();
    };

    const onPointerMove = (e) => {
        if (!active) return;
        sendPosition(el, type, is2D, e, dotnetRef);
        e.preventDefault();
    };

    const onPointerUp = () => { active = false; };

    el.addEventListener('pointerdown', onPointerDown);
    el.addEventListener('pointermove', onPointerMove);
    el.addEventListener('pointerup', onPointerUp);
    el.addEventListener('pointercancel', onPointerUp);

    cleanups.push(() => {
        el.removeEventListener('pointerdown', onPointerDown);
        el.removeEventListener('pointermove', onPointerMove);
        el.removeEventListener('pointerup', onPointerUp);
        el.removeEventListener('pointercancel', onPointerUp);
    });
}

function sendPosition(el, type, is2D, e, dotnetRef) {
    const rect = el.getBoundingClientRect();
    const x = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
    const y = is2D
        ? Math.max(0, Math.min(1, (e.clientY - rect.top) / rect.height))
        : 0;

    dotnetRef.invokeMethodAsync('OnDrag', type, x, y);
}
