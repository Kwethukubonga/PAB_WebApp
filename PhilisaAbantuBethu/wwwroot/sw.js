/*
 * Keeps the emergency numbers reachable with no data (W11).
 *
 * PAB's community buys data by the day, and runs out. Somebody who needs 10111 or the GBV
 * helpline should not be stopped by that, so the Resources page and the few files it needs are
 * kept on the device and served straight from there when the network is gone.
 *
 * Deliberately narrow. Nothing a person types is cached, no page under /Staff, no API answer and
 * no page carrying a reference number: on a shared or watched phone, a cached page is a page
 * somebody else can open. Resources is a public list of phone numbers, which is the one thing
 * worth that trade. The Resources FAQ explains how to clear it.
 */

const VERSION = 'pab-v1';

// Fetched when the worker installs, so they are there before the data runs out.
const ESSENTIAL = [
    '/Resources',
    '/css/app.css',
    '/fonts/source-sans-3-latin.woff2',
    '/fonts/lora-latin.woff2',
    '/img/brand/logo.png',
    '/offline'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(VERSION)
            // One missing file must not stop the rest being saved.
            .then((cache) => Promise.allSettled(ESSENTIAL.map((url) => cache.add(url))))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((names) => Promise.all(names.filter((name) => name !== VERSION).map((name) => caches.delete(name))))
            .then(() => self.clients.claim())
    );
});

/** Pages and answers that must never be left on the device. */
function isPrivate(url) {
    return url.pathname.startsWith('/Staff')
        || url.pathname.startsWith('/api')
        || url.pathname.startsWith('/Confirmation')
        || url.pathname.startsWith('/RequestSupport')
        || url.pathname.startsWith('/Contact')
        || url.search.length > 0;
}

self.addEventListener('fetch', (event) => {
    const { request } = event;

    // Only plain page and asset reads. A form send goes to the network or fails honestly.
    if (request.method !== 'GET') return;

    const url = new URL(request.url);
    if (url.origin !== self.location.origin || isPrivate(url)) return;

    event.respondWith(
        // Network first, so a number that PAB has corrected is the one shown.
        fetch(request)
            .then((response) => {
                if (response.ok && ESSENTIAL.includes(url.pathname)) {
                    const copy = response.clone();
                    caches.open(VERSION).then((cache) => cache.put(request, copy));
                }
                return response;
            })
            .catch(async () => {
                const cached = await caches.match(request, { ignoreSearch: true });
                if (cached) return cached;

                // A page we never had: say so, rather than showing the browser's error.
                if (request.mode === 'navigate') {
                    const offline = await caches.match('/offline');
                    if (offline) return offline;
                }
                return Response.error();
            })
    );
});

// Sent by the "clear what this site has saved" button in the Resources FAQ.
self.addEventListener('message', (event) => {
    if (event.data === 'forget') {
        caches.keys().then((names) => Promise.all(names.map((name) => caches.delete(name))));
        self.registration.unregister();
    }
});
