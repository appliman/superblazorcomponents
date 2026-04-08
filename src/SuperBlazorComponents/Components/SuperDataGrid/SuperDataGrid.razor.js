/**
 * VirtualDataGrid - JavaScript Isolation Module
 * Handles column resizing and drag-drop operations
 */

const gridInstances = new Map();

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
        cleanup: []
    };

    gridInstances.set(containerElement, instance);
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
