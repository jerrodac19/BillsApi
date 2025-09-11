const CACHE_NAME = 'bills-cache-v3';
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
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((response) => response || fetch(event.request))
  );
});


