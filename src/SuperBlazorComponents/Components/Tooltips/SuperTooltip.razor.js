const instances = new WeakMap();

export function configure(element, options) {
    if (!element) {
        return;
    }

    ensureStyles();
    dispose(element);

    if (options?.disabled || !options?.content || !window.bootstrap?.Tooltip) {
        return;
    }

    const tooltip = new window.bootstrap.Tooltip(element, {
        title: options.content,
        html: true,
        sanitize: false,
        placement: options.placement || "top",
        trigger: options.trigger || "hover focus",
        delay: {
            show: options.delay || 0,
            hide: 0
        },
        customClass: joinClasses("super-tooltip-content", options.cssClass),
        container: "body",
        boundary: "viewport"
    });

    const state = {
        tooltip,
        shownHandler: null,
        clickHandler: null,
        durationTimer: null
    };

    state.shownHandler = () => {
        const tooltipElement = getTooltipElement(tooltip);

        if (tooltipElement && options.style) {
            tooltipElement.setAttribute("style", options.style);
        }

        if (state.durationTimer) {
            window.clearTimeout(state.durationTimer);
        }

        if (options.duration && options.duration > 0) {
            state.durationTimer = window.setTimeout(() => tooltip.hide(), options.duration);
        }
    };

    element.addEventListener("shown.bs.tooltip", state.shownHandler);

    if (options.closeOnDocumentClick) {
        state.clickHandler = event => {
            const tooltipElement = getTooltipElement(tooltip);
            const clickedTarget = event.target;

            if (element.contains(clickedTarget) || tooltipElement?.contains(clickedTarget)) {
                return;
            }

            tooltip.hide();
        };

        document.addEventListener("click", state.clickHandler, true);
    }

    instances.set(element, state);
}

export function show(element) {
    instances.get(element)?.tooltip?.show();
}

export function hide(element) {
    instances.get(element)?.tooltip?.hide();
}

export function toggle(element) {
    instances.get(element)?.tooltip?.toggle();
}

export function dispose(element) {
    const state = instances.get(element);

    if (!state) {
        return;
    }

    if (state.durationTimer) {
        window.clearTimeout(state.durationTimer);
    }

    element.removeEventListener("shown.bs.tooltip", state.shownHandler);

    if (state.clickHandler) {
        document.removeEventListener("click", state.clickHandler, true);
    }

    try {
        state.tooltip.dispose();
    } catch {
        // ignore Bootstrap disposal races during Blazor navigation
    }

    instances.delete(element);
}

function getTooltipElement(tooltip) {
    return tooltip.tip ?? tooltip.getTipElement?.();
}

function joinClasses(...classes) {
    return classes.filter(value => value && `${value}`.trim().length > 0).join(" ");
}

function ensureStyles() {
    if (document.getElementById("super-tooltip-styles")) {
        return;
    }

    const style = document.createElement("style");
    style.id = "super-tooltip-styles";
    style.textContent = `
.super-tooltip-content {
    max-width: min(24rem, calc(100vw - 2rem));
    text-align: left;
}
.super-tooltip-content .tooltip-inner {
    max-width: inherit;
    padding: .55rem .7rem;
    text-align: left;
}
.super-tooltip-content p,
.super-tooltip-content ul,
.super-tooltip-content ol,
.super-tooltip-content pre,
.super-tooltip-content h1,
.super-tooltip-content h2,
.super-tooltip-content h3,
.super-tooltip-content h4,
.super-tooltip-content h5,
.super-tooltip-content h6 {
    margin-top: 0;
    margin-bottom: .35rem;
}
.super-tooltip-content :last-child {
    margin-bottom: 0;
}
.super-tooltip-content ul,
.super-tooltip-content ol {
    padding-left: 1.15rem;
}
.super-tooltip-content code {
    padding: .05rem .25rem;
    border-radius: .25rem;
    background: rgba(255, 255, 255, .16);
    color: inherit;
}
.super-tooltip-content pre {
    overflow: auto;
    padding: .45rem;
    border-radius: .35rem;
    background: rgba(0, 0, 0, .25);
}
.super-tooltip-content pre code {
    padding: 0;
    background: transparent;
}
.super-tooltip-content table {
    width: 100%;
    margin-bottom: .35rem;
    border-collapse: collapse;
    font-size: .875rem;
}
.super-tooltip-content th,
.super-tooltip-content td {
    padding: .2rem .35rem;
    border: 1px solid rgba(255, 255, 255, .25);
    vertical-align: top;
}
.super-tooltip-content th {
    font-weight: 600;
    background: rgba(255, 255, 255, .12);
}`;

    document.head.appendChild(style);
}
