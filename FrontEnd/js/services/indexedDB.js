// js/services/indexedDB.js

class IndexedDBService {
  constructor() {
    this.dbName = "ECommerceDB";
    this.dbVersion = 1;
    this.db = null;
    this.init();
  }

  async init() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, this.dbVersion);

      request.onerror = () => {
        console.error("Failed to open IndexedDB:", request.error);
        reject(request.error);
      };

      request.onsuccess = () => {
        this.db = request.result;
        console.log("IndexedDB opened successfully");
        resolve(this.db);
      };

      request.onupgradeneeded = (event) => {
        const db = event.target.result;

        // Products Store
        if (!db.objectStoreNames.contains("products")) {
          const productStore = db.createObjectStore("products", {
            keyPath: "id",
          });
          productStore.createIndex("categoryId", "categoryId", {
            unique: false,
          });
          productStore.createIndex("price", "price", { unique: false });
          productStore.createIndex("name", "name", { unique: false });
          productStore.createIndex("inStock", "inStock", { unique: false });
        }

        // Cart Store
        if (!db.objectStoreNames.contains("cart")) {
          const cartStore = db.createObjectStore("cart", {
            keyPath: "id",
            autoIncrement: true,
          });
          cartStore.createIndex("productId", "productId", { unique: true });
          cartStore.createIndex("userId", "userId", { unique: false });
        }

        // Wishlist Store
        if (!db.objectStoreNames.contains("wishlist")) {
          const wishlistStore = db.createObjectStore("wishlist", {
            keyPath: "id",
            autoIncrement: true,
          });
          wishlistStore.createIndex("productId", "productId", { unique: true });
          wishlistStore.createIndex("userId", "userId", { unique: false });
        }
        console.log("IndexedDB stores created");
      };
    });
  }

  // Generic get all items
  async getAll(storeName) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readonly");
      const store = transaction.objectStore(storeName);
      const request = store.getAll();

      request.onsuccess = () => resolve(request.result || []);
      request.onerror = () => reject(request.error);
    });
  }

  // Generic get by id
  async getById(storeName, id) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readonly");
      const store = transaction.objectStore(storeName);
      const request = store.get(id);

      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  // Generic add/update item
  async put(storeName, item) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readwrite");
      const store = transaction.objectStore(storeName);
      const request = store.put(item);

      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  // Generic delete item
  async delete(storeName, id) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readwrite");
      const store = transaction.objectStore(storeName);
      const request = store.delete(id);

      request.onsuccess = () => resolve(true);
      request.onerror = () => reject(request.error);
    });
  }

  // Clear entire store
  async clearStore(storeName) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readwrite");
      const store = transaction.objectStore(storeName);
      const request = store.clear();

      request.onsuccess = () => resolve(true);
      request.onerror = () => reject(request.error);
    });
  }

  // Query with index
  async queryByIndex(storeName, indexName, value) {
    await this.ensureDB();
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction([storeName], "readonly");
      const store = transaction.objectStore(storeName);
      const index = store.index(indexName);
      const request = index.getAll(value);

      request.onsuccess = () => resolve(request.result || []);
      request.onerror = () => reject(request.error);
    });
  }

  // Ensure DB is initialized
  async ensureDB() {
    if (!this.db) {
      await this.init();
    }
    return this.db;
  }
}

export const db = new IndexedDBService();
