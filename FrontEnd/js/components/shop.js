// js/components/shop.js

import { api } from "../services/api.js";
import { auth } from "../services/auth.js";
import { storage } from "../utils/helpers.js";

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

  // Load products from API
  async loadProducts() {
    try {
      const response = await api.request("/products", "GET");
      if (response.success) {
        this.products = response.data;
        this.filteredProducts = [...this.products];
        await this.loadCategories();
        this.renderFilters();
        this.renderProducts();
      } else {
        this.showToast("Failed to load products", "error");
      }
    } catch (error) {
      console.error("Failed to load products:", error);
      this.showToast(error.message, "error");
    }
  }

  // Load categories
  async loadCategories() {
    try {
      const response = await api.request("/categories", "GET");
      if (response.success) {
        this.categories = response.data;
      }
    } catch (error) {
      console.error("Failed to load categories:", error);
    }
  }

  // Apply filters
  applyFilters() {
    this.filteredProducts = this.products.filter((product) => {
      // Category filter
      if (
        this.filters.categories.length > 0 &&
        !this.filters.categories.includes(product.categoryId)
      ) {
        return false;
      }

      // Price filter
      if (
        product.price < this.filters.minPrice ||
        product.price > this.filters.maxPrice
      ) {
        return false;
      }

      // Rating filter
      if (this.filters.ratings.length > 0) {
        const avgRating = product.averageRating || 0;
        const matchesRating = this.filters.ratings.some(
          (rating) => avgRating >= rating,
        );
        if (!matchesRating) return false;
      }

      // Availability filter
      if (this.filters.availability.length > 0) {
        const inStock = product.quantity > 0;
        if (this.filters.availability.includes("inStock") && !inStock)
          return false;
        if (this.filters.availability.includes("outOfStock") && inStock)
          return false;
      }

      return true;
    });

    this.applySorting();
    this.currentPage = 1;
    this.renderProducts();
    this.updateResultsCount();
  }

  // Apply sorting
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

    // Add event listeners to filters
    this.setupFilterListeners();
  }

  // Setup filter listeners
  setupFilterListeners() {
    // Category checkboxes
    document
      .querySelectorAll('input[data-filter="category"]')
      .forEach((checkbox) => {
        checkbox.addEventListener("change", (e) => {
          const value = parseInt(e.target.value);
          if (e.target.checked) {
            this.filters.categories.push(value);
          } else {
            this.filters.categories = this.filters.categories.filter(
              (c) => c !== value,
            );
          }
          this.applyFilters();
        });
      });

    // Rating checkboxes
    document
      .querySelectorAll('#ratingFilter input[type="checkbox"]')
      .forEach((checkbox) => {
        checkbox.addEventListener("change", (e) => {
          const value = parseInt(e.target.value);
          if (e.target.checked) {
            this.filters.ratings.push(value);
          } else {
            this.filters.ratings = this.filters.ratings.filter(
              (r) => r !== value,
            );
          }
          this.applyFilters();
        });
      });

    // Availability checkboxes
    document
      .querySelectorAll('#availabilityFilter input[type="checkbox"]')
      .forEach((checkbox) => {
        checkbox.addEventListener("change", (e) => {
          const value = e.target.value;
          if (e.target.checked) {
            this.filters.availability.push(value);
          } else {
            this.filters.availability = this.filters.availability.filter(
              (a) => a !== value,
            );
          }
          this.applyFilters();
        });
      });

    // Price range
    const minPriceSlider = document.getElementById("minPrice");
    const maxPriceSlider = document.getElementById("maxPrice");
    const minPriceInput = document.getElementById("minPriceInput");
    const maxPriceInput = document.getElementById("maxPriceInput");

    const updatePrice = () => {
      this.filters.minPrice = parseInt(minPriceSlider.value);
      this.filters.maxPrice = parseInt(maxPriceSlider.value);
      minPriceInput.value = this.filters.minPrice;
      maxPriceInput.value = this.filters.maxPrice;
      this.applyFilters();
    };

    minPriceSlider.addEventListener("input", updatePrice);
    maxPriceSlider.addEventListener("input", updatePrice);
    minPriceInput.addEventListener("change", () => {
      minPriceSlider.value = minPriceInput.value;
      updatePrice();
    });
    maxPriceInput.addEventListener("change", () => {
      maxPriceSlider.value = maxPriceInput.value;
      updatePrice();
    });

    // Clear filters button
    const clearBtn = document.getElementById("clearFilters");
    if (clearBtn) {
      clearBtn.addEventListener("click", () => {
        document
          .querySelectorAll('input[type="checkbox"]')
          .forEach((cb) => (cb.checked = false));
        this.filters = {
          categories: [],
          minPrice: 0,
          maxPrice: 1000,
          ratings: [],
          availability: [],
        };
        minPriceSlider.value = 0;
        maxPriceSlider.value = 1000;
        minPriceInput.value = 0;
        maxPriceInput.value = 1000;
        this.applyFilters();
      });
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
        resetBtn.addEventListener("click", () => {
          document.getElementById("clearFilters")?.click();
        });
      }
      return;
    }

    grid.innerHTML = paginatedProducts
      .map(
        (product) => `
            <div class="product-card" data-product-id="${product.id}">
                <div class="product-image">
                    <img src="${product.images?.[0]?.url || "https://via.placeholder.com/300"}" alt="${product.name}">
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
                    <button class="add-to-cart" data-id="${product.id}" ${product.quantity === 0 ? "disabled" : ""}>
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

    // Previous button
    paginationHtml += `<button class="pagination-prev" ${this.currentPage === 1 ? "disabled" : ""}>&laquo;</button>`;

    // Page numbers
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

    // Next button
    paginationHtml += `<button class="pagination-next" ${this.currentPage === totalPages ? "disabled" : ""}>&raquo;</button>`;

    paginationContainer.innerHTML = paginationHtml;

    // Add event listeners
    paginationContainer.querySelectorAll("[data-page]").forEach((btn) => {
      btn.addEventListener("click", () => {
        this.currentPage = parseInt(btn.dataset.page);
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    });

    const prevBtn = paginationContainer.querySelector(".pagination-prev");
    const nextBtn = paginationContainer.querySelector(".pagination-next");

    if (prevBtn && !prevBtn.disabled) {
      prevBtn.addEventListener("click", () => {
        this.currentPage--;
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }

    if (nextBtn && !nextBtn.disabled) {
      nextBtn.addEventListener("click", () => {
        this.currentPage++;
        this.renderProducts();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }
  }

  // Setup product events (add to cart, wishlist)
  setupProductEvents() {
    // Add to cart buttons
    document.querySelectorAll(".add-to-cart").forEach((btn) => {
      btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.addToCart(productId);
      });
    });

    // Wishlist buttons
    document.querySelectorAll(".wishlist-btn").forEach((btn) => {
      btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.toggleWishlist(productId, btn);
      });
    });

    // Product click to view details
    document.querySelectorAll(".product-card").forEach((card) => {
      card.addEventListener("click", (e) => {
        if (
          !e.target.closest(".add-to-cart") &&
          !e.target.closest(".wishlist-btn")
        ) {
          const productId = card.dataset.productId;
          window.location.href = `product-details.html?id=${productId}`;
        }
      });
    });
  }

  // Add to cart
  async addToCart(productId) {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to cart", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    try {
      // Add to cart API call
      const response = await api.request(
        "/cart/add",
        "POST",
        { productId, quantity: 1 },
        true,
      );
      if (response.success) {
        this.showToast("Added to cart successfully!", "success");
        this.updateCartCount();
      }
    } catch (error) {
      this.showToast(error.message, "error");
    }
  }

  // Toggle wishlist
  async toggleWishlist(productId, button) {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to wishlist", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const isActive = button.classList.contains("active");
    const icon = button.querySelector("i");

    try {
      if (isActive) {
        await api.request("/wishlist/remove", "DELETE", { productId }, true);
        button.classList.remove("active");
        icon.className = "far fa-heart";
        this.showToast("Removed from wishlist", "success");
      } else {
        await api.request("/wishlist/add", "POST", { productId }, true);
        button.classList.add("active");
        icon.className = "fas fa-heart";
        this.showToast("Added to wishlist", "success");
      }
      this.updateWishlistCount();
    } catch (error) {
      this.showToast(error.message, "error");
    }
  }

  // Update cart count
  async updateCartCount() {
    try {
      const response = await api.request("/cart/count", "GET", null, true);
      if (response.success) {
        const cartCount = document.getElementById("cartCount");
        if (cartCount) cartCount.textContent = response.data;
      }
    } catch (error) {
      console.error("Failed to update cart count:", error);
    }
  }

  // Update wishlist count
  async updateWishlistCount() {
    try {
      const response = await api.request("/wishlist/count", "GET", null, true);
      if (response.success) {
        const wishlistCount = document.getElementById("wishlistCount");
        if (wishlistCount) wishlistCount.textContent = response.data;
      }
    } catch (error) {
      console.error("Failed to update wishlist count:", error);
    }
  }

  // Setup search
  setupSearch() {
    const searchToggle = document.getElementById("searchToggle");
    const searchBar = document.getElementById("searchBar");
    const closeSearch = document.getElementById("closeSearch");
    const searchInput = document.getElementById("searchInput");

    if (searchToggle && searchBar) {
      searchToggle.addEventListener("click", () => {
        searchBar.style.display =
          searchBar.style.display === "none" ? "block" : "none";
        if (searchBar.style.display === "block") {
          searchInput.focus();
        }
      });

      closeSearch.addEventListener("click", () => {
        searchBar.style.display = "none";
        searchInput.value = "";
      });

      let searchTimeout;
      searchInput.addEventListener("input", () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
          const searchTerm = searchInput.value.toLowerCase();
          if (searchTerm) {
            this.filteredProducts = this.products.filter(
              (product) =>
                product.name.toLowerCase().includes(searchTerm) ||
                product.description?.toLowerCase().includes(searchTerm),
            );
          } else {
            this.filteredProducts = [...this.products];
          }
          this.applySorting();
          this.currentPage = 1;
          this.renderProducts();
          this.updateResultsCount();
        }, 300);
      });
    }
  }

  // Setup sort and view
  setupSortAndView() {
    const sortSelect = document.getElementById("sortBy");
    if (sortSelect) {
      sortSelect.addEventListener("change", () => {
        this.applySorting();
        this.currentPage = 1;
        this.renderProducts();
      });
    }

    const viewBtns = document.querySelectorAll(".view-btn");
    viewBtns.forEach((btn) => {
      btn.addEventListener("click", () => {
        viewBtns.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        this.currentView = btn.dataset.view;
        if (this.currentView === "list") {
          document.getElementById("productsGrid").classList.add("list-view");
        } else {
          document.getElementById("productsGrid").classList.remove("list-view");
        }
        this.renderProducts();
      });
    });
  }

  // Setup filter toggles
  setupFilterToggles() {
    document.querySelectorAll(".filter-toggle").forEach((toggle) => {
      toggle.addEventListener("click", () => {
        toggle.classList.toggle("open");
        const filterId = toggle.dataset.filter;
        const content = document.getElementById(`${filterId}Filter`);
        if (content) {
          content.classList.toggle("show");
        }
      });
    });
  }

  // Setup user dropdown
  setupUserDropdown() {
    const userBtn = document.getElementById("userBtn");
    const dropdown = document.getElementById("userDropdown");

    if (userBtn && dropdown) {
      userBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        dropdown.classList.toggle("show");
      });

      document.addEventListener("click", (e) => {
        if (!userBtn.contains(e.target) && !dropdown.contains(e.target)) {
          dropdown.classList.remove("show");
        }
      });
    }

    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", async () => {
        await auth.logout();
        window.location.href = "index.html";
      });
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
