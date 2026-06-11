let mermaidPromise;

async function getMermaid() {
    if (!mermaidPromise) {
        mermaidPromise = import("https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs")
            .then(module => module.default);
    }

    const mermaid = await mermaidPromise;
    mermaid.initialize({
        startOnLoad: false,
        securityLevel: "strict",
        theme: document.documentElement.dataset.bsTheme === "dark" ? "dark" : "default",
        flowchart: {
            htmlLabels: true
        }
    });

    return mermaid;
}

export async function renderMermaid(rootElement) {
    if (!rootElement) {
        return;
    }

    const mermaidBlocks = rootElement.querySelectorAll(
        "pre.mermaid, pre > code.language-mermaid, pre > code.lang-mermaid, pre > code.mermaid"
    );

    const diagrams = [];
    mermaidBlocks.forEach((mermaidBlock, index) => {
        const pre = mermaidBlock.matches("pre")
            ? mermaidBlock
            : mermaidBlock.parentElement;

        if (!pre || pre.dataset.mermaidPrepared === "true") {
            return;
        }

        const diagram = document.createElement("div");
        diagram.className = "mermaid documentation-mermaid";
        diagram.id = `documentation-mermaid-${Date.now()}-${index}`;
        diagram.textContent = mermaidBlock.textContent;
        pre.dataset.mermaidPrepared = "true";
        pre.replaceWith(diagram);
        diagrams.push(diagram);
    });

    if (diagrams.length === 0) {
        return;
    }

    try {
        const mermaid = await getMermaid();
        await mermaid.run({ nodes: diagrams });
    } catch (error) {
        diagrams.forEach(diagram => {
            const fallback = document.createElement("pre");
            fallback.className = "documentation-mermaid-error";
            fallback.textContent = `Mermaid rendering failed:\n${error?.message ?? error}\n\n${diagram.textContent}`;
            diagram.replaceWith(fallback);
        });
    }
}
