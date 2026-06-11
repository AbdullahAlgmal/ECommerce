// js/components/product-details.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { db } from "../services/indexedDB.js";

class ProductDetails {
  constructor() {
    this.productId = null;
    this.product = null;
    this.quantity = 1;
    this.reviews = [];
    this.relatedProducts = [];
    this.userRating = 0;
    this.init();
  }

  // Get product ID from URL
  getProductId() {
    const urlParams = new URLSearchParams(window.location.search);
    this.productId = parseInt(urlParams.get("id"));

    if (!this.productId || isNaN(this.productId)) {
      window.location.href = "shop.html";
    }
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

  // Load product from IndexedDB or API
  async loadProduct() {
    try {
      // Try to get from IndexedDB first
      const products = await db.getAll("products");
      this.product = products.find((p) => p.id === this.productId);

      // If not in cache, fetch from API
      if (!this.product) {
        const response = await api.request(
          `/products/${this.productId}`,
          "GET",
        );
        if (response.success) {
          this.product = response.data;
          await db.put("products", this.product);
        } else {
          throw new Error("Product not found");
        }
      }

      this.renderProduct();
      this.updateBreadcrumb();
      await this.loadReviews();
      await this.loadRelatedProducts();
      await this.updateWishlistStatus();
    } catch (error) {
      console.error("Failed to load product:", error);
      this.showToast("Product not found", "error");
      setTimeout(() => {
        window.location.href = "shop.html";
      }, 1500);
    }
  }

  // Render product details
  renderProduct() {
    // Product images
    const mainImage = document.getElementById("mainProductImage");
    const thumbnailList = document.getElementById("thumbnailList");

    const images = this.product.images || [];
    if (images.length > 0) {
      mainImage.src = images[0].url;

      thumbnailList.innerHTML = images
        .map(
          (img, index) => `
                <div class="thumbnail ${index === 0 ? "active" : ""}" data-image="${img.url}">
                    <img src="${img.url}" alt="Thumbnail ${index + 1}">
                </div>
            `,
        )
        .join("");

      // Thumbnail click handler
      document.querySelectorAll(".thumbnail").forEach((thumb) => {
        thumb.addEventListener("click", () => {
          document
            .querySelectorAll(".thumbnail")
            .forEach((t) => t.classList.remove("active"));
          thumb.classList.add("active");
          mainImage.src = thumb.dataset.image;
        });
      });
    }

    // Product info
    document.getElementById("productCategory").textContent =
      this.product.categoryName || "Uncategorized";
    document.getElementById("productName").textContent = this.product.name;
    document.getElementById("productPrice").textContent =
      `$${this.product.price.toFixed(2)}`;
    document.getElementById("productDescription").textContent =
      this.product.description || "No description available";
    document.getElementById("fullDescription").textContent =
      this.product.description || "No description available";
    document.getElementById("productSku").textContent =
      `SKU-${this.product.id.toString().padStart(4, "0")}`;
    document.getElementById("productCategoryName").textContent =
      this.product.categoryName || "Uncategorized";

    // Stock status
    const stockElement = document.getElementById("productStock");
    if (this.product.quantity > 0) {
      stockElement.innerHTML = `<span class="in-stock"><i class="fas fa-check-circle"></i> In Stock (${this.product.quantity} available)</span>`;
    } else {
      stockElement.innerHTML = `<span class="out-of-stock"><i class="fas fa-times-circle"></i> Out of Stock</span>`;
      document.getElementById("addToCartBtn").disabled = true;
    }

    // Rating stars
    const avgRating = this.product.averageRating || 0;
    const starsHtml = this.renderStars(avgRating);
    document.getElementById("productRating").innerHTML = starsHtml;

    // Max quantity
    const quantityInput = document.getElementById("productQuantity");
    if (this.product.quantity > 0) {
      quantityInput.max = this.product.quantity;
    }
  }

  // Render stars
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

  // Update breadcrumb
  updateBreadcrumb() {
    document.getElementById("breadcrumbCategory").textContent =
      this.product.categoryName || "Category";
    document.getElementById("breadcrumbProduct").textContent =
      this.product.name;
  }

  // Load reviews
  async loadReviews() {
    try {
      const response = await api.request(
        `/reviews/product/${this.productId}`,
        "GET",
      );
      if (response.success) {
        this.reviews = response.data;
        this.renderReviews();
        this.renderRatingDistribution();
      }
    } catch (error) {
      console.error("Failed to load reviews:", error);
      document.getElementById("reviewsList").innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-comments"></i>
                    <h3>No reviews yet</h3>
                    <p>Be the first to review this product!</p>
                </div>
            `;
    }
  }

  // Render reviews list
  renderReviews() {
    const reviewsList = document.getElementById("reviewsList");
    const reviewTabCount = document.getElementById("reviewTabCount");
    const avgRatingElement = document.getElementById("avgRating");
    const avgRatingStars = document.getElementById("avgRatingStars");
    const totalReviews = document.getElementById("totalReviews");

    reviewTabCount.textContent = this.reviews.length;

    if (this.reviews.length === 0) {
      reviewsList.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-comments"></i>
                    <h3>No reviews yet</h3>
                    <p>Be the first to review this product!</p>
                </div>
            `;
      avgRatingElement.textContent = "0.0";
      avgRatingStars.innerHTML = this.renderStars(0);
      totalReviews.textContent = "0 reviews";
      return;
    }

    // Calculate average rating
    const avgRating =
      this.reviews.reduce((sum, r) => sum + r.rating, 0) / this.reviews.length;
    avgRatingElement.textContent = avgRating.toFixed(1);
    avgRatingStars.innerHTML = this.renderStars(avgRating);
    totalReviews.textContent = `${this.reviews.length} reviews`;

    // Render reviews
    reviewsList.innerHTML = this.reviews
      .map(
        (review) => `
            <div class="review-card">
                <div class="review-header">
                    <div class="reviewer">
                        <span class="reviewer-name">${review.userName || "Anonymous"}</span>
                        <div class="reviewer-stars">${this.renderStars(review.rating)}</div>
                    </div>
                    <div class="review-date">${new Date(review.reviewDate).toLocaleDateString()}</div>
                </div>
                <div class="review-text">${review.reviewText}</div>
            </div>
        `,
      )
      .join("");
  }

  // Render rating distribution
  renderRatingDistribution() {
    const ratingBars = document.getElementById("ratingBars");
    const distribution = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };

    this.reviews.forEach((review) => {
      distribution[Math.floor(review.rating)]++;
    });

    const total = this.reviews.length;

    ratingBars.innerHTML = "";
    for (let i = 5; i >= 1; i--) {
      const count = distribution[i];
      const percentage = total > 0 ? (count / total) * 100 : 0;

      ratingBars.innerHTML += `
                <div class="rating-bar-item">
                    <div class="rating-label">${i} <i class="fas fa-star"></i></div>
                    <div class="bar-container">
                        <div class="bar-fill" style="width: ${percentage}%"></div>
                    </div>
                    <div class="rating-count">${count}</div>
                </div>
            `;
    }
  }

  // Load related products
  async loadRelatedProducts() {
    try {
      const products = await db.getAll("products");
      this.relatedProducts = products
        .filter(
          (p) =>
            p.categoryId === this.product.categoryId &&
            p.id !== this.product.id,
        )
        .slice(0, 4);

      this.renderRelatedProducts();
    } catch (error) {
      console.error("Failed to load related products:", error);
    }
  }

  // Render related products
  renderRelatedProducts() {
    const grid = document.getElementById("relatedProductsGrid");

    if (this.relatedProducts.length === 0) {
      grid.innerHTML =
        '<div class="empty-state"><i class="fas fa-box-open"></i><h3>No related products</h3></div>';
      return;
    }

    grid.innerHTML = this.relatedProducts
      .map(
        (product) => `
            <div class="related-product-card">
                <div class="related-product-image">
                    <img src="${product.images?.[0]?.url || "https://via.placeholder.com/300"}" alt="${product.name}">
                </div>
                <div class="related-product-info">
                    <h4>${product.name}</h4>
                    <div class="related-product-price">$${product.price.toFixed(2)}</div>
                    <a href="product-details.html?id=${product.id}" class="view-product-link">View Product</a>
                </div>
            </div>
        `,
      )
      .join("");
  }

  // Update wishlist status
  async updateWishlistStatus() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id;
    const wishlist = await db.getAll("wishlist");
    const inWishlist = wishlist.some(
      (item) => item.productId === this.productId && item.userId === userId,
    );

    const wishlistBtn = document.getElementById("wishlistBtn");
    if (inWishlist) {
      wishlistBtn.classList.add("active");
      wishlistBtn.innerHTML =
        '<i class="fas fa-heart"></i> Remove from Wishlist';
    } else {
      wishlistBtn.classList.remove("active");
      wishlistBtn.innerHTML = '<i class="far fa-heart"></i> Add to Wishlist';
    }
  }

  // Setup quantity selector
  setupQuantitySelector() {
    const decreaseBtn = document.getElementById("decreaseQty");
    const increaseBtn = document.getElementById("increaseQty");
    const quantityInput = document.getElementById("productQuantity");

    decreaseBtn.addEventListener("click", () => {
      let value = parseInt(quantityInput.value);
      if (value > 1) {
        quantityInput.value = value - 1;
        this.quantity = value - 1;
      }
    });

    increaseBtn.addEventListener("click", () => {
      let value = parseInt(quantityInput.value);
      if (value < this.product.quantity) {
        quantityInput.value = value + 1;
        this.quantity = value + 1;
      }
    });

    quantityInput.addEventListener("change", () => {
      let value = parseInt(quantityInput.value);
      if (isNaN(value) || value < 1) {
        quantityInput.value = 1;
        this.quantity = 1;
      } else if (value > this.product.quantity) {
        quantityInput.value = this.product.quantity;
        this.quantity = this.product.quantity;
      } else {
        this.quantity = value;
      }
    });
  }

  // Add to cart
  async addToCart() {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to cart", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const userId = auth.user.id;
    const quantity = parseInt(document.getElementById("productQuantity").value);

    try {
      const cart = await db.getAll("cart");
      const existingItem = cart.find(
        (item) => item.productId === this.productId && item.userId === userId,
      );

      if (existingItem) {
        existingItem.quantity += quantity;
        await db.put("cart", existingItem);
      } else {
        await db.put("cart", {
          productId: this.product.id,
          productName: this.product.name,
          productPrice: this.product.price,
          productImage: this.product.images?.[0]?.url,
          quantity: quantity,
          userId: userId,
          addedAt: new Date().toISOString(),
        });
      }

      this.showToast("Added to cart successfully!", "success");
      await this.updateCartCount();
    } catch (error) {
      console.error("Failed to add to cart:", error);
      this.showToast("Failed to add to cart", "error");
    }
  }

  // Toggle wishlist
  async toggleWishlist() {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to wishlist", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const userId = auth.user.id;
    const wishlistBtn = document.getElementById("wishlistBtn");
    const isActive = wishlistBtn.classList.contains("active");

    try {
      if (isActive) {
        // Remove from wishlist
        const wishlist = await db.getAll("wishlist");
        const item = wishlist.find(
          (i) => i.productId === this.productId && i.userId === userId,
        );
        if (item) {
          await db.delete("wishlist", item.id);
        }
        wishlistBtn.classList.remove("active");
        wishlistBtn.innerHTML = '<i class="far fa-heart"></i> Add to Wishlist';
        this.showToast("Removed from wishlist", "success");
      } else {
        // Add to wishlist
        await db.put("wishlist", {
          productId: this.product.id,
          productName: this.product.name,
          productPrice: this.product.price,
          productImage: this.product.images?.[0]?.url,
          userId: userId,
          addedAt: new Date().toISOString(),
        });
        wishlistBtn.classList.add("active");
        wishlistBtn.innerHTML =
          '<i class="fas fa-heart"></i> Remove from Wishlist';
        this.showToast("Added to wishlist", "success");
      }
      await this.updateWishlistCount();
    } catch (error) {
      console.error("Failed to toggle wishlist:", error);
      this.showToast("Operation failed", "error");
    }
  }

  // Submit review
  async submitReview(event) {
    event.preventDefault();

    if (!auth.isAuthenticated()) {
      this.showToast("Please login to submit a review", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const rating = this.userRating;
    const reviewText = document.getElementById("reviewText").value.trim();

    if (rating === 0) {
      this.showToast("Please select a rating", "error");
      return;
    }

    if (!reviewText) {
      this.showToast("Please enter your review", "error");
      return;
    }

    try {
      const response = await api.request(
        "/reviews",
        "POST",
        {
          productId: this.productId,
          userId: auth.user.id,
          rating: rating,
          reviewText: reviewText,
        },
        true,
      );

      if (response.success) {
        this.showToast("Review submitted successfully!", "success");
        document.getElementById("reviewForm").reset();
        this.userRating = 0;
        document.querySelectorAll(".star-rating i").forEach((star) => {
          star.classList.remove("active");
        });
        await this.loadReviews();
      } else {
        this.showToast(response.message || "Failed to submit review", "error");
      }
    } catch (error) {
      console.error("Failed to submit review:", error);
      this.showToast(error.message, "error");
    }
  }

  // Setup star rating
  setupStarRating() {
    const stars = document.querySelectorAll(".star-rating i");
    stars.forEach((star) => {
      star.addEventListener("click", () => {
        const rating = parseInt(star.dataset.rating);
        this.userRating = rating;

        stars.forEach((s, index) => {
          if (index < rating) {
            s.classList.add("active");
          } else {
            s.classList.remove("active");
          }
        });
      });
    });
  }

  // Setup tabs
  setupTabs() {
    const tabBtns = document.querySelectorAll(".tab-btn");
    const tabPanes = document.querySelectorAll(".tab-pane");

    tabBtns.forEach((btn) => {
      btn.addEventListener("click", () => {
        const tabId = btn.dataset.tab;

        tabBtns.forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");

        tabPanes.forEach((pane) => pane.classList.remove("active"));
        document.getElementById(`${tabId}Tab`).classList.add("active");
      });
    });
  }

  // Update cart count
  async updateCartCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
  }

  // Update wishlist count
  async updateWishlistCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
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
          const searchTerm = searchInput.value;
          if (searchTerm) {
            window.location.href = `shop.html?search=${encodeURIComponent(searchTerm)}`;
          }
        }, 300);
      });
    }
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

  // Initialize product details
  async init() {
    this.getProductId();
    await this.loadProduct();

    this.setupQuantitySelector();
    this.setupStarRating();
    this.setupTabs();
    this.setupSearch();
    this.setupUserDropdown();

    document
      .getElementById("addToCartBtn")
      .addEventListener("click", () => this.addToCart());
    document
      .getElementById("wishlistBtn")
      .addEventListener("click", () => this.toggleWishlist());
    document
      .getElementById("reviewForm")
      .addEventListener("submit", (e) => this.submitReview(e));

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      await this.updateWishlistCount();
      auth.startTokenRefreshTimer();
    }
  }
}

// Initialize product details when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new ProductDetails();
});
