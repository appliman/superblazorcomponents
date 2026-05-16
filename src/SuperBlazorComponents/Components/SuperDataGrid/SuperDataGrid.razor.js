/**
 * VirtualDataGrid - JavaScript Isolation Module
 * Handles column resizing and drag-drop operations
 */

const gridInstances = new Map();
let overflowPreviewStyleInstalled = false;

/**
 * Initialize the data grid with JS interop
 * @param {HTMLElement} containerElement - The container element
 * @param {object} dotNetRef - DotNet object reference for callbacks
 */
export function initialize(containerElement, dotNetRef) {
    if (!containerElement || !dotNetRef) {
        return;
    }

    const instance = {
        containerElement,
        dotNetRef,
        resizeState: null,
        activeOverflowPreview: null,
        cleanup: []
    };

    gridInstances.set(containerElement, instance);

    installOverflowPreviewStyles();

    const onPointerOver = (e) => handleOverflowPreviewPointerOver(e, instance);
    const onPointerOut = (e) => handleOverflowPreviewPointerOut(e, instance);
    const onMouseOver = (e) => handleOverflowPreviewPointerOver(e, instance);
    const onMouseOut = (e) => handleOverflowPreviewPointerOut(e, instance);
    const onScroll = () => hideOverflowPreview(instance);
    const onResize = () => hideOverflowPreview(instance);

    containerElement.addEventListener('pointerover', onPointerOver);
    containerElement.addEventListener('pointerout', onPointerOut);
    containerElement.addEventListener('mouseover', onMouseOver);
    containerElement.addEventListener('mouseout', onMouseOut);
    containerElement.addEventListener('scroll', onScroll, true);
    window.addEventListener('resize', onResize);

    instance.cleanup.push(
        () => containerElement.removeEventListener('pointerover', onPointerOver),
        () => containerElement.removeEventListener('pointerout', onPointerOut),
        () => containerElement.removeEventListener('mouseover', onMouseOver),
        () => containerElement.removeEventListener('mouseout', onMouseOut),
        () => containerElement.removeEventListener('scroll', onScroll, true),
        () => window.removeEventListener('resize', onResize)
    );
}

function installOverflowPreviewStyles() {
    if (overflowPreviewStyleInstalled) {
        return;
    }

    const style = document.createElement('style');
    style.textContent = `
        .sdg-overflow-preview {
            position: fixed;
            box-sizing: border-box;
            overflow-x: hidden;
            overflow-y: auto;
            padding: 0.25rem 0.35rem;
            border: 1px solid var(--bs-primary, #0d6efd);
            border-radius: var(--bs-border-radius-sm, 0.25rem);
            background: var(--bs-body-bg, #fff);
            color: var(--bs-body-color, #212529);
            box-shadow: 0 0.5rem 1.25rem rgba(0, 0, 0, 0.18);
            font: inherit;
            white-space: normal;
            z-index: 10000;
            scrollbar-gutter: stable;
            scrollbar-width: auto;
            scrollbar-color: var(--bs-secondary, #6c757d) var(--bs-tertiary-bg, #f8f9fa);
        }

        .sdg-overflow-preview * {
            max-width: 100%;
            white-space: normal !important;
            overflow: visible !important;
            text-overflow: clip !important;
        }

        .sdg-overflow-preview::-webkit-scrollbar {
            width: 12px;
        }

        .sdg-overflow-preview::-webkit-scrollbar-track {
            background: var(--bs-tertiary-bg, #f8f9fa);
            border-left: 1px solid var(--bs-border-color, #dee2e6);
        }

        .sdg-overflow-preview::-webkit-scrollbar-thumb {
            background-color: var(--bs-secondary, #6c757d);
            border: 2px solid var(--bs-tertiary-bg, #f8f9fa);
            border-radius: 999px;
        }
    `;
    document.head.appendChild(style);
    overflowPreviewStyleInstalled = true;
}

function handleOverflowPreviewPointerOver(e, instance) {
    const contentElement = getOverflowContentElement(e.target, instance.containerElement);
    if (!contentElement || !instance.containerElement.contains(contentElement)) {
        return;
    }

    if (!instance.containerElement.classList.contains('sdg-fixed-row-height')) {
        return;
    }

    if (instance.activeOverflowPreview?.source === contentElement) {
        return;
    }

    if (!hasVerticalOverflow(contentElement)) {
        hideOverflowPreview(instance);
        return;
    }

    showOverflowPreview(instance, contentElement);
}

function handleOverflowPreviewPointerOut(e, instance) {
    const contentElement = getOverflowContentElement(e.target, instance.containerElement);
    if (!contentElement || !instance.containerElement.contains(contentElement)) {
        return;
    }

    const relatedTarget = e.relatedTarget;
    const previewElement = instance.activeOverflowPreview?.element;
    if (relatedTarget && (contentElement.contains(relatedTarget) || previewElement?.contains(relatedTarget))) {
        return;
    }

    hideOverflowPreview(instance);
}

function hasVerticalOverflow(element) {
    return element.scrollHeight > element.clientHeight + 1;
}

function getOverflowContentElement(target, containerElement) {
    const closest = target?.closest?.('.sdg-cell-content');
    if (closest) {
        return closest;
    }

    const cell = target?.closest?.('td.sdg-cell, td.sdg-vertical-value');
    if (!cell || !containerElement.contains(cell)) {
        return null;
    }

    return cell.querySelector('.sdg-cell-content');
}

function showOverflowPreview(instance, contentElement) {
    hideOverflowPreview(instance);

    const cell = contentElement.closest('td');
    if (!cell) {
        return;
    }

    const cellRect = cell.getBoundingClientRect();
    const contentStyle = window.getComputedStyle(contentElement);
    const preview = document.createElement('div');
    preview.className = 'sdg-overflow-preview';
    preview.style.fontSize = contentStyle.fontSize;
    preview.style.lineHeight = contentStyle.lineHeight;
    preview.style.textAlign = contentStyle.textAlign;
    preview.style.width = `${Math.max(80, cellRect.width)}px`;
    preview.style.left = `${Math.max(4, Math.min(cellRect.left, window.innerWidth - cellRect.width - 4))}px`;

    const clone = contentElement.cloneNode(true);
    clone.classList.remove('sdg-cell-content');
    preview.appendChild(clone);
    document.body.appendChild(preview);

    const margin = 6;
    const viewportPadding = 8;
    const spaceBelow = window.innerHeight - cellRect.top - viewportPadding;
    const spaceAbove = cellRect.bottom - viewportPadding;
    const naturalHeight = preview.scrollHeight;
    const opensDown = spaceBelow >= naturalHeight || spaceBelow >= spaceAbove;
    const maxHeight = Math.max(40, opensDown ? spaceBelow : spaceAbove);
    const height = Math.min(naturalHeight, maxHeight);

    preview.style.maxHeight = `${maxHeight}px`;
    preview.style.height = naturalHeight > maxHeight ? `${maxHeight}px` : 'auto';
    preview.style.top = opensDown
        ? `${Math.max(viewportPadding, cellRect.top)}px`
        : `${Math.max(viewportPadding, cellRect.bottom - height)}px`;

    const onPreviewPointerOut = (e) => {
        const relatedTarget = e.relatedTarget;
        if (relatedTarget && (preview.contains(relatedTarget) || contentElement.contains(relatedTarget))) {
            return;
        }

        hideOverflowPreview(instance);
    };

    preview.addEventListener('pointerout', onPreviewPointerOut);
    instance.activeOverflowPreview = {
        source: contentElement,
        element: preview,
        cleanup: () => preview.removeEventListener('pointerout', onPreviewPointerOut)
    };
}

function hideOverflowPreview(instance) {
    const activePreview = instance.activeOverflowPreview;
    if (!activePreview) {
        return;
    }

    activePreview.cleanup?.();
    activePreview.element?.remove();
    instance.activeOverflowPreview = null;
}

/**
 * Start column resize operation
 * @param {HTMLElement} tableElement - The table element
 * @param {number} columnIndex - The index of the column being resized
 * @param {number} startX - Starting X position of the mouse
 */
export function startResize(tableElement, columnIndex, startX) {
    const containerElement = tableElement.closest('.sdg-container');
    const instance = gridInstances.get(containerElement);

    if (!instance || !tableElement) {
        return;
    }

    const headers = tableElement.querySelectorAll('thead tr:first-child th');
    if (columnIndex < 0 || columnIndex >= headers.length) {
        return;
    }

    const header = headers[columnIndex];
    const startWidth = header.offsetWidth;

    instance.resizeState = {
        columnIndex,
        startX,
        startWidth,
        tableElement,
        header
    };

    // Add visual feedback
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';

    // Add resize class to header
    header.classList.add('sdg-resizing');

    // Create resize guide line
    const guideLine = document.createElement('div');
    guideLine.className = 'sdg-resize-guide';
    guideLine.style.cssText = `
        position: fixed;
        top: 0;
        bottom: 0;
        width: 2px;
        background-color: var(--bs-primary, #0d6efd);
        z-index: 10000;
        pointer-events: none;
    `;
    guideLine.style.left = `${startX}px`;
    document.body.appendChild(guideLine);
    instance.resizeState.guideLine = guideLine;

    // Setup event listeners
    const onMouseMove = (e) => handleResizeMove(e, instance);
    const onMouseUp = (e) => handleResizeEnd(e, instance);

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    instance.cleanup.push(
        () => document.removeEventListener('mousemove', onMouseMove),
        () => document.removeEventListener('mouseup', onMouseUp)
    );
}

/**
 * Handle mouse move during resize
 * @param {MouseEvent} e - The mouse event
 * @param {object} instance - The grid instance
 */
function handleResizeMove(e, instance) {
    if (!instance.resizeState) {
        return;
    }

    const { startX, startWidth, guideLine } = instance.resizeState;
    const delta = e.clientX - startX;
    const newWidth = Math.max(50, startWidth + delta);

    // Update guide line position
    if (guideLine) {
        guideLine.style.left = `${e.clientX}px`;
    }

    // Preview the new width (optional - can be performance heavy)
    // instance.resizeState.header.style.width = `${newWidth}px`;
}

/**
 * Handle mouse up to end resize
 * @param {MouseEvent} e - The mouse event
 * @param {object} instance - The grid instance
 */
async function handleResizeEnd(e, instance) {
    if (!instance.resizeState) {
        return;
    }

    const { columnIndex, startX, startWidth, header, guideLine, tableElement } = instance.resizeState;
    const delta = e.clientX - startX;
    const newWidth = Math.max(50, startWidth + delta);

    // Cleanup
    document.body.style.cursor = '';
    document.body.style.userSelect = '';
    header.classList.remove('sdg-resizing');

    if (guideLine) {
        guideLine.remove();
    }

    // Cleanup event listeners
    instance.cleanup.forEach(fn => fn());
    instance.cleanup = [];

    instance.resizeState = null;

    // Update all cells in this column with the new width
    updateColumnWidth(tableElement, columnIndex, newWidth);

    // Notify .NET
    try {
        await instance.dotNetRef.invokeMethodAsync('OnResizeComplete', columnIndex, newWidth);
    } catch (error) {
        console.error('Failed to notify resize complete:', error);
    }
}

/**
 * Update the width of all cells in a column
 * @param {HTMLElement} tableElement - The table element
 * @param {number} columnIndex - The column index
 * @param {number} width - The new width in pixels
 */
function updateColumnWidth(tableElement, columnIndex, width) {
    const widthStyle = `${width}px`;

    // Update header
    const headers = tableElement.querySelectorAll('thead tr th');
    headers.forEach((row) => {
        const cells = row.parentElement.querySelectorAll('th');
        if (cells[columnIndex]) {
            cells[columnIndex].style.width = widthStyle;
            cells[columnIndex].style.minWidth = widthStyle;
            cells[columnIndex].style.maxWidth = widthStyle;
        }
    });

    // Update body cells
    const bodyRows = tableElement.querySelectorAll('tbody tr');
    bodyRows.forEach((row) => {
        const cells = row.querySelectorAll('td');
        if (cells[columnIndex]) {
            cells[columnIndex].style.width = widthStyle;
            cells[columnIndex].style.minWidth = widthStyle;
            cells[columnIndex].style.maxWidth = widthStyle;
        }
    });

    // Update footer cells
    const footerRows = tableElement.querySelectorAll('tfoot tr');
    footerRows.forEach((row) => {
        const cells = row.querySelectorAll('td');
        if (cells[columnIndex]) {
            cells[columnIndex].style.width = widthStyle;
            cells[columnIndex].style.minWidth = widthStyle;
            cells[columnIndex].style.maxWidth = widthStyle;
        }
    });
}

/**
 * Get the current scroll position
 * @param {HTMLElement} containerElement - The container element
 * @returns {object} - The scroll position { top, left }
 */
export function getScrollPosition(containerElement) {
    const wrapper = containerElement.querySelector('.sdg-table-wrapper');
    if (!wrapper) {
        return { top: 0, left: 0 };
    }
    return {
        top: wrapper.scrollTop,
        left: wrapper.scrollLeft
    };
}

/**
 * Set the scroll position
 * @param {HTMLElement} containerElement - The container element
 * @param {number} top - The top scroll position
 * @param {number} left - The left scroll position
 */
export function setScrollPosition(containerElement, top, left) {
    const wrapper = containerElement.querySelector('.sdg-table-wrapper');
    if (wrapper) {
        wrapper.scrollTop = top;
        wrapper.scrollLeft = left;
    }
}

/**
 * Scroll to a specific row
 * @param {HTMLElement} containerElement - The container element
 * @param {number} rowIndex - The row index to scroll to
 * @param {number} rowHeight - The estimated row height
 */
export function scrollToRow(containerElement, rowIndex, rowHeight) {
    const wrapper = containerElement.querySelector('.sdg-table-wrapper');
    if (wrapper) {
        const targetTop = rowIndex * rowHeight;
        wrapper.scrollTop = targetTop;
    }
}

/**
 * Export grid data to clipboard
 * @param {HTMLElement} tableElement - The table element
 * @param {boolean} includeHeaders - Whether to include headers
 */
export async function copyToClipboard(tableElement, includeHeaders = true) {
    const rows = [];

    if (includeHeaders) {
        const headerCells = tableElement.querySelectorAll('thead tr:first-child th .sdg-header-text');
        const headerRow = Array.from(headerCells).map(cell => cell.textContent.trim());
        rows.push(headerRow.join('\t'));
    }

    const bodyRows = tableElement.querySelectorAll('tbody tr:not(.sdg-placeholder-row)');
    bodyRows.forEach(row => {
        const cells = row.querySelectorAll('td');
        const rowData = Array.from(cells).map(cell => cell.textContent.trim());
        rows.push(rowData.join('\t'));
    });

    const text = rows.join('\n');

    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (error) {
        console.error('Failed to copy to clipboard:', error);
        return false;
    }
}

/**
 * Dispose the grid instance and cleanup
 * @param {HTMLElement} containerElement - The container element
 */
export function dispose(containerElement) {
    const instance = gridInstances.get(containerElement);
    if (instance) {
        // Cleanup any active operations
        if (instance.resizeState) {
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
            if (instance.resizeState.guideLine) {
                instance.resizeState.guideLine.remove();
            }
        }

        // Cleanup event listeners
        instance.cleanup.forEach(fn => fn());

        gridInstances.delete(containerElement);
    }
}

/**
 * Debug: Log column widths and offsets
 * @param {HTMLElement} containerElement - The container element
 */
export function debugColumnPositions(containerElement) {
    const tableElement = containerElement.querySelector('table');
    if (!tableElement) {
        console.log('No table found');
        return;
    }

    const headers = tableElement.querySelectorAll('thead tr:first-child th');
    console.log('=== Column Debug Info ===');
    headers.forEach((header, index) => {
        const rect = header.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(header);
        console.log(`Column ${index}:`, {
            width: `${header.offsetWidth}px`,
            computedWidth: computedStyle.width,
            left: computedStyle.left,
            position: computedStyle.position,
            boxSizing: computedStyle.boxSizing,
            borderLeft: computedStyle.borderLeftWidth,
            borderRight: computedStyle.borderRightWidth,
            paddingLeft: computedStyle.paddingLeft,
            paddingRight: computedStyle.paddingRight,
            actualPosition: rect.left
        });
    });
}
