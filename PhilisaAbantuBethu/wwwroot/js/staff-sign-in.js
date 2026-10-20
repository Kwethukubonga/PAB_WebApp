/* Staff sign-in (Pages/Staff/SignIn.cshtml). Signs in to Firebase, hands the ID token to the
   server, which checks the admin role and starts the staff session. */
import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js';
import {
    getAuth, setPersistence, inMemoryPersistence, signInWithEmailAndPassword,
    signInWithPopup, GoogleAuthProvider, signOut
} from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-auth.js';

const root = document.querySelector('[data-staff-signin]');
if (root) {
    const form = root.querySelector('[data-signin-form]');
    const submit = root.querySelector('[data-signin-submit]');
    const google = root.querySelector('[data-signin-google]');
    const errorBox = root.querySelector('[data-signin-error]');
    const submitLabel = submit.textContent;

    const auth = getAuth(initializeApp({
        apiKey: root.dataset.apiKey,
        authDomain: root.dataset.authDomain,
        projectId: root.dataset.projectId
    }));
    // The server keeps its own session, so the Firebase sign-in is never saved in this browser.
    const ready = setPersistence(auth, inMemoryPersistence);

    const setBusy = (busy) => {
        submit.disabled = busy;
        google.disabled = busy;
        submit.textContent = busy ? root.dataset.busyLabel : submitLabel;
    };

    const showError = (message) => {
        errorBox.textContent = message;
        errorBox.hidden = false;
    };

    const messageFor = (error) => {
        switch (error && error.code) {
            case 'auth/invalid-credential':
            case 'auth/wrong-password':
            case 'auth/user-not-found':
            case 'auth/invalid-email':
                return root.dataset.errorCredentials;
            case 'auth/too-many-requests':
                return root.dataset.errorTooMany;
            case 'auth/network-request-failed':
                return root.dataset.errorNetwork;
            default:
                return root.dataset.errorGeneric;
        }
    };

    // Hands the token to the server, which answers with where to go next or why not.
    const startSession = async (credential) => {
        const body = new FormData();
        body.append('idToken', await credential.user.getIdToken());
        body.append('__RequestVerificationToken', form.querySelector('[name="__RequestVerificationToken"]').value);

        const response = await fetch(root.dataset.sessionUrl, { method: 'POST', body, credentials: 'same-origin' });
        const result = await response.json().catch(() => ({}));
        await signOut(auth);

        if (response.ok && result.redirect) {
            window.location.assign(result.redirect);
            return;
        }
        throw Object.assign(new Error(result.error || root.dataset.errorGeneric), { fromServer: true });
    };

    const run = async (signIn) => {
        errorBox.hidden = true;
        setBusy(true);
        try {
            await ready;
            await startSession(await signIn());
        } catch (error) {
            setBusy(false);
            // Closing Google's window is a choice, not an error.
            if (error && error.code === 'auth/popup-closed-by-user') return;
            showError(error && error.fromServer ? error.message : messageFor(error));
        }
    };

    form.addEventListener('submit', (e) => {
        e.preventDefault();
        run(() => signInWithEmailAndPassword(auth, form.email.value.trim(), form.password.value));
    });

    google.addEventListener('click', () => {
        run(() => signInWithPopup(auth, new GoogleAuthProvider()));
    });
}
