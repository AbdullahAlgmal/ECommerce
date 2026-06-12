// js/components/wishlist.js

import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

class WishlistPage {
  constructor() {
    this.wishlistItems = [];
    this.recommendedProducts = [];
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

  // Load wishlist items from IndexedDB
  async loadWishlist() {
    const container = document.getElementById("wishlistItemsContainer");

    try {
      const userId = auth.user?.id;
      if (!userId) {
        container.innerHTML = `
                    <div class="empty-wishlist" style="grid-column: 1/-1;">
                        <i class="fas fa-heart-broken"></i>
                        <h3>Please login to view your wishlist</h3>
                        <p>Login or create an account to save your favorite items</p>
                        <a href="index.html" class="shop-now-btn">Login</a>
                    </div>
                `;
        return;
      }

      const wishlist = await db.getAll("wishlist");
      this.wishlistItems = wishlist.filter((item) => item.userId === userId);

      // Get product details for each wishlist item
      const products = await db.getAll("products");
      this.wishlistItems = this.wishlistItems.map((item) => {
        const product = products.find((p) => p.id === item.productId);
        return {
            ...item,
            productImage: product?.images?.[0]?.url,
            wishlistId: item.id
          };
      });

      this.updateWishlistCount();

      if (this.wishlistItems.length === 0) {
        container.innerHTML = `
                    <div class="empty-wishlist" style="grid-column: 1/-1;">
                        <i class="far fa-heart"></i>
                        <h3>Your wishlist is empty</h3>
                        <p>Start adding products you love to your wishlist!</p>
                        <a href="shop.html" class="shop-now-btn">Start Shopping</a>
                    </div>
                `;
        document.getElementById("recommendedSection").style.display = "none";
        return;
      }

      this.renderWishlistItems();
      await this.loadRecommendedProducts();
    } catch (error) {
      console.error("Failed to load wishlist:", error);
      container.innerHTML = `
                <div class="empty-wishlist" style="grid-column: 1/-1;">
                    <i class="fas fa-exclamation-circle"></i>
                    <h3>Failed to load wishlist</h3>
                    <p>Please try again later</p>
                </div>
            `;
    }
  }

  // Render wishlist items
  renderWishlistItems() {
    const container = document.getElementById("wishlistItemsContainer");
    const itemCountSpan = document.getElementById("wishlistItemCount");

    itemCountSpan.textContent = this.wishlistItems.length;

    container.innerHTML = this.wishlistItems
      .map(
        (item) => `
            <div class="wishlist-card" data-wishlist-id="${item.wishlistId}" data-product-id="${item.productId}">
                <div class="wishlist-card-image">
                    <img src="${item.productImage || "https://via.placeholder.com/300"}" alt="${item.productName}">
                    <button class="remove-wishlist-btn" data-id="${item.wishlistId}">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                    ${item.discount ? '<span class="sale-badge">Sale</span>' : ""}
                    <span class="stock-badge ${item.quantity > 0 ? "in-stock" : "out-of-stock"}">
                        ${item.quantity > 0 ? "In Stock" : "Out of Stock"}
                    </span>
                    <div class="product-actions-overlay">
                        <button class="add-to-cart-overlay" data-id="${item.productId}" ${item.quantity === 0 ? "disabled" : ""}>
                            <i class="fas fa-shopping-cart"></i> Add to Cart
                        </button>
                        <button class="view-product-overlay" data-id="${item.productId}">
                            <i class="fas fa-eye"></i>
                        </button>
                    </div>
                </div>
                <div class="wishlist-card-info">
                    <h3>${item.productName}</h3>
                    <div class="product-price">
                        <span class="current-price">$${item.productPrice.toFixed(2)}</span>
                        ${item.oldPrice ? `<span class="old-price">$${item.oldPrice.toFixed(2)}</span>` : ""}
                    </div>
                    <div class="product-rating">
                        ${this.renderStars(item.averageRating || 0)}
                        <span>(${item.reviewCount || 0})</span>
                    </div>
                    <div class="added-date">
                        <small><i class="fas fa-clock"></i> Added on: ${new Date(item.addedAt).toLocaleDateString()}</small>
                    </div>
                </div>
            </div>
        `,
      )
      .join("");

    this.setupWishlistEvents();
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

  // Setup wishlist event listeners
  setupWishlistEvents() {
    // Remove from wishlist buttons
    document.querySelectorAll(".remove-wishlist-btn").forEach((btn) => {
      btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const wishlistId = parseInt(btn.dataset.id);
        await this.removeFromWishlist(wishlistId);
      });
    });

    // Add to cart buttons
    document.querySelectorAll(".add-to-cart-overlay").forEach((btn) => {
      btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.addToCart(productId);
      });
    });

    // View product buttons
    document.querySelectorAll(".view-product-overlay").forEach((btn) => {
      btn.addEventListener("click", (e) => {
        e.stopPropagation();
        const productId = btn.dataset.id;
        window.location.href = `product-details.html?id=${productId}`;
      });
    });

    // Wishlist card click to view product
    document.querySelectorAll(".wishlist-card").forEach((card) => {
      card.addEventListener("click", (e) => {
        if (
          !e.target.closest(".remove-wishlist-btn") &&
          !e.target.closest(".add-to-cart-overlay") &&
          !e.target.closest(".view-product-overlay")
        ) {
          const productId = card.dataset.productId;
          window.location.href = `product-details.html?id=${productId}`;
        }
      });
    });
  }

  // Remove from wishlist
  async removeFromWishlist(wishlistId) {
    try {
      await db.delete("wishlist", wishlistId);
      this.wishlistItems = this.wishlistItems.filter(
        (item) => item.wishlistId !== wishlistId,
      );

      if (this.wishlistItems.length === 0) {
        const container = document.getElementById("wishlistItemsContainer");
        container.innerHTML = `
                    <div class="empty-wishlist" style="grid-column: 1/-1;">
                        <i class="far fa-heart"></i>
                        <h3>Your wishlist is empty</h3>
                        <p>Start adding products you love to your wishlist!</p>
                        <a href="shop.html" class="shop-now-btn">Start Shopping</a>
                    </div>
                `;
        document.getElementById("recommendedSection").style.display = "none";
      } else {
        this.renderWishlistItems();
      }

      this.updateWishlistCount();
      this.showToast("Removed from wishlist", "success");
    } catch (error) {
      console.error("Failed to remove from wishlist:", error);
      this.showToast("Failed to remove item", "error");
    }
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

    const userId = auth.user.id;
    const product = this.wishlistItems.find(
      (item) => item.productId === productId,
    );

    if (!product) return;

    try {
      const cart = await db.getAll("cart");
      const existingItem = cart.find(
        (item) => item.productId === productId && item.userId === userId,
      );

      if (existingItem) {
        existingItem.quantity += 1;
        await db.put("cart", existingItem);
      } else {
        await db.put("cart", {
          productId: product.productId,
          productName: product.productName,
          productPrice: product.productPrice,
          productImage: product.productImage,
          quantity: 1,
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

  // Load recommended products
  async loadRecommendedProducts() {
    try {
      const products = await db.getAll("products");
      const wishlistProductIds = new Set(
        this.wishlistItems.map((item) => item.productId),
      );

      // Get products from same categories as wishlist items
      const categories = new Set(
        this.wishlistItems.map((item) => item.categoryId),
      );
      let recommended = products.filter(
        (product) =>
          !wishlistProductIds.has(product.id) &&
          categories.has(product.categoryId),
      );

      // If not enough recommendations, add other products
      if (recommended.length < 4) {
        const otherProducts = products.filter(
          (p) => !wishlistProductIds.has(p.id) && !categories.has(p.categoryId),
        );
        recommended = [...recommended, ...otherProducts];
      }

      this.recommendedProducts = recommended.slice(0, 4);

      if (this.recommendedProducts.length > 0) {
        document.getElementById("recommendedSection").style.display = "block";
        this.renderRecommendedProducts();
      }
    } catch (error) {
      console.error("Failed to load recommended products:", error);
    }
  }

  // Render recommended products
  renderRecommendedProducts() {
    const grid = document.getElementById("recommendedGrid");

    grid.innerHTML = this.recommendedProducts
      .map(
        (product) => `
            <div class="recommended-card">
                <div class="recommended-image">
                    <img src="${product.images?.[0]?.url || "https://via.placeholder.com/300"}" alt="${product.name}">
                </div>
                <div class="recommended-info">
                    <h4>${product.name}</h4>
                    <div class="recommended-price">$${product.price.toFixed(2)}</div>
                    <button class="add-to-wishlist-btn" data-id="${product.id}">
                        <i class="far fa-heart"></i> Add to Wishlist
                    </button>
                </div>
            </div>
        `,
      )
      .join("");

    // Setup add to wishlist for recommended products
    document.querySelectorAll(".add-to-wishlist-btn").forEach((btn) => {
      btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.addToWishlistFromRecommended(productId, btn);
      });
    });
  }

  // Add to wishlist from recommended
  async addToWishlistFromRecommended(productId, button) {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to add items to wishlist", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    const userId = auth.user.id;
    const products = await db.getAll("products");
    const product = products.find((p) => p.id === productId);

    if (!product) return;

    try {
      const wishlist = await db.getAll("wishlist");
      const exists = wishlist.some(
        (item) => item.productId === productId && item.userId === userId,
      );

      if (exists) {
        this.showToast("Item already in wishlist", "error");
        return;
      }

      await db.put("wishlist", {
        productId: product.id,
        productName: product.name,
        productPrice: product.price,
        productImage: product.images?.[0]?.url,
        userId: userId,
        addedAt: new Date().toISOString(),
      });

      button.innerHTML = '<i class="fas fa-heart"></i> Added!';
      button.disabled = true;
      setTimeout(() => {
        button.innerHTML = '<i class="far fa-heart"></i> Add to Wishlist';
        button.disabled = false;
      }, 2000);

      this.updateWishlistCount();
      this.showToast("Added to wishlist!", "success");
    } catch (error) {
      console.error("Failed to add to wishlist:", error);
      this.showToast("Failed to add to wishlist", "error");
    }
  }

  // Clear entire wishlist
  async clearWishlist() {
    if (this.wishlistItems.length === 0) return;

    const confirmed = confirm(
      "Are you sure you want to clear your entire wishlist? This action cannot be undone.",
    );
    if (!confirmed) return;

    try {
      for (const item of this.wishlistItems) {
        await db.delete("wishlist", item.wishlistId);
      }

      this.wishlistItems = [];
      const container = document.getElementById("wishlistItemsContainer");
      container.innerHTML = `
                <div class="empty-wishlist" style="grid-column: 1/-1;">
                    <i class="far fa-heart"></i>
                    <h3>Your wishlist is empty</h3>
                    <p>Start adding products you love to your wishlist!</p>
                    <a href="shop.html" class="shop-now-btn">Start Shopping</a>
                </div>
            `;
      document.getElementById("recommendedSection").style.display = "none";
      this.updateWishlistCount();
      this.showToast("Wishlist cleared", "success");
    } catch (error) {
      console.error("Failed to clear wishlist:", error);
      this.showToast("Failed to clear wishlist", "error");
    }
  }

  // Share wishlist modal
  setupShareModal() {
    const shareBtn = document.getElementById("shareWishlistBtn");
    const modal = document.getElementById("shareModal");
    const closeModal = document.getElementById("closeModalBtn");
    const copyLinkBtn = document.getElementById("copyLinkBtn");
    const shareLink = document.getElementById("shareLink");

    shareBtn.addEventListener("click", () => {
      modal.classList.add("active");
      // Generate share link (in production, this would be a real link)
      const shareId = btoa(`user_${auth.user?.id}_wishlist`).slice(0, 20);
      shareLink.value = `${window.location.origin}/wishlist/share/${shareId}`;
    });

    closeModal.addEventListener("click", () => {
      modal.classList.remove("active");
    });

    modal.addEventListener("click", (e) => {
      if (e.target === modal) {
        modal.classList.remove("active");
      }
    });

    copyLinkBtn.addEventListener("click", () => {
      shareLink.select();
      document.execCommand("copy");
      this.showToast("Link copied to clipboard!", "success");
    });

    // Share to social media
    document.querySelectorAll(".share-option").forEach((option) => {
      option.addEventListener("click", () => {
        const platform = option.dataset.platform;
        const url = shareLink.value;
        const text = "Check out my wishlist!";

        let shareUrl = "";
        switch (platform) {
          case "facebook":
            shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`;
            break;
          case "twitter":
            shareUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`;
            break;
          case "pinterest":
            shareUrl = `https://pinterest.com/pin/create/button/?url=${encodeURIComponent(url)}&description=${encodeURIComponent(text)}`;
            break;
          case "whatsapp":
            shareUrl = `https://wa.me/?text=${encodeURIComponent(text + " " + url)}`;
            break;
          case "email":
            shareUrl = `mailto:?subject=${encodeURIComponent("My Wishlist")}&body=${encodeURIComponent(text + "\n\n" + url)}`;
            break;
        }

        if (shareUrl) {
          window.open(shareUrl, "_blank", "width=600,height=400");
        }
      });
    });
  }

  // Update wishlist count badge
  async updateWishlistCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
  }

  // Update cart count badge
  async updateCartCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
  }

  // Setup back to top button
  setupBackToTop() {
    const backToTop = document.getElementById("backToTop");

    window.addEventListener("scroll", () => {
      if (window.scrollY > 300) {
        backToTop.classList.add("visible");
      } else {
        backToTop.classList.remove("visible");
      }
    });

    backToTop.addEventListener("click", () => {
      window.scrollTo({ top: 0, behavior: "smooth" });
    });
  }

  // Setup header search
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

  // Initialize wishlist page
  async init() {
    if (auth.isAuthenticated()) {
      await auth.getCurrentUser();
      await this.loadWishlist();
    } else {
      const container = document.getElementById("wishlistItemsContainer");
      container.innerHTML = `
                <div class="empty-wishlist" style="grid-column: 1/-1;">
                    <i class="fas fa-heart-broken"></i>
                    <h3>Please login to view your wishlist</h3>
                    <p>Login or create an account to save your favorite items</p>
                    <a href="index.html" class="shop-now-btn">Login</a>
                </div>
            `;
    }

    this.setupBackToTop();
    this.setupSearch();
    this.setupUserDropdown();
    this.setupShareModal();

    const clearBtn = document.getElementById("clearWishlistBtn");
    if (clearBtn) {
      clearBtn.addEventListener("click", () => this.clearWishlist());
    }

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      auth.startTokenRefreshTimer();
    }
  }
}

// Initialize wishlist page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new WishlistPage();
});
