// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).


self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    self.skipWaiting();
});


self.addEventListener('fetch', () => { });


self.addEventListener('push', event => {
    const payload = event.data.json();
    console.log("payload", payload);
    event.waitUntil(
        self.registration.showNotification('Exam time', {
            body: payload.message,
            icon: 'icon-512.png',
            vibrate: [100, 50, 100],
            data: { url: "https://google.com"}
        })
    );
});

/*console.log("adding push listener")*/



/*self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data.url));
});*/
