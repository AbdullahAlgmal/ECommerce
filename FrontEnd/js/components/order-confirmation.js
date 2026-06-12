// js/components/order-confirmation.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";

class OrderConfirmation {
  constructor() {
    this.orderId = null;
    this.order = null;
    this.init();
  }

  // Get order ID from URL
  getOrderId() {
    const urlParams = new URLSearchParams(window.location.search);
    this.orderId = urlParams.get("id");

    if (!this.orderId) {
      // Try to get from sessionStorage
      const lastOrderId = sessionStorage.getItem("lastOrderId");
      if (lastOrderId) {
        this.orderId = lastOrderId;
      } else {
        this.showToast("Order information not found", "error");
        setTimeout(() => {
          window.location.href = "orders.html";
        }, 2000);
        return false;
      }
    }
    return true;
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

  // Load order from API
  async loadOrder() {
    try {
      const response = await api.request(
        `/orders/${this.orderId}`,
        "GET",
        null,
        true,
      );

      if (response.success && response.data) {
        this.order = response.data;
        this.renderOrder();
        this.updateStepStatus();
      } else {
        throw new Error("Order not found");
      }
    } catch (error) {
      console.error("Failed to load order:", error);
      this.showToast("Failed to load order details", "error");

      // Try to load from sessionStorage as fallback
      const savedOrder = sessionStorage.getItem("lastOrder");
      if (savedOrder) {
        this.order = JSON.parse(savedOrder);
        this.renderOrder();
      } else {
        setTimeout(() => {
          window.location.href = "orders.html";
        }, 2000);
      }
    }
  }

  // Render order details
  renderOrder() {
    // Update order info
    document.getElementById("orderNumber").textContent = `#${this.order.id}`;
    document.getElementById("orderDate").textContent = new Date(
      this.order.orderDate,
    ).toLocaleDateString();
    document.getElementById("orderTotal").textContent =
      `$${this.order.total.toFixed(2)}`;

    const paymentStatus = document.getElementById("paymentStatus");
    paymentStatus.textContent = this.order.paymentStatus || "Pending";
    if (this.order.paymentStatus === "Completed") {
      paymentStatus.classList.add("completed");
    }

    // Render order items
    this.renderOrderItems();

    // Render shipping address
    this.renderShippingAddress();

    // Render payment method
    this.renderPaymentMethod();

    // Clear sessionStorage
    sessionStorage.removeItem("lastOrderId");
    sessionStorage.removeItem("lastOrder");
    sessionStorage.removeItem("orderTotal");
  }

  // Render order items
  renderOrderItems() {
    const container = document.getElementById("orderItems");

    if (!this.order.items || this.order.items.length === 0) {
      container.innerHTML = '<div class="empty-state">No items found</div>';
      return;
    }

    let itemsHtml = "";
    for (const item of this.order.items) {
      itemsHtml += `
                <div class="order-item">
                    <div class="order-item-image">
                        <img src="${item.productImage || "https://via.placeholder.com/80"}" alt="${item.productName}">
                    </div>
                    <div class="order-item-details">
                        <div class="order-item-name">${item.productName}</div>
                        <div class="order-item-price">$${item.price.toFixed(2)}</div>
                    </div>
                    <div class="order-item-quantity">x${item.quantity}</div>
                    <div class="order-item-total">$${(item.price * item.quantity).toFixed(2)}</div>
                </div>
            `;
    }

    // Add summary
    const summaryHtml = `
            ${itemsHtml}
            <div class="order-summary">
                <div class="summary-row">
                    <span>Subtotal</span>
                    <span>$${this.order.subtotal.toFixed(2)}</span>
                </div>
                <div class="summary-row">
                    <span>Shipping</span>
                    <span>${this.order.shippingCost === 0 ? "Free" : `$${this.order.shippingCost.toFixed(2)}`}</span>
                </div>
                <div class="summary-row">
                    <span>Tax (10%)</span>
                    <span>$${this.order.tax.toFixed(2)}</span>
                </div>
                ${
                  this.order.discount > 0
                    ? `
                <div class="summary-row discount">
                    <span>Discount</span>
                    <span>-$${this.order.discount.toFixed(2)}</span>
                </div>
                `
                    : ""
                }
                <div class="summary-row total">
                    <span>Total</span>
                    <span>$${this.order.total.toFixed(2)}</span>
                </div>
            </div>
        `;

    container.innerHTML = summaryHtml;
  }

  // Render shipping address
  renderShippingAddress() {
    const container = document.getElementById("shippingAddress");

    if (!this.order.shippingAddress) {
      container.innerHTML = "<p>No shipping address provided</p>";
      return;
    }

    const address = this.order.shippingAddress;
    const addressHtml = `
            <div class="shipping-address">
                <p class="address-name">${address.firstName} ${address.lastName}</p>
                <p>${address.address} ${address.apartment ? ", " + address.apartment : ""}</p>
                <p>${address.city}, ${address.state} ${address.zipCode}</p>
                <p>${address.country}</p>
                <p><i class="fas fa-phone"></i> ${this.order.contactInfo?.phone || "N/A"}</p>
            </div>
        `;

    container.innerHTML = addressHtml;
  }

  // Render payment method
  renderPaymentMethod() {
    const container = document.getElementById("paymentMethod");

    const methodNames = {
      card: "Credit Card",
      paypal: "PayPal",
      cod: "Cash on Delivery",
    };

    const methodName =
      methodNames[this.order.paymentMethod] || this.order.paymentMethod;

    let paymentHtml = `
            <div class="payment-info">
                <p><strong>${methodName}</strong></p>
        `;

    if (this.order.paymentMethod === "card" && this.order.paymentDetails) {
      paymentHtml += `
                <p>Card ending in ${this.order.paymentDetails.cardLastFour}</p>
                <p>${this.order.paymentDetails.cardType}</p>
            `;
    } else if (this.order.paymentMethod === "cod") {
      paymentHtml += `
                <p>Pay with cash upon delivery</p>
                <small>Additional $2.99 COD fee applied</small>
            `;
    }

    paymentHtml += `</div>`;
    container.innerHTML = paymentHtml;
  }

  // Update step status
  updateStepStatus() {
    // The steps are already set in HTML for order complete
    // This method can be used to update if needed
  }

  // Print receipt
  printReceipt() {
    window.print();
  }

  // Setup event listeners
  setupEventListeners() {
    const printBtn = document.getElementById("printOrderBtn");
    if (printBtn) {
      printBtn.addEventListener("click", () => this.printReceipt());
    }
  }

  // Update cart count (for header)
  async updateCartCount() {
    if (!auth.user) return;

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
    if (!auth.user) return;

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

  // Initialize order confirmation
  async init() {
    // Check authentication
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to view order confirmation", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    await auth.getCurrentUser();

    if (!this.getOrderId()) {
      return;
    }

    await this.loadOrder();
    this.setupEventListeners();

    // Update cart and wishlist counts
    await this.updateCartCount();
    await this.updateWishlistCount();

    // Start token refresh timer
    auth.startTokenRefreshTimer();
  }
}

// Initialize order confirmation when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new OrderConfirmation();
});
