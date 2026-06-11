// js/components/cart.js

import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

class CartPage {
  constructor() {
    this.cartItems = [];
    this.promoApplied = false;
    this.discount = 0;
    this.shippingCost = 5.99;
    this.taxRate = 0.1;
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

  // Load cart items from IndexedDB
  async loadCartItems() {
    const container = document.getElementById("cartItemsContainer");
    if (!container) return;

    try {
      const userId = auth.user?.id;
      if (!userId) {
        container.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>Your cart is empty</h3>
                        <p>Please login to view your cart</p>
                        <button class="shop-now-btn" onclick="window.location.href='index.html'">Login</button>
                    </div>
                `;
        return;
      }

      const cart = await db.getAll("cart");
      this.cartItems = cart.filter((item) => item.userId === userId);

      // Get product details for each cart item
      const products = await db.getAll("products");
      this.cartItems = this.cartItems.map((item) => {
        const product = products.find((p) => p.id === item.productId);
        return {
          ...item,
          cartId: item.id,
        };
      });

      if (this.cartItems.length === 0) {
        container.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>Your cart is empty</h3>
                        <p>Looks like you haven't added any items to your cart yet.</p>
                        <button class="shop-now-btn" onclick="window.location.href='shop.html'">Start Shopping</button>
                    </div>
                `;
        this.updateSummary();
        return;
      }

      this.renderCartItems();
      this.updateSummary();
      this.updateCartCount();
    } catch (error) {
      console.error("Failed to load cart:", error);
      container.innerHTML = `
                <div class="empty-cart">
                    <i class="fas fa-exclamation-circle"></i>
                    <h3>Failed to load cart</h3>
                    <p>Please try again later</p>
                </div>
            `;
    }
  }

  // Render cart items
  renderCartItems() {
    const container = document.getElementById("cartItemsContainer");

    let itemsHtml = `
            <div class="cart-items-list">
        `;

    for (const item of this.cartItems) {
      const itemTotal = item.productPrice * item.quantity;
      itemsHtml += `
                <div class="cart-item" data-cart-id="${item.cartId}" data-product-id="${item.productId}">
                    <div class="cart-item-image">
                        <img loading="lazy" src="${item.images?.[0]?.url || "https://via.placeholder.com/300"}" alt="${item.name}">
                    </div>
                    <div class="cart-item-details">
                        <h3>${item.productName}</h3>
                        <div class="cart-item-price">$${item.productPrice.toFixed(2)}</div>
                        <div class="cart-item-stock">
                            ${
                              item.quantity > 0
                                ? '<span class="in-stock"><i class="fas fa-check-circle"></i> In Stock</span>'
                                : '<span class="out-of-stock"><i class="fas fa-times-circle"></i> Out of Stock</span>'
                            }
                        </div>
                    </div>
                    <div class="cart-item-actions">
                        <div class="quantity-selector">
                            <button class="decrease-qty" ${item.quantity <= 1 ? "disabled" : ""}>
                                <i class="fas fa-minus"></i>
                            </button>
                            <span editable="true">${item.quantity}</span>
                            <button class="increase-qty">
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                        <div class="cart-item-total">
                            <div class="item-total-label">Total:</div>
                            <div class="item-total">$${itemTotal.toFixed(2)}</div>
                        </div>
                        <button class="remove-item">
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </div>
                </div>
            `;
    }

    itemsHtml += `</div>`;
    container.innerHTML = itemsHtml;

    // Add event listeners
    this.setupItemEvents();
  }

  // Setup item event listeners
  setupItemEvents() {
    // Decrease quantity buttons
    document.querySelectorAll(".decrease-qty").forEach((btn, index) => {
      btn.addEventListener("click", async () => {
        const cartItem = this.cartItems[index];
        if (cartItem.quantity > 1) {
          cartItem.quantity--;
          await this.updateCartItem(cartItem);
        }
      });
    });

    // Increase quantity buttons
    document.querySelectorAll(".increase-qty").forEach((btn, index) => {
      btn.addEventListener("click", async () => {
        const cartItem = this.cartItems[index];
        cartItem.quantity++;
        await this.updateCartItem(cartItem);
      });
    });

    // Remove item buttons
    document.querySelectorAll(".remove-item").forEach((btn, index) => {
      btn.addEventListener("click", async () => {
        const cartItem = this.cartItems[index];
        await this.removeCartItem(cartItem.cartId);
      });
    });
  }

  // Update cart item in IndexedDB
  async updateCartItem(cartItem) {
    try {
      const cart = await db.getAll("cart");
      const itemToUpdate = cart.find((item) => item.id === cartItem.cartId);
      if (itemToUpdate) {
        itemToUpdate.quantity = cartItem.quantity;
        await db.put("cart", itemToUpdate);

        // Update local array
        const index = this.cartItems.findIndex(
          (item) => item.cartId === cartItem.cartId,
        );
        if (index !== -1) {
          this.cartItems[index].quantity = cartItem.quantity;
        }

        this.renderCartItems();
        this.updateSummary();
        this.updateCartCount();
        this.showToast("Cart updated", "success");
      }
    } catch (error) {
      console.error("Failed to update cart:", error);
      this.showToast("Failed to update cart", "error");
    }
  }

  // Remove cart item
  async removeCartItem(cartId) {
    try {
      await db.delete("cart", cartId);

      // Remove from local array
      this.cartItems = this.cartItems.filter((item) => item.cartId !== cartId);

      if (this.cartItems.length === 0) {
        const container = document.getElementById("cartItemsContainer");
        container.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>Your cart is empty</h3>
                        <p>Looks like you haven't added any items to your cart yet.</p>
                        <button class="shop-now-btn" onclick="window.location.href='shop.html'">Start Shopping</button>
                    </div>
                `;
      } else {
        this.renderCartItems();
      }

      this.updateSummary();
      this.updateCartCount();
      this.showToast("Item removed from cart", "success");
    } catch (error) {
      console.error("Failed to remove item:", error);
      this.showToast("Failed to remove item", "error");
    }
  }

  // Update cart summary
  updateSummary() {
    const subtotal = this.cartItems.reduce(
      (sum, item) => sum + item.productPrice * item.quantity,
      0,
    );

    // Free shipping over $50
    const shipping = subtotal > 50 ? 0 : this.shippingCost;
    const tax = subtotal * this.taxRate;
    let total = subtotal + shipping + tax;

    // Apply discount if promo code applied
    if (this.promoApplied) {
      total -= this.discount;
    }

    document.getElementById("subtotal").textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById("shipping").textContent =
      shipping === 0 ? "Free" : `$${shipping.toFixed(2)}`;
    document.getElementById("tax").textContent = `$${tax.toFixed(2)}`;
    document.getElementById("total").textContent = `$${total.toFixed(2)}`;
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

  // Update wishlist count badge
  async updateWishlistCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
  }

  // Apply promo code
  setupPromoCode() {
    const promoInput = document.getElementById("promoCode");
    const applyBtn = document.getElementById("applyPromoBtn");

    applyBtn.addEventListener("click", () => {
      const code = promoInput.value.trim().toUpperCase();

      if (code === "SAVE10" && !this.promoApplied) {
        this.discount =
          this.cartItems.reduce(
            (sum, item) => sum + item.productPrice * item.quantity,
            0,
          ) * 0.1;
        this.promoApplied = true;
        this.updateSummary();
        this.showToast("Promo code applied! 10% discount", "success");
        applyBtn.disabled = true;
        promoInput.disabled = true;
      } else if (this.promoApplied) {
        this.showToast("Promo code already applied", "error");
      } else {
        this.showToast("Invalid promo code", "error");
      }
    });
  }

  // Setup checkout
  setupCheckout() {
    const checkoutBtn = document.getElementById("checkoutBtn");
    checkoutBtn.addEventListener("click", async () => {
      if (!auth.isAuthenticated()) {
        this.showToast("Please login to checkout", "error");
        setTimeout(() => {
          window.location.href = "index.html";
        }, 1500);
        return;
      }

      if (this.cartItems.length === 0) {
        this.showToast("Your cart is empty", "error");
        return;
      }

      // Store cart data for checkout page
      sessionStorage.setItem("checkoutCart", JSON.stringify(this.cartItems));
      sessionStorage.setItem(
        "checkoutTotal",
        document.getElementById("total").textContent,
      );

      window.location.href = "checkout.html";
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

  // Initialize cart page
  async init() {
    // Check authentication and load user info
    if (auth.isAuthenticated()) {
      await auth.getCurrentUser();
      await this.loadCartItems();
      await this.updateWishlistCount();
    } else {
      const container = document.getElementById("cartItemsContainer");
      container.innerHTML = `
                <div class="empty-cart">
                    <i class="fas fa-shopping-cart"></i>
                    <h3>Please login to view your cart</h3>
                    <p>Login or create an account to start shopping</p>
                    <button class="shop-now-btn" onclick="window.location.href='index.html'">Login</button>
                </div>
            `;
    }

    this.setupPromoCode();
    this.setupCheckout();
    this.setupUserDropdown();

    // Start token refresh timer
    auth.startTokenRefreshTimer();

    // Update cart count periodically
    setInterval(() => this.updateCartCount(), 5000);
  }
}

// Initialize cart page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new CartPage();
});
