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

    // Text offset of a point inside the original pane's text, or null if it is not in the document text. Every piece of text is wrapped in
    // an element carrying data-start (its offset in the document), so a point inside a text node is that offset plus the offset in the node.
    function textOffset(node, off, isEnd, pane) {
        if (node.nodeType === 3) {
            const p = node.parentElement && node.parentElement.closest('[data-start]');
            return p && pane.contains(p) ? +p.dataset.start + off : null;
        }
        const at = (el) => (el && el.nodeType === 1 ? (el.matches('[data-start]') ? el : el.querySelector('[data-start]')) : null);
        if (isEnd) {   // the end of the piece just before this boundary
            for (let i = off - 1; i >= 0; i--) {
                const kids = node.childNodes[i]; const el = kids && kids.nodeType === 1 ? (kids.matches('[data-start]') ? kids : [...kids.querySelectorAll('[data-start]')].pop()) : null;
                if (el) return +el.dataset.start + el.textContent.length;
            }
            return null;
        }
        for (let i = off; i < node.childNodes.length; i++) { const el = at(node.childNodes[i]); if (el) return +el.dataset.start; }
        return null;
    }

    return {
        // Tells .NET when the person selects text in the original pane (offset, length and where to show the redact bar).
        watchSelection(dotnet) {
            document.addEventListener('mouseup', () => {
                const pane = document.getElementById('pane-original'); const sel = window.getSelection();
                if (!pane || !sel || sel.isCollapsed || sel.rangeCount === 0) return;
                const r = sel.getRangeAt(0);
                const a = textOffset(r.startContainer, r.startOffset, false, pane), b = textOffset(r.endContainer, r.endOffset, true, pane);
                if (a === null || b === null || b <= a) return;
                const rect = r.getBoundingClientRect();
                dotnet.invokeMethodAsync('OnTextSelected', a, b - a, Math.max(8, Math.min(rect.left, window.innerWidth - 460)), Math.min(rect.bottom + 8, window.innerHeight - 70));
            });
        },
        // Clears the browser's text selection (after a selection has been redacted or dismissed).
        clearSelection() { const s = window.getSelection(); if (s) s.removeAllRanges(); },
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
