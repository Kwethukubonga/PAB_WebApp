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
            if (e.key === 'Escape' && !lightbox.hasAttribute('hidden')) closeLightbox();
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
                dot.classList.toggle('bg-purple-600', i === current);
                dot.classList.toggle('bg-purple-200', i !== current);
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
})();
