const CACHE_NAME = 'financial-projection-cache-v1';
const urlsToCache = [
  '/',
  '/FinancialProjection.html',
  'https://cdn.jsdelivr.net/npm/luxon/build/global/luxon.min.js',
  'https://cdn.jsdelivr.net/npm/chart.js/dist/chart.umd.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon/dist/chartjs-adapter-luxon.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-plugin-trendline/dist/chartjs-plugin-trendline.min.js',
  'https://cdn.jsdelivr.net/npm/chartjs-plugin-annotation@2.2.1/dist/chartjs-plugin-annotation.min.js',
  '/android-chrome-192x192.png',
  '/android-chrome-512x512.png'
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => response || fetch(event.request))
  );
});