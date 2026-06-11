const editors = new WeakMap();
let monacoPromise;

export async function renderCode(element, code, language) {
    if (!element) {
        return;
    }

    try {
        const monaco = await loadMonaco();
        const existing = editors.get(element);

        if (existing) {
            existing.setValue(code);
            existing.layout();
            return;
        }

        element.innerHTML = "";
        const editor = monaco.editor.create(element, {
            value: code,
            language: language || "razor",
            readOnly: true,
            minimap: { enabled: false },
            automaticLayout: true,
            scrollBeyondLastLine: false,
            fontSize: 13,
            lineNumbersMinChars: 3,
            theme: document.documentElement.dataset.bsTheme === "dark" ? "vs-dark" : "vs",
            wordWrap: "off"
        });

        editors.set(element, editor);
    } catch {
        element.innerHTML = "";
        const pre = document.createElement("pre");
        pre.textContent = code;
        element.appendChild(pre);
    }
}

export async function copyText(text) {
    if (navigator.clipboard) {
        await navigator.clipboard.writeText(text);
        return;
    }

    const textArea = document.createElement("textarea");
    textArea.value = text;
    textArea.style.position = "fixed";
    textArea.style.opacity = "0";
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand("copy");
    textArea.remove();
}

export function disposeEditors() {
    // Editors are keyed by DOM nodes and are released with their owner component.
}

function loadMonaco() {
    if (window.monaco) {
        return Promise.resolve(window.monaco);
    }

    monacoPromise ??= new Promise((resolve, reject) => {
        const existingLoader = document.querySelector("script[data-monaco-loader]");

        if (existingLoader) {
            waitForRequire(resolve, reject);
            return;
        }

        const loader = document.createElement("script");
        loader.src = "https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs/loader.js";
        loader.dataset.monacoLoader = "true";
        loader.onload = () => waitForRequire(resolve, reject);
        loader.onerror = reject;
        document.head.appendChild(loader);
    });

    return monacoPromise;
}

function waitForRequire(resolve, reject) {
    if (!window.require) {
        reject();
        return;
    }

    window.require.config({
        paths: {
            vs: "https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs"
        }
    });

    window.require(["vs/editor/editor.main"], () => resolve(window.monaco), reject);
}
