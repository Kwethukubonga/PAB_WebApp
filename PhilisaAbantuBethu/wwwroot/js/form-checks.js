/* Philisa Abafazi Bethu — form checks while typing.

   A field is checked as soon as the person leaves it, and again on every key once it shows a
   problem, so the message goes away as it is fixed. Emojis are caught on the key that typed
   them. Before, nothing was said until Next or Send, which on the last step of Request Support
   meant going back to find what was wrong.

   The rules are the server's (Forms/SupportRequestForm.cs, Forms/ContactForm.cs,
   Services/PhoneNumber.cs, Forms/TextInput.cs), and the messages come from the page, in the
   visitor's language. The server still checks everything, for anyone without this script.

   A form opts in with data-live-checks and the shared messages (data-msg-emoji and so on). A
   field opts in with data-check="text|name|phone|email" and, when it must be filled in,
   data-msg-required.
*/
(function () {
    'use strict';

    var forms = Array.prototype.slice.call(document.querySelectorAll('[data-live-checks]'));
    if (!forms.length) return;

    // The same ranges as TextInput.HasEmoji.
    var emoji = /[\u{1F000}-\u{1FAFF}\u{2600}-\u{27BF}\u{2B50}-\u{2B55}\u{231A}\u{231B}\u{23E9}-\u{23FA}\u{FE0F}\u{E0020}-\u{E007F}]/u;
    var letter = /\p{L}/u;
    var email = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // The same rules as PhoneNumber.IsValid: 0821234567 or +27821234567, a real network or
    // landline code, and not one digit over and over.
    var mobileCodes = ['72', '76', '79', '82', '73', '78', '83', '74', '84', '81',
        '60', '61', '62', '63', '64', '65', '66', '67', '68', '71'];
    function isPhone(value) {
        var compact = value.replace(/[\s\-()]/g, '');
        if (!/^(0|\+27)[1-9]\d{8}$/.test(compact)) return false;
        var local = compact.indexOf('+27') === 0 ? '0' + compact.slice(3) : compact;
        if (mobileCodes.indexOf(local.substr(1, 2)) < 0 && !/^0[1-5]/.test(local)) return false;
        return new Set(local.slice(3)).size > 1;
    }

    // Only digits, and a + at the start, fit in a phone box, and no more than a number needs:
    // 10 starting with 0, 12 starting with +27.
    function trimPhone(field) {
        var value = field.value;
        var kept = value.replace(/[^\d+]/g, '').replace(/(?!^)\+/g, '');
        kept = kept.slice(0, kept.charAt(0) === '+' ? 12 : 10);
        if (kept !== value) field.value = kept;
    }

    // Same look as the server's error state on these pages.
    var okClasses = ['border', 'border-ink-200', 'focus:ring-brand-500'];
    var errorClasses = ['border-2', 'border-red-600', 'focus:ring-red-500'];

    function problemWith(form, field) {
        if (field.disabled) return null;
        var d = form.dataset;
        var value = field.type === 'checkbox' ? (field.checked ? 'yes' : '') : field.value.trim();

        if (!value) {
            if (field.dataset.msgRequired) return field.dataset.msgRequired;
            // Email is needed when it is how PAB should reply.
            var method = form.querySelector('[name="ContactMethod"]:checked');
            if (field.dataset.msgNeededForEmail && method && method.value === 'email') return field.dataset.msgNeededForEmail;
            // On Contact, either email or phone will do.
            var other = field.dataset.either && document.getElementById(field.dataset.either);
            if (other && !other.value.trim()) return field.dataset.msgEither;
            return null;
        }

        if (emoji.test(value)) return d.msgEmoji;
        switch (field.dataset.check) {
            case 'name': return letter.test(value) ? null : d.msgLetter;
            case 'phone': return isPhone(value) ? null : d.msgPhone;
            case 'email': return value.length <= 254 && email.test(value) ? null : d.msgEmail;
        }
        return null;
    }

    function show(field, message) {
        var id = field.id + '-error';
        var note = document.getElementById(id);
        var described = (field.getAttribute('aria-describedby') || '').split(' ').filter(function (x) { return x && x !== id; });

        if (message) {
            if (!note) {
                note = document.createElement('p');
                note.id = id;
                note.className = 'mt-1.5 text-sm font-medium text-red-700';
                // A tick box sits in a row with its label, so the message goes under the row.
                var after = field.type === 'checkbox' ? field.parentElement : field;
                after.insertAdjacentElement('afterend', note);
            }
            note.textContent = message;
            note.removeAttribute('hidden');
            described.push(id);
            field.setAttribute('aria-invalid', 'true');
        } else {
            if (note) note.setAttribute('hidden', '');
            field.removeAttribute('aria-invalid');
        }
        if (described.length) field.setAttribute('aria-describedby', described.join(' '));
        else field.removeAttribute('aria-describedby');

        // Only the public forms' boxes have these borders; the staff notes box keeps its own.
        if (field.type !== 'checkbox' && (field.classList.contains('border-ink-200') || field.classList.contains('border-red-600'))) {
            okClasses.forEach(function (c) { field.classList.toggle(c, !message); });
            errorClasses.forEach(function (c) { field.classList.toggle(c, !!message); });
        }
    }

    // Fields that have been left, or checked by Next or Send. From then on they are checked on
    // every key, so a message goes away as soon as it is fixed.
    var touched = new WeakSet();

    function check(form, field) {
        touched.add(field);
        var message = problemWith(form, field);
        show(field, message);
        return !message;
    }

    function fieldsOf(form) {
        return Array.prototype.slice.call(form.querySelectorAll('[data-check], [data-msg-required]'));
    }

    /**
     * Checks every visible field in the container and returns the first one with a problem.
     * Fields on other steps are left alone, so a message the server gave there stays.
     */
    function firstProblem(form, container) {
        var first = null;
        fieldsOf(form).forEach(function (field) {
            if (!container.contains(field) || field.closest('[hidden]')) return;
            if (!check(form, field) && !first) first = field;
        });
        return first;
    }

    forms.forEach(function (form) {
        // The messages above replace the browser's own bubbles. Without this script the
        // browser's checks still apply, since novalidate is only set here.
        form.noValidate = true;

        fieldsOf(form).forEach(function (field) {
            // A message from the server counts as checked already.
            if (document.getElementById(field.id + '-error')) touched.add(field);

            field.addEventListener('input', function () {
                if (field.dataset.check === 'phone') trimPhone(field);
                // An emoji is said straight away; anything else waits until the field is left,
                // so nobody is told off for a number they are still typing.
                if (touched.has(field) || emoji.test(field.value)) check(form, field);
            });
            field.addEventListener(field.type === 'checkbox' || field.tagName === 'SELECT' ? 'change' : 'blur', function () {
                // Tabbing past an empty field isn't a mistake yet; leaving it after typing is.
                if (!field.value && field.type !== 'checkbox' && !touched.has(field)) return;
                check(form, field);
            });
        });

        // Choosing email as the way to reply makes the email box needed, and the other way round.
        form.querySelectorAll('[name="ContactMethod"]').forEach(function (radio) {
            radio.addEventListener('change', function () {
                var box = form.querySelector('[data-msg-needed-for-email]');
                if (box && (touched.has(box) || radio.value !== 'email')) check(form, box);
            });
        });
        // On Contact, filling in a phone number settles the "email or phone" message.
        fieldsOf(form).forEach(function (field) {
            var other = field.dataset.either && document.getElementById(field.dataset.either);
            if (other) other.addEventListener('input', function () { if (touched.has(field)) check(form, field); });
        });

        // Runs before the page's own submit handlers, so a form with a problem neither sends nor
        // locks its button. On Request Support only the step on screen is checked; the wizard
        // checks each step before moving on.
        form.addEventListener('submit', function (e) {
            var bad = firstProblem(form, form);
            if (!bad) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            bad.focus();
        }, true);
    });

    window.PabFormChecks = {
        /** The first field with a problem in the container (marked on the page), or null. */
        firstProblem: function (container) {
            var form = container.closest('[data-live-checks]');
            return form ? firstProblem(form, container) : null;
        }
    };
})();
