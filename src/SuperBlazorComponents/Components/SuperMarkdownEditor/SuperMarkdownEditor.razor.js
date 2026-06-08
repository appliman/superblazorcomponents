/**
 * SuperMarkdownEditor.razor.js
 * Simple markdown editor with a lightweight toolbar.
 */

const _editorMap = new WeakMap();

export function initialize(renderedEl, sourceEl, dotnetRef, markdown) {
    sourceEl.value = markdown ?? '';
    renderedEl.innerHTML = renderMarkdown(markdown ?? '');

    const state = {
        dotnetRef,
        renderedRange: null,
        sourceStart: 0,
        sourceEnd: 0,
        onRenderedInput: null,
        onRenderedFocus: null,
        onRenderedBlur: null,
        onRenderedSelectionChange: null,
        onSourceInput: null,
        onSourceFocus: null,
        onSourceBlur: null,
        onSourceSelectionChange: null,
        onSourceSelect: null,
        onDocumentSelectionChange: null,
    };

    const saveRenderedSelection = () => {
        const selection = window.getSelection();
        if (selection && selection.rangeCount > 0 && renderedEl.contains(selection.anchorNode)) {
            state.renderedRange = selection.getRangeAt(0).cloneRange();
        }
    };

    const saveSourceSelection = () => {
        state.sourceStart = typeof sourceEl.selectionStart === 'number' ? sourceEl.selectionStart : 0;
        state.sourceEnd = typeof sourceEl.selectionEnd === 'number' ? sourceEl.selectionEnd : state.sourceStart;
    };

    state.onRenderedInput = () => {
        saveRenderedSelection();
        const markdownValue = htmlToMarkdown(renderedEl.innerHTML);
        if (sourceEl.value !== markdownValue) {
            sourceEl.value = markdownValue;
        }
        dotnetRef.invokeMethodAsync('OnContentChanged', markdownValue);
    };

    state.onRenderedFocus = () => {
        saveRenderedSelection();
        dotnetRef.invokeMethodAsync('OnFocusChanged', true);
    };

    state.onRenderedBlur = () => {
        saveRenderedSelection();
        dotnetRef.invokeMethodAsync('OnFocusChanged', false);
    };

    state.onRenderedSelectionChange = () => {
        saveRenderedSelection();
    };

    state.onSourceInput = () => {
        saveSourceSelection();
        renderedEl.innerHTML = renderMarkdown(sourceEl.value ?? '');
        dotnetRef.invokeMethodAsync('OnContentChanged', sourceEl.value);
    };

    state.onSourceFocus = () => {
        saveSourceSelection();
        dotnetRef.invokeMethodAsync('OnFocusChanged', true);
    };

    state.onSourceBlur = () => {
        saveSourceSelection();
        dotnetRef.invokeMethodAsync('OnFocusChanged', false);
    };

    state.onSourceSelectionChange = () => {
        saveSourceSelection();
    };

    state.onSourceSelect = () => {
        saveSourceSelection();
    };

    state.onDocumentSelectionChange = () => {
        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0) {
            return;
        }

        const activeElement = document.activeElement;
        if (activeElement === renderedEl || renderedEl.contains(activeElement)) {
            saveRenderedSelection();
            return;
        }

        if (activeElement === sourceEl) {
            saveSourceSelection();
        }
    };

    renderedEl.addEventListener('input', state.onRenderedInput);
    renderedEl.addEventListener('focus', state.onRenderedFocus);
    renderedEl.addEventListener('blur', state.onRenderedBlur);
    renderedEl.addEventListener('keyup', state.onRenderedSelectionChange);
    renderedEl.addEventListener('mouseup', state.onRenderedSelectionChange);

    sourceEl.addEventListener('input', state.onSourceInput);
    sourceEl.addEventListener('focus', state.onSourceFocus);
    sourceEl.addEventListener('blur', state.onSourceBlur);
    sourceEl.addEventListener('keyup', state.onSourceSelectionChange);
    sourceEl.addEventListener('mouseup', state.onSourceSelectionChange);
    sourceEl.addEventListener('select', state.onSourceSelect);

    document.addEventListener('selectionchange', state.onDocumentSelectionChange);

    state.mode = 'rendered';
    state.renderedEl = renderedEl;
    state.sourceEl = sourceEl;
    _editorMap.set(renderedEl, state);
    _editorMap.set(sourceEl, state);
}

export function setValue(renderedEl, sourceEl, markdown, notifyDotNet = true) {
    const value = markdown ?? '';
    sourceEl.value = value;
    renderedEl.innerHTML = renderMarkdown(value);

    const state = _editorMap.get(renderedEl) || _editorMap.get(sourceEl);
    if (state && notifyDotNet) {
        state.dotnetRef.invokeMethodAsync('OnContentChanged', value);
    }
}

export function getValue(renderedEl, sourceEl) {
    return sourceEl.value ?? '';
}

export function execCommand(renderedEl, sourceEl, command, value, renderedView) {
    if (!renderedEl || !sourceEl) {
        return;
    }

    const state = renderedView ? _editorMap.get(renderedEl) : _editorMap.get(sourceEl);
    if (!state) {
        return;
    }

    if (renderedView) {
        const selection = window.getSelection();
        if (state.renderedRange && selection) {
            selection.removeAllRanges();
            selection.addRange(state.renderedRange);
        }

        if (command === 'bold' || command === 'italic') {
            const tagName = command === 'bold' ? 'strong' : 'em';
            if (!toggleRenderedInlineFormat(renderedEl, state, tagName)) {
                return;
            }
        } else if (command === 'heading') {
            if (!applyRenderedHeading(renderedEl, state, value)) {
                return;
            }
        } else {
            renderedEl.focus();
            if (state.renderedRange && selection) {
                selection.removeAllRanges();
                selection.addRange(state.renderedRange);
            }

            document.execCommand(command, false, value ?? null);
        }

        const markdownValue = htmlToMarkdown(renderedEl.innerHTML);
        sourceEl.value = markdownValue;
        saveRenderedRange(state, renderedEl);
        state.dotnetRef.invokeMethodAsync('OnContentChanged', markdownValue);
        return;
    }

    const currentValue = sourceEl.value ?? '';
    const sourceFocused = document.activeElement === sourceEl;
    if (sourceFocused) {
        const start = typeof sourceEl.selectionStart === 'number' ? sourceEl.selectionStart : 0;
        const end = typeof sourceEl.selectionEnd === 'number' ? sourceEl.selectionEnd : start;
        state.sourceStart = start;
        state.sourceEnd = end;
    }

    const start = typeof state.sourceStart === 'number' ? state.sourceStart : 0;
    const end = typeof state.sourceEnd === 'number' ? state.sourceEnd : start;
    const selectionStart = Math.min(start, end);
    const selectionEnd = Math.max(start, end);

    const replacement = _applyCommand(currentValue, command, value, selectionStart, selectionEnd);
    sourceEl.value = replacement.value;

    if (typeof sourceEl.setSelectionRange === 'function') {
        sourceEl.setSelectionRange(replacement.selectionStart, replacement.selectionEnd);
    }

    state.sourceStart = replacement.selectionStart;
    state.sourceEnd = replacement.selectionEnd;
    renderedEl.innerHTML = renderMarkdown(sourceEl.value ?? '');
    notifySourceChangedAfterInterop(sourceEl, replacement.selectionStart, replacement.selectionEnd);
}

export function execCommandById(editorId, command, value, renderedView) {
    const renderedEl = document.getElementById(`${editorId}-rendered`);
    const sourceEl = document.getElementById(`${editorId}-source`);
    execCommand(renderedEl, sourceEl, command, value, renderedView);
}

export function dispose(renderedEl, sourceEl) {
    const state = _editorMap.get(renderedEl) || _editorMap.get(sourceEl);
    if (state) {
        state.renderedEl.removeEventListener('input', state.onRenderedInput);
        state.renderedEl.removeEventListener('focus', state.onRenderedFocus);
        state.renderedEl.removeEventListener('blur', state.onRenderedBlur);
        state.renderedEl.removeEventListener('keyup', state.onRenderedSelectionChange);
        state.renderedEl.removeEventListener('mouseup', state.onRenderedSelectionChange);

        state.sourceEl.removeEventListener('input', state.onSourceInput);
        state.sourceEl.removeEventListener('focus', state.onSourceFocus);
        state.sourceEl.removeEventListener('blur', state.onSourceBlur);
        state.sourceEl.removeEventListener('keyup', state.onSourceSelectionChange);
        state.sourceEl.removeEventListener('mouseup', state.onSourceSelectionChange);
        state.sourceEl.removeEventListener('select', state.onSourceSelect);
        document.removeEventListener('selectionchange', state.onDocumentSelectionChange);

        _editorMap.delete(state.renderedEl);
        _editorMap.delete(state.sourceEl);
    }
}

function _applyCommand(text, command, value, start, end) {
    switch (command) {
        case 'bold':
            return _toggleWrapSelection(text, start, end, '**', '**');
        case 'italic':
            return _toggleWrapSelection(text, start, end, '*', '*');
        case 'fontName':
            return _wrapSelection(text, start, end, `<span style="font-family:${_escapeAttribute(value)};">`, '</span>');
        case 'fontSize':
            return _wrapSelection(text, start, end, `<span style="font-size:${_fontSizeToCss(value)};">`, '</span>');
        case 'foreColor':
            return _wrapSelection(text, start, end, `<span style="color:${_escapeAttribute(value)};">`, '</span>');
        case 'hiliteColor':
            return _wrapSelection(text, start, end, `<span style="background-color:${_escapeAttribute(value)};">`, '</span>');
        case 'justifyLeft':
            return _wrapSelection(text, start, end, '<div style="text-align:left;">', '</div>');
        case 'justifyCenter':
            return _wrapSelection(text, start, end, '<div style="text-align:center;">', '</div>');
        case 'justifyRight':
            return _wrapSelection(text, start, end, '<div style="text-align:right;">', '</div>');
        case 'insertOrderedList':
            return _prefixSelectedLines(text, start, end, '1. ');
        case 'insertUnorderedList':
            return _prefixSelectedLines(text, start, end, '- ');
        case 'heading':
            return _applyHeading(text, start, end, value);
        case 'removeFormat':
            return _removeFormatting(text, start, end);
        default:
            return { value: text, selectionStart: start, selectionEnd: end };
    }
}

function _wrapSelection(text, start, end, prefix, suffix) {
    const selected = text.slice(start, end);
    const nextValue = `${text.slice(0, start)}${prefix}${selected}${suffix}${text.slice(end)}`;
    const selectionStart = start + prefix.length;
    const selectionEnd = selectionStart + selected.length;

    return {
        value: nextValue,
        selectionStart,
        selectionEnd,
    };
}

function notifySourceChangedAfterInterop(sourceEl, selectionStart, selectionEnd) {
    window.setTimeout(() => {
        sourceEl.focus({ preventScroll: true });

        if (typeof sourceEl.setSelectionRange === 'function') {
            sourceEl.setSelectionRange(selectionStart, selectionEnd);
        }

        sourceEl.dispatchEvent(new Event('input', { bubbles: true }));
    }, 0);
}

function _toggleWrapSelection(text, start, end, prefix, suffix) {
    if (start >= prefix.length && text.slice(start - prefix.length, start) === prefix && text.slice(end, end + suffix.length) === suffix) {
        const nextValue = `${text.slice(0, start - prefix.length)}${text.slice(start, end)}${text.slice(end + suffix.length)}`;
        return {
            value: nextValue,
            selectionStart: start - prefix.length,
            selectionEnd: end - prefix.length,
        };
    }

    const selected = text.slice(start, end);
    if (selected.startsWith(prefix) && selected.endsWith(suffix) && selected.length >= prefix.length + suffix.length) {
        const unwrapped = selected.slice(prefix.length, selected.length - suffix.length);
        const nextValue = `${text.slice(0, start)}${unwrapped}${text.slice(end)}`;
        return {
            value: nextValue,
            selectionStart: start,
            selectionEnd: start + unwrapped.length,
        };
    }

    return _wrapSelection(text, start, end, prefix, suffix);
}

function _prefixSelectedLines(text, start, end, prefix) {
    const selected = text.slice(start, end);
    const lines = selected.length > 0 ? selected.split('\n') : [''];
    const nextSelection = lines.map((line) => `${prefix}${line}`).join('\n');
    const nextValue = `${text.slice(0, start)}${nextSelection}${text.slice(end)}`;

    return {
        value: nextValue,
        selectionStart: start,
        selectionEnd: start + nextSelection.length,
    };
}

function _removeFormatting(text, start, end) {
    const selected = text.slice(start, end);
    const cleaned = selected
        .replace(/\*\*(.*?)\*\*/gs, '$1')
        .replace(/\*(.*?)\*/gs, '$1')
        .replace(/<span[^>]*>/gi, '')
        .replace(/<\/span>/gi, '')
        .replace(/<div[^>]*>/gi, '')
        .replace(/<\/div>/gi, '')
        .replace(/^\s*(?:[-*+]|\d+\.)\s+/gm, '');

    const nextValue = `${text.slice(0, start)}${cleaned}${text.slice(end)}`;

    return {
        value: nextValue,
        selectionStart: start,
        selectionEnd: start + cleaned.length,
    };
}

function _fontSizeToCss(value) {
    switch (`${value ?? ''}`) {
        case '1':
            return '11px';
        case '2':
            return '13px';
        case '3':
            return '16px';
        case '4':
            return '18px';
        case '5':
            return '24px';
        case '6':
            return '32px';
        case '7':
            return '48px';
        default:
            return '16px';
    }
}

function _escapeAttribute(value) {
    return `${value ?? ''}`.replace(/"/g, '&quot;');
}

function saveRenderedRange(state, renderedEl) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount > 0 && renderedEl.contains(selection.anchorNode)) {
        state.renderedRange = selection.getRangeAt(0).cloneRange();
    }
}

function toggleRenderedInlineFormat(renderedEl, state, tagName) {
    const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) {
        return false;
    }

    const range = selection.getRangeAt(0);
    if (range.collapsed || !renderedEl.contains(range.commonAncestorContainer)) {
        return false;
    }

    const formattedElement = findFormattedAncestor(range, renderedEl, tagName);
    if (formattedElement) {
        unwrapElement(formattedElement);
        const nextRange = document.createRange();
        nextRange.selectNodeContents(renderedEl);
        selection.removeAllRanges();
        selection.addRange(nextRange);
        state.renderedRange = nextRange.cloneRange();
        renderedEl.focus();
        return true;
    }

    const wrapper = document.createElement(tagName);
    try {
        range.surroundContents(wrapper);
    } catch {
        wrapper.appendChild(range.extractContents());
        range.insertNode(wrapper);
    }

    const nextRange = document.createRange();
    nextRange.selectNodeContents(wrapper);
    selection.removeAllRanges();
    selection.addRange(nextRange);
    state.renderedRange = nextRange.cloneRange();
    renderedEl.focus();
    return true;
}

function findFormattedAncestor(range, renderedEl, tagName) {
    const aliases = tagName === 'strong' ? ['strong', 'b'] : ['em', 'i'];
    const startElement = nearestElement(range.startContainer);
    const endElement = nearestElement(range.endContainer);
    const startFormatted = closestWithin(startElement, renderedEl, aliases);
    const endFormatted = closestWithin(endElement, renderedEl, aliases);

    if (startFormatted && startFormatted === endFormatted) {
        return startFormatted;
    }

    const commonElement = nearestElement(range.commonAncestorContainer);
    return closestWithin(commonElement, renderedEl, aliases);
}

function nearestElement(node) {
    if (!node) {
        return null;
    }

    return node.nodeType === Node.ELEMENT_NODE ? node : node.parentElement;
}

function closestWithin(element, boundary, tagNames) {
    let current = element;
    while (current && current !== boundary) {
        if (tagNames.includes(current.tagName.toLowerCase())) {
            return current;
        }

        current = current.parentElement;
    }

    return null;
}

function unwrapElement(element) {
    const parent = element.parentNode;
    if (!parent) {
        return;
    }

    while (element.firstChild) {
        parent.insertBefore(element.firstChild, element);
    }

    parent.removeChild(element);
}

function applyRenderedHeading(renderedEl, state, headingValue) {
    const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) {
        return false;
    }

    const range = selection.getRangeAt(0);
    if (range.collapsed || !renderedEl.contains(range.commonAncestorContainer)) {
        return false;
    }

    const tagName = `${headingValue ?? ''}`.toLowerCase();
    if (!['h1', 'h2', 'h3'].includes(tagName)) {
        return false;
    }

    const wrapper = document.createElement(tagName);
    wrapper.appendChild(range.extractContents());
    range.insertNode(wrapper);

    const nextRange = document.createRange();
    nextRange.selectNodeContents(wrapper);
    selection.removeAllRanges();
    selection.addRange(nextRange);
    state.renderedRange = nextRange.cloneRange();
    renderedEl.focus();
    return true;
}

function _applyHeading(text, start, end, headingValue) {
    const prefix = _headingPrefix(headingValue);
    if (!prefix) {
        return { value: text, selectionStart: start, selectionEnd: end };
    }

    const selected = text.slice(start, end);
    const lines = selected.length > 0 ? selected.split('\n') : [''];
    const nextSelection = lines.map((line) => `${prefix}${line.replace(/^#+\s*/, '')}`).join('\n');
    const nextValue = `${text.slice(0, start)}${nextSelection}${text.slice(end)}`;

    return {
        value: nextValue,
        selectionStart: start,
        selectionEnd: start + nextSelection.length,
    };
}

function _headingPrefix(headingValue) {
    switch (`${headingValue ?? ''}`.toLowerCase()) {
        case 'h1':
            return '# ';
        case 'h2':
            return '## ';
        case 'h3':
            return '### ';
        default:
            return '';
    }
}

function renderMarkdown(markdown) {
    const lines = `${markdown ?? ''}`.replace(/\r\n/g, '\n').split('\n');
    const html = [];
    let inUnorderedList = false;
    let inOrderedList = false;

    const closeLists = () => {
        if (inUnorderedList) {
            html.push('</ul>');
            inUnorderedList = false;
        }

        if (inOrderedList) {
            html.push('</ol>');
            inOrderedList = false;
        }
    };

    for (const rawLine of lines) {
        const line = rawLine.trim();

        if (!line) {
            closeLists();
            continue;
        }

        if (line.startsWith('### ')) {
            closeLists();
            html.push(`<h3>${formatInline(line.slice(4))}</h3>`);
            continue;
        }

        if (line.startsWith('## ')) {
            closeLists();
            html.push(`<h2>${formatInline(line.slice(3))}</h2>`);
            continue;
        }

        if (line.startsWith('# ')) {
            closeLists();
            html.push(`<h1>${formatInline(line.slice(2))}</h1>`);
            continue;
        }

        if (line.startsWith('- ') || line.startsWith('* ')) {
            if (inOrderedList) {
                html.push('</ol>');
                inOrderedList = false;
            }

            if (!inUnorderedList) {
                html.push('<ul>');
                inUnorderedList = true;
            }

            html.push(`<li>${formatInline(line.slice(2))}</li>`);
            continue;
        }

        if (isOrderedListItem(line)) {
            if (inUnorderedList) {
                html.push('</ul>');
                inUnorderedList = false;
            }

            if (!inOrderedList) {
                html.push('<ol>');
                inOrderedList = true;
            }

            html.push(`<li>${formatInline(line.replace(/^\d+\.\s+/, ''))}</li>`);
            continue;
        }

        closeLists();
        html.push(`<p>${formatInline(line)}</p>`);
    }

    closeLists();
    return html.join('');
}

function formatInline(text) {
    return `${text ?? ''}`
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/`(.+?)`/g, '<code>$1</code>')
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.+?)\*/g, '<em>$1</em>');
}

function htmlToMarkdown(html) {
    const container = document.createElement('div');
    container.innerHTML = html ?? '';
    return nodeToMarkdown(container).trim();
}

function nodeToMarkdown(node) {
    if (node.nodeType === Node.TEXT_NODE) {
        return node.textContent ?? '';
    }

    if (node.nodeType !== Node.ELEMENT_NODE) {
        return '';
    }

    const element = node;
    const tag = element.tagName.toLowerCase();
    const children = Array.from(element.childNodes).map(nodeToMarkdown).join('');

    switch (tag) {
        case 'h1':
            return `# ${children}\n\n`;
        case 'h2':
            return `## ${children}\n\n`;
        case 'h3':
            return `### ${children}\n\n`;
        case 'strong':
        case 'b':
            return `**${children}**`;
        case 'em':
        case 'i':
            return `*${children}*`;
        case 'code':
            return `\`${children}\``;
        case 'br':
            return '\n';
        case 'p':
            return `${children}\n\n`;
        case 'ul':
            return Array.from(element.children).map((child) => `- ${nodeToMarkdown(child).trim()}\n`).join('') + '\n';
        case 'ol':
            return Array.from(element.children).map((child, index) => `${index + 1}. ${nodeToMarkdown(child).trim()}\n`).join('') + '\n';
        case 'li':
            return children;
        case 'div':
            return `${children}\n`;
        case 'span':
            return children;
        default:
            return children;
    }
}

function isOrderedListItem(text) {
    return /^\d+\.\s+/.test(text);
}
