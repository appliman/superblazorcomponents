/**
 * SuperHtmlEditor.razor.js
 * WYSIWYG HTML editor with lazy Monaco code-editor for HTML source view.
 */

// ── Monaco singleton ─────────────────────────────────────────────────────────
let _monacoEditor = null;

// ── Per-element listeners map ─────────────────────────────────────────────────
const _editorMap = new WeakMap();

// ── Public API ────────────────────────────────────────────────────────────────

/**
 * Initialise the contenteditable zone.
 * @param {HTMLElement} el         - The contenteditable div
 * @param {HTMLElement} toolbar    - The toolbar div
 * @param {object}      dotnetRef  - DotNetObjectReference
 * @param {string}      html       - Initial HTML content
 */
export function initialize(el, toolbar, dotnetRef, html) {
    el.contentEditable = 'true';
    el.innerHTML = html ?? '';

    // Prevent toolbar buttons/labels from stealing focus, but allow <select> & <input> to work normally.
    const onToolbarMouseDown = (e) => {
        const tag = e.target.tagName;
        if (tag !== 'SELECT' && tag !== 'INPUT' && tag !== 'OPTION') {
            e.preventDefault();
        }
    };
    toolbar.addEventListener('mousedown', onToolbarMouseDown);

    const onInput = () => {
        dotnetRef.invokeMethodAsync('OnContentChanged', el.innerHTML);
    };

    const state = { onInput: null, onFocus: null, onBlur: null, onSelectionChange: null, dotnetRef, savedRange: null, toolbar, onToolbarMouseDown };

    const saveRange = () => {
        const sel = window.getSelection();
        if (sel && sel.rangeCount > 0) {
            state.savedRange = sel.getRangeAt(0).cloneRange();
        }
    };

    const onFocus = () => dotnetRef.invokeMethodAsync('OnFocusChanged', true);
    const onBlur = () => {
        saveRange();
        dotnetRef.invokeMethodAsync('OnFocusChanged', false);
    };

    const onSelectionChange = () => {
        if (!el.contains(document.activeElement) && document.activeElement !== el) {
            return;
        }
        saveRange();
        dotnetRef.invokeMethodAsync('OnSelectionStateChanged',
            document.queryCommandState('bold'),
            document.queryCommandState('italic'),
            document.queryCommandState('underline')
        );
    };

    state.onInput = onInput;
    state.onFocus = onFocus;
    state.onBlur = onBlur;
    state.onSelectionChange = onSelectionChange;

    el.addEventListener('input', onInput);
    el.addEventListener('focus', onFocus);
    el.addEventListener('blur', onBlur);
    el.addEventListener('keyup', onSelectionChange);
    el.addEventListener('mouseup', onSelectionChange);

    _editorMap.set(el, state);
}

/**
 * Execute a document command on the editable element.
 * @param {HTMLElement} el
 * @param {string}      command
 * @param {string|null} value
 */
export function execCommand(el, command, value) {
    const state = _editorMap.get(el);
    el.focus();
    if (state && state.savedRange) {
        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(state.savedRange);
    }
    document.execCommand(command, false, value ?? null);
    // refresh saved range after command
    const sel = window.getSelection();
    if (state && sel && sel.rangeCount > 0) {
        state.savedRange = sel.getRangeAt(0).cloneRange();
    }
}

/**
 * Get the inner HTML of the editable element.
 * @param {HTMLElement} el
 * @returns {string}
 */
export function getHtml(el) {
    return el.innerHTML ?? '';
}

/**
 * Set the inner HTML of the editable element.
 * @param {HTMLElement} el
 * @param {string}      html
 */
export function setHtml(el, html) {
    el.innerHTML = html ?? '';
    const state = _editorMap.get(el);
    if (state) {
        state.dotnetRef.invokeMethodAsync('OnContentChanged', el.innerHTML);
    }
}

/**
 * Lazy-load Monaco Editor from CDN and create an editor instance
 * inside the given container.
 * @param {HTMLElement} container
 * @param {string}      html       - Initial content to show in Monaco
 */
export function loadMonaco(container, html) {
    return new Promise((resolve) => {
        if (typeof window.monaco !== 'undefined') {
            _createMonacoEditor(container, html);
            resolve();
            return;
        }

        // Inject AMD loader (required by Monaco)
        const loaderScript = document.createElement('script');
        loaderScript.src = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.0/min/vs/loader.js';
        loaderScript.onload = () => {
            window.require.config({
                paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.0/min/vs' }
            });
            window.require(['vs/editor/editor.main'], () => {
                _createMonacoEditor(container, html);
                resolve();
            });
        };
        document.head.appendChild(loaderScript);
    });
}

/**
 * Update the content inside the existing Monaco editor.
 * @param {string} html
 */
export function setMonacoValue(html) {
    if (_monacoEditor) {
        _monacoEditor.setValue(html ?? '');
    }
}

/**
 * Read the current content from Monaco editor.
 * @returns {string}
 */
export function getMonacoValue() {
    if (_monacoEditor) {
        return _monacoEditor.getValue();
    }
    return '';
}

/**
 * Detach event listeners from the editable element.
 * Monaco editor is kept as a singleton during the component lifetime.
 * @param {HTMLElement} el
 */
export function dispose(el) {
    const state = _editorMap.get(el);
    if (state) {
        el.removeEventListener('input', state.onInput);
        el.removeEventListener('focus', state.onFocus);
        el.removeEventListener('blur', state.onBlur);
        el.removeEventListener('keyup', state.onSelectionChange);
        el.removeEventListener('mouseup', state.onSelectionChange);
        if (state.toolbar && state.onToolbarMouseDown) {
            state.toolbar.removeEventListener('mousedown', state.onToolbarMouseDown);
        }
        _editorMap.delete(el);
    }

    if (_monacoEditor) {
        _monacoEditor.dispose();
        _monacoEditor = null;
    }
}

// ── Internals ─────────────────────────────────────────────────────────────────

function _createMonacoEditor(container, html) {
    if (_monacoEditor) {
        _monacoEditor.dispose();
        _monacoEditor = null;
    }

    _monacoEditor = window.monaco.editor.create(container, {
        value: html ?? '',
        language: 'html',
        theme: _isDarkTheme() ? 'vs-dark' : 'vs',
        automaticLayout: true,
        minimap: { enabled: false },
        wordWrap: 'on',
        scrollBeyondLastLine: false,
        fontSize: 13,
        lineNumbers: 'on',
        formatOnPaste: true,
    });
}

function _isDarkTheme() {
    return document.documentElement.getAttribute('data-bs-theme') === 'dark'
        || document.documentElement.classList.contains('dark');
}
