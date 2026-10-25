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
       Programme gallery lightbox (Pages/ProgrammeDetail.cshtml)
    --------------------------------------------------------------- */
    var lightbox = document.querySelector('[data-gallery-lightbox]');
    if (lightbox) {
        var lightboxImage = lightbox.querySelector('[data-gallery-lightbox-image]');

        var openLightbox = function (src) {
            lightboxImage.setAttribute('src', src);
            lightbox.removeAttribute('hidden');
        };
        var closeLightbox = function () {
            lightbox.setAttribute('hidden', '');
            lightboxImage.setAttribute('src', '');
        };

        document.querySelectorAll('[data-gallery-trigger]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                openLightbox(btn.getAttribute('data-full-src'));
            });
        });

        var closeBtn = lightbox.querySelector('[data-gallery-close]');
        if (closeBtn) closeBtn.addEventListener('click', closeLightbox);

        lightbox.addEventListener('click', function (e) {
            if (e.target === lightbox) closeLightbox();
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && !lightbox.hasAttribute('hidden')) {
                // Marked as used, so closing a photo never counts towards a quick exit.
                e.preventDefault();
                closeLightbox();
            }
        });
    }

    /* ---------------------------------------------------------------
       Testimonial carousel (Pages/ProgrammeDetail.cshtml)
    --------------------------------------------------------------- */
    var testimonialCarousel = document.querySelector('[data-testimonial-carousel]');
    if (testimonialCarousel) {
        var slides = Array.prototype.slice.call(testimonialCarousel.querySelectorAll('[data-testimonial-slide]'));
        var dots = Array.prototype.slice.call(testimonialCarousel.querySelectorAll('[data-testimonial-dot]'));
        var current = 0;

        var showSlide = function (index) {
            current = (index + slides.length) % slides.length;
            slides.forEach(function (slide, i) {
                if (i === current) { slide.removeAttribute('hidden'); slide.classList.remove('hidden'); }
                else { slide.setAttribute('hidden', ''); slide.classList.add('hidden'); }
            });
            dots.forEach(function (dot, i) {
                dot.classList.toggle('bg-brand-700', i === current);
                dot.classList.toggle('bg-brand-100', i !== current);
            });
        };

        var prevBtn = testimonialCarousel.querySelector('[data-testimonial-prev]');
        var nextBtn = testimonialCarousel.querySelector('[data-testimonial-next]');
        if (prevBtn) prevBtn.addEventListener('click', function () { showSlide(current - 1); });
        if (nextBtn) nextBtn.addEventListener('click', function () { showSlide(current + 1); });
        dots.forEach(function (dot, i) {
            dot.addEventListener('click', function () { showSlide(i); });
        });
    }

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

    /* ---------------------------------------------------------------
       Quick exit (Pages/Shared/_Layout.cshtml). Someone may need to leave
       this site at once if the person hurting them walks in. The button,
       or Esc twice, swaps the page for a neutral one; replace() means the
       Back button doesn't return here.
    --------------------------------------------------------------- */
    var quickExit = document.querySelector('[data-quick-exit]');
    if (quickExit) {
        var leave = function () {
            window.location.replace(quickExit.getAttribute('href'));
        };

        quickExit.addEventListener('click', function (e) {
            e.preventDefault();
            leave();
        });

        // Twice, so an Esc meant for a dropdown doesn't throw away a half-written request.
        var lastEscape = 0;
        document.addEventListener('keydown', function (e) {
            if (e.key !== 'Escape' || e.defaultPrevented) return;
            var now = Date.now();
            if (now - lastEscape < 1000) { leave(); return; }
            lastEscape = now;
        });
    }
})();

/* ---------------------------------------------------------------
   Keeping the emergency numbers reachable with no data (W11).

   The worker is deliberately narrow about what it keeps (see sw.js):
   public phone numbers, and nothing anybody typed. Because a saved
   page is a page somebody else with the phone could open, the
   Resources FAQ carries a button to clear it again, which is wired
   up here.
--------------------------------------------------------------- */
(function () {
    'use strict';

    if (!('serviceWorker' in navigator)) return;

    var forgetButton = document.querySelector('[data-forget-site]');
    var forgetDone = document.querySelector('[data-forget-done]');
    var offlineReady = document.querySelector('[data-offline-ready]');

    navigator.serviceWorker.register('/sw.js').then(function (registration) {
        if (offlineReady) offlineReady.removeAttribute('hidden');
        if (forgetButton) forgetButton.removeAttribute('hidden');

        if (forgetButton) {
            forgetButton.addEventListener('click', function () {
                var worker = registration.active;
                if (worker) worker.postMessage('forget');
                // Clear it from this page too, in case the worker has already gone.
                if (window.caches) {
                    caches.keys().then(function (names) {
                        names.forEach(function (name) { caches.delete(name); });
                    });
                }
                forgetButton.setAttribute('hidden', '');
                if (forgetDone) forgetDone.removeAttribute('hidden');
            });
        }
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
