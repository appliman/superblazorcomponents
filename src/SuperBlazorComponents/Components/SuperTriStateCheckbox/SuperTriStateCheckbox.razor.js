export function setIndeterminate(element, value) {
    if (!element) {
        return;
    }

    element.indeterminate = value === true;
}
