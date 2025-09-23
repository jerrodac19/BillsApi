const CACHE_PREFIX = 'bills-cache';
const CACHE_NAME = `${CACHE_PREFIX}-v14`;

const urlsToCache = [
  '/BillsApp/index.html',
  '/BillsApp/views/bill-list.html',
  '/BillsApp/views/edit-bill.html',
  '/BillsApp/style.css',
  '/BillsApp/app.js',
  '/BillsApp/billsicon-192x192.png',
  '/BillsApp/billsicon-512x512.png'
];

self.addEventListener('install', (event) => {
  console.log('Service worker is installing...');
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => {
        return Promise.all(
          urlsToCache.map((url) => {
            return cache.add(url).catch((error) => {
              console.error(`Failed to cache ${url}:`, error);
              return Promise.resolve(); // This ensures the all-or-nothing promise doesn't fail
            });
          })
        );
      })
      .then(() => {
        console.log('All files successfully cached.');
        // Force the new service worker to activate immediately after installation
        self.skipWaiting();
      })
      .catch((error) => {
        console.error('Service worker installation failed:', error);
      })
  );
});

self.addEventListener('activate', (event) => {
  console.log('Service worker is activating...');
  event.waitUntil(
    // Delete old caches to free up space
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.filter((cacheName) => cacheName.startsWith(CACHE_PREFIX) && cacheName !== CACHE_NAME)
          .map((cacheName) => caches.delete(cacheName))
      );
    }).then(() => {
      // Takes control of the page immediately, bypassing the waiting state
      return self.clients.claim();
    })
  );
});

self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => response || fetch(event.request))
  );
});