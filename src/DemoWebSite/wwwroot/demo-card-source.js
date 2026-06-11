import { copyText, renderCode } from "./demo-example.js";

let pending;

document.addEventListener("DOMContentLoaded", enhanceCards);
document.addEventListener("enhancedload", enhanceCards);

const observer = new MutationObserver(() => {
    window.clearTimeout(pending);
    pending = window.setTimeout(enhanceCards, 100);
});

observer.observe(document.body, { childList: true, subtree: true });

async function enhanceCards() {
    const cards = [...document.querySelectorAll(".card.mt-3:not(.demo-example):not([data-demo-source])")];

    if (cards.length === 0) {
        return;
    }

    const response = await fetch(`/demo-source?route=${encodeURIComponent(location.pathname)}`);
    if (!response.ok) {
        return;
    }

    const payload = await response.json();

    cards.forEach((card, index) => {
        const source = payload.cards?.[index]?.code;
        if (source) {
            enhanceCard(card, source);
        }
    });
}

function enhanceCard(card, source) {
    card.dataset.demoSource = "true";
    card.classList.add("demo-source-card");

    const header = card.querySelector(".card-header");
    const body = card.querySelector(".card-body");

    if (!body) {
        return;
    }

    const title = header?.textContent?.trim() || "Example";
    const tabs = document.createElement("div");
    tabs.className = "demo-source-tabs";
    tabs.setAttribute("role", "tablist");
    tabs.setAttribute("aria-label", title);
    tabs.innerHTML = `
        <button type="button" class="demo-source-tab active" role="tab" aria-selected="true">Exemple</button>
        <button type="button" class="demo-source-tab" role="tab" aria-selected="false">Code</button>
    `;

    const toolbar = document.createElement("div");
    toolbar.className = "demo-source-toolbar";
    toolbar.innerHTML = `
        <button type="button" class="btn btn-sm btn-link demo-source-action" data-demo-run>
            <i class="fa-solid fa-play" aria-hidden="true"></i><span>Run</span>
        </button>
        <button type="button" class="btn btn-sm btn-link demo-source-action" data-demo-copy>
            <i class="fa-regular fa-copy" aria-hidden="true"></i><span>Copier</span>
        </button>
    `;

    const codeHost = document.createElement("div");
    codeHost.className = "demo-source-code";
    codeHost.style.height = "420px";
    codeHost.hidden = true;

    if (header) {
        header.after(tabs, toolbar);
    } else {
        card.prepend(tabs, toolbar);
    }

    body.after(codeHost);

    const exampleTab = tabs.children[0];
    const codeTab = tabs.children[1];
    const runButton = toolbar.querySelector("[data-demo-run]");
    const copyButton = toolbar.querySelector("[data-demo-copy]");

    exampleTab.addEventListener("click", () => showExample(exampleTab, codeTab, body, codeHost));
    runButton.addEventListener("click", () => showExample(exampleTab, codeTab, body, codeHost));
    codeTab.addEventListener("click", async () => {
        body.hidden = true;
        codeHost.hidden = false;
        exampleTab.classList.remove("active");
        exampleTab.setAttribute("aria-selected", "false");
        codeTab.classList.add("active");
        codeTab.setAttribute("aria-selected", "true");
        await renderCode(codeHost, source, "razor");
    });
    copyButton.addEventListener("click", () => copyText(source));
}

function showExample(exampleTab, codeTab, body, codeHost) {
    body.hidden = false;
    codeHost.hidden = true;
    exampleTab.classList.add("active");
    exampleTab.setAttribute("aria-selected", "true");
    codeTab.classList.remove("active");
    codeTab.setAttribute("aria-selected", "false");
}
