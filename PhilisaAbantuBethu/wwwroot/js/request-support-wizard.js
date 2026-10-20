/* Philisa Abafazi Bethu — Request Support form: 3-step wizard navigation. */
(function () {
    'use strict';

    var stepForm = document.querySelector('[data-step-form]');
    if (!stepForm) return;

    var steps = Array.prototype.slice.call(stepForm.querySelectorAll('[data-step]'));
    var markers = Array.prototype.slice.call(document.querySelectorAll('[data-step-marker]'));
    var bars = Array.prototype.slice.call(document.querySelectorAll('[data-step-bar]'));
    var labels = Array.prototype.slice.call(document.querySelectorAll('[data-step-label]'));
    var backBtn = stepForm.querySelector('[data-step-back]');
    var nextBtn = stepForm.querySelector('[data-step-next]');
    var submitBtn = stepForm.querySelector('[data-step-submit]');
    var status = stepForm.querySelector('[data-step-status]');
    var total = steps.length;
    var submitLabel = submitBtn.innerHTML;

    // After a failed submit the server says which step holds the first problem.
    var current = Math.min(total, Math.max(1, Number(stepForm.dataset.startStep) || 1));

    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function render() {
        steps.forEach(function (panel) {
            var n = Number(panel.dataset.step);
            if (n === current) { panel.removeAttribute('hidden'); } else { panel.setAttribute('hidden', ''); }
            // Only the visible step should block native validation
            panel.querySelectorAll('[data-required]').forEach(function (field) {
                if (n === current) { field.setAttribute('required', ''); } else { field.removeAttribute('required'); }
            });
        });

        markers.forEach(function (m, i) {
            var n = i + 1;
            m.className = 'flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold transition-all ' +
                (current > n ? 'bg-green-600 text-white'
                    : current === n ? 'bg-purple-700 text-white ring-4 ring-purple-100'
                        : 'bg-stone-200 text-stone-600');
            m.innerHTML = current > n
                ? '<svg width="14" height="14" fill="none" stroke="white" stroke-width="2.5" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 13l4 4L19 7" stroke-linecap="round" stroke-linejoin="round"/></svg>' +
                  '<span class="sr-only">' + n + '</span>'
                : String(n);
            if (current === n) { m.setAttribute('aria-current', 'step'); } else { m.removeAttribute('aria-current'); }
        });

        labels.forEach(function (l, i) {
            l.className = 'text-xs font-medium hidden sm:block flex-shrink-0 ' + (current === i + 1 ? 'text-purple-700' : 'text-stone-500');
        });

        bars.forEach(function (b, i) {
            b.className = 'flex-1 h-0.5 ' + (current > i + 1 ? 'bg-green-500' : 'bg-stone-200');
        });

        backBtn.disabled = current === 1;
        backBtn.className = 'flex items-center gap-2 text-sm font-medium transition-colors ' +
            (current === 1 ? 'text-stone-300 cursor-not-allowed' : 'text-stone-600 hover:text-stone-900');

        if (current < total) {
            nextBtn.removeAttribute('hidden');
            submitBtn.setAttribute('hidden', '');
        } else {
            nextBtn.setAttribute('hidden', '');
            submitBtn.removeAttribute('hidden');
            fillSummary();
        }
    }

    function fillSummary() {
        document.querySelectorAll('[data-summary-for]').forEach(function (cell) {
            var name = cell.dataset.summaryFor;
            var field = stepForm.querySelector('[name="' + name + '"]');
            var value = '';
            if (name === 'FullName') {
                var f = stepForm.querySelector('[name="FirstName"]');
                var s = stepForm.querySelector('[name="Surname"]');
                value = ((f && f.value) + ' ' + (s && s.value)).trim();
            } else if (name === 'Urgent') {
                var checked = stepForm.querySelector('[name="Urgent"]:checked');
                var yesLabel = stepForm.dataset.yesLabel || 'Yes';
                var noLabel = stepForm.dataset.noLabel || 'No';
                value = checked && checked.value === 'yes' ? yesLabel : noLabel;
            } else if (field) {
                value = field.value;
            }
            cell.textContent = value ? value : '-';
        });
    }

    function validateStep() {
        var panel = steps[current - 1];
        var fields = panel.querySelectorAll('input, select, textarea');
        for (var i = 0; i < fields.length; i++) {
            if (!fields[i].checkValidity()) { fields[i].reportValidity(); return false; }
        }
        return true;
    }

    // The button that was clicked is hidden by the step change, so focus moves to the new
    // step's heading and the step is announced, instead of focus being lost.
    function showStep() {
        render();
        stepForm.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'start' });
        var heading = steps[current - 1].querySelector('[data-step-heading]');
        if (heading) heading.focus({ preventScroll: true });
        if (status) {
            status.textContent = (stepForm.dataset.stepOfLabel || 'Step {0} of {1}')
                .replace('{0}', current).replace('{1}', total) +
                (heading ? ': ' + heading.textContent.trim() : '');
        }
    }

    nextBtn.addEventListener('click', function () {
        if (!validateStep()) return;
        current = Math.min(total, current + 1);
        showStep();
    });

    backBtn.addEventListener('click', function () {
        current = Math.max(1, current - 1);
        showStep();
    });

    // One request per press: the button locks and shows that it is sending.
    stepForm.addEventListener('submit', function (e) {
        if (submitBtn.disabled) { e.preventDefault(); return; }
        submitBtn.disabled = true;
        submitBtn.setAttribute('aria-busy', 'true');
        submitBtn.innerHTML =
            '<svg class="animate-spin" width="16" height="16" fill="none" viewBox="0 0 24 24" aria-hidden="true">' +
            '<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" opacity="0.25"/>' +
            '<path d="M22 12a10 10 0 0 0-10-10" stroke="currentColor" stroke-width="3" stroke-linecap="round"/></svg>' +
            (submitBtn.dataset.busyLabel || 'Sending…');
    });

    // Coming back with the browser's Back button can restore the locked button.
    window.addEventListener('pageshow', function () {
        submitBtn.disabled = false;
        submitBtn.removeAttribute('aria-busy');
        submitBtn.innerHTML = submitLabel;
    });

    render();
})();
