const CACHE_NAME = 'financial-projection-cache-v3';
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
      })
      .catch((error) => {
        console.error('Service worker installation failed:', error);
      })
  );
});

self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => response || fetch(event.request))
  );
});