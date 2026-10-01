// Small helpers for the document panes: jump-to-edit with a flash, and proportional synchronised scrolling.
window.redactorUi = (() => {
    let sync = true, busy = false;
    // The two document panes (original and redacted).
    const panes = () => ['pane-original', 'pane-redacted'].map(id => document.getElementById(id));

    // Keeps the other pane at the same proportional scroll position.
    function onScroll(src, dst) {
        if (!sync || busy || !src || !dst) return;
        busy = true;
        const max = src.scrollHeight - src.clientHeight;
        const ratio = max > 0 ? src.scrollTop / max : 0;
        dst.scrollTop = ratio * (dst.scrollHeight - dst.clientHeight);
        requestAnimationFrame(() => { busy = false; });
    }

    return {
        // Starts listening for scrolls on the panes (called once after the first render).
        init() {
            // Panes are re-created by Blazor, so use delegation on the document for scroll events (capture phase).
            document.addEventListener('scroll', e => {
                const [a, b] = panes();
                if (e.target === a) onScroll(a, b); else if (e.target === b) onScroll(b, a);
            }, true);
        },
        // Turns synchronised scrolling on or off.
        setSync(on) { sync = on; },
        // Scrolls both panes to the edit with this id and flashes it.
        jumpTo(id) {
            for (const pane of panes()) {
                const el = pane && pane.querySelector(`mark[data-id="${id}"]`);
                if (!el) continue;
                busy = true; // do not let the two scrolls fight
                el.scrollIntoView({ block: 'center', behavior: 'smooth' });
                el.classList.remove('flash'); void el.offsetWidth; el.classList.add('flash');
                setTimeout(() => { busy = false; }, 600);
            }
        },
    };
})();
