// js/components/shop.js

import { api } from "../services/api.js";
import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

class ShopPage {
  constructor() {
    this.products = [];
    this.filteredProducts = [];
    this.categories = [];
    this.currentPage = 1;
    this.itemsPerPage = 12;
    this.currentView = "grid";
    this.filters = {
      categories: [],
      minPrice: 0,
      maxPrice: 1000,
      ratings: [],
      availability: [],
      searchTerm: "",
    };
    this.init();
  }

  // Show toast message
  showToast(message, type = "success") {
    const toast = document.getElementById("toast");
    const toastContent = toast.querySelector(".toast-content");
    const icon = toastContent.querySelector("i");
    const messageSpan = toastContent.querySelector("#toastMessage");

    icon.className = "fas";
    icon.classList.add(
      type === "success" ? "fa-check-circle" : "fa-exclamation-circle",
    );
    messageSpan.textContent = message;

    toast.classList.remove("success", "error");
    toast.classList.add(type);
    toast.classList.add("show");

    setTimeout(() => {
      toast.classList.remove("show");
    }, 3000);
  }

  // Load products from server and store in IndexedDB
  async loadProducts() {
    try {
      // First, try to load from IndexedDB cache
      const cachedProducts = await db.getAll("products");

      if (cachedProducts && cachedProducts.length > 0) {
        console.log(`Loaded ${cachedProducts.length} products from cache`);
        this.products = cachedProducts;
        this.filteredProducts = [...this.products];
        await this.extractCategories();
        this.renderFilters();
        this.renderProducts();
        this.updateResultsCount();

        // Refresh from server in background
        this.refreshProductsFromServer();
      } else {
        // Load from server if cache is empty
        await this.refreshProductsFromServer();
      }
    } catch (error) {
      console.error("Failed to load products:", error);
      this.showToast("Failed to load products", "error");
    }
  }

  // Refresh products from server and update cache
  async refreshProductsFromServer() {
    try {
      const response = await api.request("/products", "GET");
      if (response.success && response.data) {
        // Store products in IndexedDB
        await db.clearStore("products");
        for (const product of response.data) {
          await db.put("products", {
            ...product,
            inStock: product.quantity > 0,
            cachedAt: new Date().toISOString(),
          });
        }

        this.products = response.data;
        this.filteredProducts = [...this.products];
        await this.extractCategories();
        this.renderFilters();
        this.renderProducts();
        this.updateResultsCount();
        console.log(`Loaded ${response.data.length} products from server`);
      }
    } catch (error) {
      console.error("Failed to refresh products from server:", error);
    }
  }

  // Extract unique categories from products
  async extractCategories() {
    const categoryMap = new Map();
    for (const product of this.products) {
      if (product.categoryId && !categoryMap.has(product.categoryId)) {
        categoryMap.set(product.categoryId, {
          id: product.categoryId,
          name: product.categoryName || `Category ${product.categoryId}`,
        });
      }
    }
    this.categories = Array.from(categoryMap.values());
  }

  // Apply all filters locally
  applyFilters() {
    let filtered = [...this.products];

    // Category filter
    if (this.filters.categories.length > 0) {
      filtered = filtered.filter((product) =>
        this.filters.categories.includes(product.categoryId),
      );
    }

    // Price filter
    filtered = filtered.filter(
      (product) =>
        product.price >= this.filters.minPrice &&
        product.price <= this.filters.maxPrice,
    );

    // Rating filter
    if (this.filters.ratings.length > 0) {
      filtered = filtered.filter((product) => {
        const avgRating = product.averageRating || 0;
        return this.filters.ratings.some((rating) => avgRating >= rating);
      });
    }

    // Availability filter
    if (this.filters.availability.length > 0) {
      filtered = filtered.filter((product) => {
        const inStock = product.quantity > 0;
        if (this.filters.availability.includes("inStock") && !inStock)
          return false;
        if (this.filters.availability.includes("outOfStock") && inStock)
          return false;
        return true;
      });
    }

    // Search filter
    if (this.filters.searchTerm) {
      const term = this.filters.searchTerm.toLowerCase();
      filtered = filtered.filter(
        (product) =>
          product.name.toLowerCase().includes(term) ||
          (product.description &&
            product.description.toLowerCase().includes(term)),
      );
    }

    this.filteredProducts = filtered;
    this.applySorting();
    this.currentPage = 1;
    this.renderProducts();
    this.updateResultsCount();
  }

  // Apply sorting locally
  applySorting() {
    const sortBy = document.getElementById("sortBy")?.value || "featured";

    switch (sortBy) {
      case "price_asc":
        this.filteredProducts.sort((a, b) => a.price - b.price);
        break;
      case "price_desc":
        this.filteredProducts.sort((a, b) => b.price - a.price);
        break;
      case "name_asc":
        this.filteredProducts.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case "name_desc":
        this.filteredProducts.sort((a, b) => b.name.localeCompare(a.name));
        break;
      case "rating":
        this.filteredProducts.sort(
          (a, b) => (b.averageRating || 0) - (a.averageRating || 0),
        );
        break;
      default:
        // featured - keep original order
        break;
    }
  }

  // Update results count
  updateResultsCount() {
    const countElement = document.getElementById("productsCount");
    if (countElement) {
      countElement.textContent = this.filteredProducts.length;
    }
  }

  // Render category filters
  renderFilters() {
    const categoryList = document.getElementById("categoryList");
    if (categoryList && this.categories.length > 0) {
      categoryList.innerHTML = this.categories
        .map(
          (category) => `
            <label class="checkbox-label">
                <input type="checkbox" value="${category.id}" data-filter="category">
                <span>${category.name}</span>
            </label>
          `,
        )
        .join("");
    }

    this.setupFilterListeners();
  }

  // Setup filter listeners
  setupFilterListeners() {
    // Category checkboxes
    document
      .querySelectorAll('input[data-filter="category"]')
      .forEach((checkbox) => {
        checkbox.removeEventListener("change", this.handleCategoryChange);
        this.handleCategoryChange = (e) => {
          const value = parseInt(e.target.value);
          if (e.target.checked) {
            this.filters.categories.push(value);
          } else {
            this.filters.categories = this.filters.categories.filter(
              (c) => c !== value,
            );
          }
          this.applyFilters();
        };
        checkbox.addEventListener("change", this.handleCategoryChange);
      });

    // Rating checkboxes
    document
      .querySelectorAll('#ratingFilter input[type="checkbox"]')
      .forEach((checkbox) => {
        checkbox.removeEventListener("change", this.handleRatingChange);
        this.handleRatingChange = (e) => {
          const value = parseInt(e.target.value);
          if (e.target.checked) {
            this.filters.ratings.push(value);
          } else {
            this.filters.ratings = this.filters.ratings.filter(
              (r) => r !== value,
            );
          }
          this.applyFilters();
        };
        checkbox.addEventListener("change", this.handleRatingChange);
      });

    // Availability checkboxes
    document
      .querySelectorAll('#availabilityFilter input[type="checkbox"]')
      .forEach((checkbox) => {
        checkbox.removeEventListener("change", this.handleAvailabilityChange);
        this.handleAvailabilityChange = (e) => {
          const value = e.target.value;
          if (e.target.checked) {
            this.filters.availability.push(value);
          } else {
            this.filters.availability = this.filters.availability.filter(
              (a) => a !== value,
            );
          }
          this.applyFilters();
        };
        checkbox.addEventListener("change", this.handleAvailabilityChange);
      });

    // Price range
    const minPriceSlider = document.getElementById("minPrice");
    const maxPriceSlider = document.getElementById("maxPrice");
    const minPriceInput = document.getElementById("minPriceInput");
    const maxPriceInput = document.getElementById("maxPriceInput");

    if (minPriceSlider && maxPriceSlider) {
      const updatePrice = () => {
        this.filters.minPrice = parseInt(minPriceSlider.value);
        this.filters.maxPrice = parseInt(maxPriceSlider.value);
        if (minPriceInput) minPriceInput.value = this.filters.minPrice;
        if (maxPriceInput) maxPriceInput.value = this.filters.maxPrice;
        this.applyFilters();
      };

      minPriceSlider.removeEventListener("input", updatePrice);
      maxPriceSlider.removeEventListener("input", updatePrice);
      minPriceSlider.addEventListener("input", updatePrice);
      maxPriceSlider.addEventListener("input", updatePrice);

      if (minPriceInput) {
        minPriceInput.removeEventListener("change", updatePrice);
        minPriceInput.addEventListener("change", updatePrice);
      }
      if (maxPriceInput) {
        maxPriceInput.removeEventListener("change", updatePrice);
        maxPriceInput.addEventListener("change", updatePrice);
      }
    }

    // Clear filters button
    const clearBtn = document.getElementById("clearFilters");
    if (clearBtn) {
      clearBtn.removeEventListener("click", this.handleClearFilters);
      this.handleClearFilters = () => {
        document
          .querySelectorAll('input[type="checkbox"]')
          .forEach((cb) => (cb.checked = false));
        this.filters = {
          categories: [],
          minPrice: 0,
          maxPrice: 1000,
          ratings: [],
          availability: [],
          searchTerm: this.filters.searchTerm,
        };
        if (minPriceSlider) minPriceSlider.value = 0;
        if (maxPriceSlider) maxPriceSlider.value = 1000;
        if (minPriceInput) minPriceInput.value = 0;
        if (maxPriceInput) maxPriceInput.value = 1000;
        this.applyFilters();
      };
      clearBtn.addEventListener("click", this.handleClearFilters);
    }
  }

  // Render products grid
  renderProducts() {
    const grid = document.getElementById("productsGrid");
    if (!grid) return;

    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    const paginatedProducts = this.filteredProducts.slice(start, end);

    if (paginatedProducts.length === 0) {
      grid.innerHTML = `
        <div class="empty-state" style="grid-column: 1/-1;">
          <i class="fas fa-box-open"></i>
          <h3>No products found</h3>
          <p>Try adjusting your filters or search criteria</p>
          <button class="register-btn" id="resetFiltersBtn">Reset Filters</button>
        </div>
      `;
      const resetBtn = document.getElementById("resetFiltersBtn");
      if (resetBtn) {
        resetBtn.removeEventListener("click", this.handleResetFilters);
        this.handleResetFilters = () => {
          document.getElementById("clearFilters")?.click();
        };
        resetBtn.addEventListener("click", this.handleResetFilters);
      }
      return;
    }

    grid.innerHTML = paginatedProducts
      .map(
        (product) => `
          <div class="product-card" data-product-id="${product.id}">
            <div class="product-image">
              <img loading="lazy" src="${product.images?.[0]?.url || product.mainImageUrl || "https://via.placeholder.com/300"}" alt="${product.name}">
              <button class="wishlist-btn" data-id="${product.id}">
                <i class="far fa-heart"></i>
              </button>
              ${product.discount ? '<span class="sale-badge">Sale</span>' : ""}
            </div>
            <div class="product-info">
              <h3>${product.name}</h3>
              <div class="product-price">
                <span class="current-price">$${product.price.toFixed(2)}</span>
                ${product.oldPrice ? `<span class="old-price">$${product.oldPrice.toFixed(2)}</span>` : ""}
              </div>
              <div class="product-rating">
                ${this.renderStars(product.averageRating || 0)}
                <span>(${product.reviewCount || 0})</span>
              </div>
              <div class="product-stock">
                ${
                  product.quantity > 0
                    ? '<span class="in-stock"><i class="fas fa-check-circle"></i> In Stock</span>'
                    : '<span class="out-of-stock"><i class="fas fa-times-circle"></i> Out of Stock</span>'
                }
              </div>
              <button class="add-to-cart" data-id="${product.id}" data-product='${JSON.stringify(product)}' ${product.quantity === 0 ? "disabled" : ""}>
                <i class="fas fa-shopping-cart"></i> Add to Cart
              </button>
            </div>
          </div>
        `,
      )
      .join("");

    if (this.currentView === "list") {
      grid.classList.add("list-view");
    } else {
      grid.classList.remove("list-view");
    }

    this.renderPagination();
    this.setupProductEvents();
    this.updateWishlistButtonStates();
  }

  // Render star rating
  renderStars(rating) {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    let stars = "";
    for (let i = 0; i < fullStars; i++) stars += '<i class="fas fa-star"></i>';
    if (hasHalfStar) stars += '<i class="fas fa-star-half-alt"></i>';
    for (let i = 0; i < emptyStars; i++) stars += '<i class="far fa-star"></i>';

    return stars;
  }

  // Render pagination
  renderPagination() {
    const paginationContainer = document.getElementById("pagination");
    if (!paginationContainer) return;

    const totalPages = Math.ceil(
      this.filteredProducts.length / this.itemsPerPage,
    );

    if (totalPages <= 1) {
      paginationContainer.innerHTML = "";
      return;
    }

    let paginationHtml = "";
    paginationHtml += `<button class="pagination-prev" ${this.currentPage === 1 ? "disabled" : ""}>&laquo;</button>`;

    for (let i = 1; i <= totalPages; i++) {
      if (
        i === 1 ||
        i === totalPages ||
        (i >= this.currentPage - 1 && i <= this.currentPage + 1)
      ) {
        paginationHtml += `<button class="${i === this.currentPage ? "active" : ""}" data-page="${i}">${i}</button>`;
      } else if (i === this.currentPage - 2 || i === this.currentPage + 2) {
        paginationHtml += `<span class="pagination-dots">...</span>`;
      }
    }

    paginationHtml += `<button class="pagination-next" ${this.currentPage === totalPages ? "disabled" : ""}>&raquo;</button>`;
    paginationContainer.innerHTML = paginationHtml;

    paginationContainer.querySelectorAll("[data-page]").forEach((btn) => {
      btn.removeEventListener("click", this.handlePageClick);
      this.handlePageClick = () => {
        this.currentPage = parseInt(btn.dataset.page);
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      };
      btn.addEventListener("click", this.handlePageClick);
    });

    const prevBtn = paginationContainer.querySelector(".pagination-prev");
    const nextBtn = paginationContainer.querySelector(".pagination-next");

    if (prevBtn && !prevBtn.disabled) {
      prevBtn.removeEventListener("click", this.handlePrevClick);
      this.handlePrevClick = () => {
        this.currentPage--;
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      };
      prevBtn.addEventListener("click", this.handlePrevClick);
    }

    if (nextBtn && !nextBtn.disabled) {
      nextBtn.removeEventListener("click", this.handleNextClick);
      this.handleNextClick = () => {
        this.currentPage++;
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      };
      nextBtn.addEventListener("click", this.handleNextClick);
    }
  }

  // Setup product events
  setupProductEvents() {
    // Add to cart buttons (local storage based)
    document.querySelectorAll(".add-to-cart").forEach((btn) => {
      btn.removeEventListener("click", this.handleAddToCart);
      this.handleAddToCart = async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        const product = this.products.find((p) => p.id === productId);
        await this.addToCart(product);
      };
      btn.addEventListener("click", this.handleAddToCart);
    });

    // Wishlist buttons (local storage based)
    document.querySelectorAll(".wishlist-btn").forEach((btn) => {
      btn.removeEventListener("click", this.handleWishlist);
      this.handleWishlist = async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.toggleWishlist(productId, btn);
      };
      btn.addEventListener("click", this.handleWishlist);
    });

    // Product click to view details
    document.querySelectorAll(".product-card").forEach((card) => {
      card.removeEventListener("click", this.handleProductClick);
      this.handleProductClick = (e) => {
        if (
          !e.target.closest(".add-to-cart") &&
          !e.target.closest(".wishlist-btn")
        ) {
          const productId = card.dataset.productId;
          window.location.href = `product-details.html?id=${productId}`;
        }
      };
      card.addEventListener("click", this.handleProductClick);
    });
  }

  // Add to cart (IndexedDB based - no server call)
  async addToCart(product) {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to cart", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    try {
      const userId = auth.user?.id || "guest";

      // Check if product already in cart
      const existingCart = await db.getAll("cart");
      const existingItem = existingCart.find(
        (item) => item.productId === product.id && item.userId === userId,
      );

      if (existingItem) {
        // Update quantity
        existingItem.quantity += 1;
        await db.put("cart", existingItem);
      } else {
        // Add new item
        await db.put("cart", {
          productId: product.id,
          productName: product.name,
          productPrice: product.price,
          productImage: product.images?.[0]?.url || product.mainImageUrl,
          quantity: 1,
          userId: userId,
          addedAt: new Date().toISOString(),
        });
      }

      this.showToast("Added to cart successfully!", "success");
      await this.updateCartCount();
    } catch (error) {
      console.error("Failed to add to cart:", error);
      this.showToast(error.message, "error");
    }
  }

  // Toggle wishlist (IndexedDB based - no server call)
  async toggleWishlist(productId, button) {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to wishlist", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const userId = auth.user?.id || "guest";
    const isActive = button.classList.contains("active");
    const icon = button.querySelector("i");

    try {
      if (isActive) {
        // Remove from wishlist
        const wishlist = await db.getAll("wishlist");
        const item = wishlist.find(
          (i) => i.productId === productId && i.userId === userId,
        );
        if (item) {
          await db.delete("wishlist", item.id);
        }
        button.classList.remove("active");
        icon.className = "far fa-heart";
        this.showToast("Removed from wishlist", "success");
      } else {
        // Add to wishlist
        const product = this.products.find((p) => p.id === productId);
        await db.put("wishlist", {
          productId: productId,
          productName: product?.name,
          productPrice: product?.price,
          productImage:
            product?.images[0]?.url || product?.mainImageUrl,
          userId: userId,
          addedAt: new Date().toISOString(),
        });
        button.classList.add("active");
        icon.className = "fas fa-heart";
        this.showToast("Added to wishlist", "success");
      }
      await this.updateWishlistCount();
    } catch (error) {
      console.error("Failed to toggle wishlist:", error);
      this.showToast(error.message, "error");
    }
  }

  // Update wishlist button states
  async updateWishlistButtonStates() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id || "guest";
    const wishlist = await db.getAll("wishlist");
    const wishlistProductIds = new Set(
      wishlist
        .filter((item) => item.userId === userId)
        .map((item) => item.productId),
    );

    document.querySelectorAll(".wishlist-btn").forEach((btn) => {
      const productId = parseInt(btn.dataset.id);
      const icon = btn.querySelector("i");
      if (wishlistProductIds.has(productId)) {
        btn.classList.add("active");
        icon.className = "fas fa-heart";
      } else {
        btn.classList.remove("active");
        icon.className = "far fa-heart";
      }
    });
  }

  // Update cart count from IndexedDB
  async updateCartCount() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id || "guest";
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
  }

  // Update wishlist count from IndexedDB
  async updateWishlistCount() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id || "guest";
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
  }

  // Setup search (local filtering)
  setupSearch() {
    const searchToggle = document.getElementById("searchToggle");
    const searchBar = document.getElementById("searchBar");
    const closeSearch = document.getElementById("closeSearch");
    const searchInput = document.getElementById("searchInput");

    if (searchToggle && searchBar) {
      searchToggle.removeEventListener("click", this.handleSearchToggle);
      this.handleSearchToggle = () => {
        searchBar.style.display =
          searchBar.style.display === "none" ? "block" : "none";
        if (searchBar.style.display === "block") {
          searchInput.focus();
        }
      };
      searchToggle.addEventListener("click", this.handleSearchToggle);

      closeSearch.removeEventListener("click", this.handleCloseSearch);
      this.handleCloseSearch = () => {
        searchBar.style.display = "none";
        searchInput.value = "";
        this.filters.searchTerm = "";
        this.applyFilters();
      };
      closeSearch.addEventListener("click", this.handleCloseSearch);

      let searchTimeout;
      searchInput.removeEventListener("input", this.handleSearchInput);
      this.handleSearchInput = () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
          this.filters.searchTerm = searchInput.value;
          this.applyFilters();
        }, 300);
      };
      searchInput.addEventListener("input", this.handleSearchInput);
    }
  }

  // Setup sort and view
  setupSortAndView() {
    const sortSelect = document.getElementById("sortBy");
    if (sortSelect) {
      sortSelect.removeEventListener("change", this.handleSortChange);
      this.handleSortChange = () => {
        this.applySorting();
        this.currentPage = 1;
        this.renderProducts();
      };
      sortSelect.addEventListener("change", this.handleSortChange);
    }

    const viewBtns = document.querySelectorAll(".view-btn");
    viewBtns.forEach((btn) => {
      btn.removeEventListener("click", this.handleViewChange);
      this.handleViewChange = () => {
        viewBtns.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        this.currentView = btn.dataset.view;
        const grid = document.getElementById("productsGrid");
        if (this.currentView === "list") {
          grid.classList.add("list-view");
        } else {
          grid.classList.remove("list-view");
        }
        this.renderProducts();
      };
      btn.addEventListener("click", this.handleViewChange);
    });
  }

  // Setup filter toggles
  setupFilterToggles() {
    document.querySelectorAll(".filter-toggle").forEach((toggle) => {
      toggle.removeEventListener("click", this.handleFilterToggle);
      this.handleFilterToggle = () => {
        toggle.classList.toggle("open");
        const filterId = toggle.dataset.filter;
        const content = document.getElementById(`${filterId}Filter`);
        if (content) {
          content.classList.toggle("show");
        }
      };
      toggle.addEventListener("click", this.handleFilterToggle);
    });
  }

  // Setup user dropdown
  setupUserDropdown() {
    const userBtn = document.getElementById("userBtn");
    const dropdown = document.getElementById("userDropdown");

    if (userBtn && dropdown) {
      userBtn.removeEventListener("click", this.handleUserClick);
      this.handleUserClick = (e) => {
        e.stopPropagation();
        dropdown.classList.toggle("show");
      };
      userBtn.addEventListener("click", this.handleUserClick);

      document.removeEventListener("click", this.handleDocumentClick);
      this.handleDocumentClick = (e) => {
        if (!userBtn.contains(e.target) && !dropdown.contains(e.target)) {
          dropdown.classList.remove("show");
        }
      };
      document.addEventListener("click", this.handleDocumentClick);
    }

    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
      logoutBtn.removeEventListener("click", this.handleLogout);
      this.handleLogout = async () => {
        await auth.logout();
        window.location.href = "index.html";
      };
      logoutBtn.addEventListener("click", this.handleLogout);
    }
  }

  // Initialize shop page
  async init() {
    await this.loadProducts();
    this.setupSearch();
    this.setupSortAndView();
    this.setupFilterToggles();
    this.setupUserDropdown();

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      await this.updateWishlistCount();
    }
  }
}

// Initialize shop page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new ShopPage();
});
