const CACHE_PREFIX = 'financial-projection-cache';
const CACHE_NAME = `${CACHE_PREFIX}-v4`;

const urlsToCache = [
  '/Projections/FinancialProjection.html',
  'https://cdn.jsdelivr.net/npm/luxon/build/global/luxon.min.js',
  'https://cdn.jsdelivr.net/npm/chart.js/dist/chart.umd.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon/dist/chartjs-adapter-luxon.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-plugin-trendline/dist/chartjs-plugin-trendline.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-plugin-annotation@2.2.1/dist/chartjs-plugin-annotation.min.js',
  '/Projections/android-chrome-192x192.png',
  '/Projections/android-chrome-512x512.png'
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