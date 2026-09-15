/* Philisa Abantu Bethu — site-wide interactive behaviour. */
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
       Contact form demo success panel (Pages/Contact.cshtml)
       Remove this block once the form posts to a real handler.
    --------------------------------------------------------------- */
    var contactForm = document.querySelector('[data-contact-form]');
    if (contactForm) {
        contactForm.addEventListener('submit', function (e) {
            e.preventDefault();
            contactForm.setAttribute('hidden', '');
            var ok = document.querySelector('[data-contact-success]');
            if (ok) { ok.removeAttribute('hidden'); ok.scrollIntoView({ block: 'center' }); }
        });
    }
})();
