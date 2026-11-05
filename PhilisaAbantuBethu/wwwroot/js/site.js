/* Philisa Abafazi Bethu — site-wide interactive behaviour. */
(function () {
    'use strict';

    /* ---------------------------------------------------------------
       Mobile navigation (header hamburger, Pages/Shared/_Layout.cshtml)
    --------------------------------------------------------------- */
    document.querySelectorAll('[data-nav-toggle]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var panel = document.getElementById('mobile-nav');
            if (!panel) return;
            var open = panel.hasAttribute('hidden');
            if (open) { panel.removeAttribute('hidden'); } else { panel.setAttribute('hidden', ''); }
            btn.setAttribute('aria-expanded', String(open));
        });
    });

    /* ---------------------------------------------------------------
       FAQ accordion (Pages/Resources.cshtml) — single panel open at a time
    --------------------------------------------------------------- */
    document.querySelectorAll('[data-faq-toggle]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var panel = document.getElementById(btn.getAttribute('aria-controls'));
            var icon = btn.querySelector('[data-faq-icon]');
            var isOpen = btn.getAttribute('aria-expanded') === 'true';

            document.querySelectorAll('[data-faq-toggle]').forEach(function (other) {
                if (other === btn) return;
                other.setAttribute('aria-expanded', 'false');
                var op = document.getElementById(other.getAttribute('aria-controls'));
                if (op) op.setAttribute('hidden', '');
                var oi = other.querySelector('[data-faq-icon]');
                if (oi) oi.classList.remove('rotate-180');
            });

            btn.setAttribute('aria-expanded', String(!isOpen));
            if (panel) { if (isOpen) { panel.setAttribute('hidden', ''); } else { panel.removeAttribute('hidden'); } }
            if (icon) icon.classList.toggle('rotate-180', !isOpen);
        });
    });

    /* ---------------------------------------------------------------
       Invisible characters (G7). Text pasted from other apps can carry
       control codes the server refuses outright (a null makes the whole
       form unreadable), or direction marks that make a name display
       backwards. They are removed before sending, so nothing typed is
       lost. The server removes them too, for anyone without JavaScript.
    --------------------------------------------------------------- */
    var invisible = /[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F-\u009F‪-‮⁦-⁩]/g;
    document.addEventListener('submit', function (e) {
        e.target.querySelectorAll('input:not([type]), input[type="text"], input[type="tel"], input[type="email"], textarea')
            .forEach(function (field) {
                if (invisible.test(field.value)) field.value = field.value.replace(invisible, '');
                invisible.lastIndex = 0;
            });
    }, true);

    /* ---------------------------------------------------------------
       Busy state for simple forms (Pages/Contact.cshtml): one message
       per press, and the button shows that it is sending.
    --------------------------------------------------------------- */
    document.querySelectorAll('[data-busy-form]').forEach(function (form) {
        var button = form.querySelector('button[type="submit"]');
        if (!button) return;
        var label = button.innerHTML;

        form.addEventListener('submit', function (e) {
            if (button.disabled) { e.preventDefault(); return; }
            button.disabled = true;
            button.setAttribute('aria-busy', 'true');
            button.textContent = button.dataset.busyLabel || 'Sending…';
        });

        // Coming back with the browser's Back button can restore the locked button.
        window.addEventListener('pageshow', function () {
            button.disabled = false;
            button.removeAttribute('aria-busy');
            button.innerHTML = label;
        });
    });
})();

/* ---------------------------------------------------------------
   Keeping the emergency numbers reachable with no data (W11).

   The worker is deliberately narrow about what it keeps (see sw.js):
   public phone numbers, and nothing anybody typed. The Resources page
   only says the numbers are saved once the browser has really saved
   them.
--------------------------------------------------------------- */
(function () {
    'use strict';

    if (!('serviceWorker' in navigator)) return;

    var offlineReady = document.querySelector('[data-offline-ready]');

    navigator.serviceWorker.register('/sw.js').then(function () {
        if (offlineReady) offlineReady.removeAttribute('hidden');
    }).catch(function () {
        // No offline copy. Everything still works with data, so there is nothing to say.
    });
})();

/* ---------------------------------------------------------------
   Copying the reference number off the confirmation page (G6).

   Six characters read off a screen and written down is easy to get
   wrong, and the phone may need to go away quickly. Falls back to
   selecting the text where the clipboard is not allowed.
--------------------------------------------------------------- */
(function () {
    'use strict';

    var button = document.querySelector('[data-copy-reference]');
    if (!button) return;

    var label = button.querySelector('[data-copy-label]');
    var original = label ? label.textContent : '';

    function done() {
        if (!label) return;
        label.textContent = button.dataset.copiedLabel || 'Copied';
        window.setTimeout(function () { label.textContent = original; }, 2500);
    }

    button.addEventListener('click', function () {
        var reference = button.dataset.reference || '';

        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(reference).then(done, fallback);
        } else {
            fallback();
        }

        function fallback() {
            // An off-screen field is the only way to copy without the clipboard API.
            var field = document.createElement('textarea');
            field.value = reference;
            field.setAttribute('readonly', '');
            field.style.position = 'absolute';
            field.style.left = '-9999px';
            document.body.appendChild(field);
            field.select();
            try { document.execCommand('copy'); done(); } catch (e) { /* Nothing more to try. */ }
            document.body.removeChild(field);
        }
    });
})();
