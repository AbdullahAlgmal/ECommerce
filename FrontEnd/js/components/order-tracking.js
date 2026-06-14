// js/components/order-tracking.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { db } from "../services/indexedDB.js";

class OrderTracking {
  constructor() {
    this.currentTab = "order";
    this.order = null;
    this.init();
  }

  // Get order ID from URL
  getOrderIdFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    const orderId = urlParams.get("id");

    if (orderId) {
      document.getElementById("trackingInput").value = orderId;
      this.trackOrder(orderId);
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

  // Track order by order ID
  async trackOrder(orderId) {
    const loadingState = document.getElementById("loadingState");
    const trackingResult = document.getElementById("trackingResult");
    const notFoundState = document.getElementById("notFoundState");

    // Show loading
    loadingState.style.display = "block";
    trackingResult.style.display = "none";
    notFoundState.style.display = "none";

    try {
      const response = await api.request(
        `/orders/${orderId}`,
        "GET",
        null,
        true,
      );

      loadingState.style.display = "none";

      if (response.success && response.data) {
        this.order = response.data;
        this.renderOrderTracking();
        trackingResult.style.display = "block";

        // Save to sessionStorage for reference
        sessionStorage.setItem("lastTrackedOrder", JSON.stringify(this.order));
      } else {
        notFoundState.style.display = "block";
      }
    } catch (error) {
      console.error("Failed to track order:", error);
      loadingState.style.display = "none";
      notFoundState.style.display = "block";
      this.showToast("Failed to find order", "error");
    }
  }

  // Render order tracking information
  renderOrderTracking() {
    this.renderOrderInfo();
    this.renderTimeline();
    this.renderOrderItems();
    this.renderOrderTotals();
    this.renderShippingAddress();
  }

  // Render order info
  renderOrderInfo() {
    document.getElementById("orderNumber").textContent = `#${this.order.id}`;
    document.getElementById("orderDate").textContent = new Date(
      this.order.orderDate,
    ).toLocaleDateString();
    document.getElementById("orderTotal").textContent =
      `$${this.order.totalAmount.toFixed(2)}`;

    // Get tracking info from shipping if available
    const trackingNumber = this.order.trackingNumber || "Not Available";
    document.getElementById("trackingNumberDisplay").textContent =
      trackingNumber;
  }

  // Get status name from status code
  getStatusName(status) {
    const statusMap = {
      1: "Pending",
      2: "Processing",
      3: "Shipped",
      4: "Delivered",
      5: "Cancelled",
      6: "Refunded",
    };
    return statusMap[status] || "Unknown";
  }

  // Render timeline based on order status
  renderTimeline() {
    const timelineContainer = document.getElementById("timeline");
    const status = this.order.status;

    const timelineSteps = [
      {
        status: 1,
        title: "Order Placed",
        icon: "fa-shopping-cart",
        description: "Your order has been received and confirmed.",
      },
      {
        status: 2,
        title: "Processing",
        icon: "fa-cogs",
        description: "Your order is being processed and prepared for shipping.",
      },
      {
        status: 3,
        title: "Shipped",
        icon: "fa-truck",
        description: "Your order has been shipped and is on its way.",
      },
      {
        status: 4,
        title: "Out for Delivery",
        icon: "fa-truck-fast",
        description: "Your order is out for delivery.",
      },
      {
        status: 5,
        title: "Delivered",
        icon: "fa-check-circle",
        description: "Your order has been delivered.",
      },
    ];

    // Determine which steps are completed based on status
    let completedStatus = 0;
    switch (status) {
      case 1:
        completedStatus = 1;
        break;
      case 2:
        completedStatus = 2;
        break;
      case 3:
        completedStatus = 3;
        break;
      case 4:
        completedStatus = 4;
        break;
      case 5:
        completedStatus = 5;
        break;
      default:
        completedStatus = 1;
    }

    // Get shipping info if available
    const shippingInfo = this.order.shippingInfo || {};
    const estimatedDate =
      shippingInfo.estimatedDeliveryDate || this.getEstimatedDeliveryDate();

    let timelineHtml = "";

    for (let i = 0; i < timelineSteps.length; i++) {
      const step = timelineSteps[i];
      const isCompleted = i + 1 <= completedStatus;
      const isActive = i + 1 === completedStatus;

      let stepDate = "";
      if (isCompleted) {
        if (step.status === 1) {
          stepDate = new Date(this.order.orderDate).toLocaleDateString();
        } else if (step.status === 5 && this.order.deliveryDate) {
          stepDate = new Date(this.order.deliveryDate).toLocaleDateString();
        } else if (isActive && step.status === 3) {
          stepDate = `Estimated: ${estimatedDate}`;
        }
      }

      timelineHtml += `
        <div class="timeline-step ${isCompleted ? "completed" : ""} ${isActive ? "active" : ""}">
          <div class="step-content">
            <div class="step-header">
              <div class="step-icon">
                <i class="fas ${step.icon}"></i>
              </div>
              <div class="step-title">${step.title}</div>
              ${stepDate ? `<div class="step-date">${stepDate}</div>` : ""}
            </div>
            <div class="step-description">${step.description}</div>
          </div>
        </div>
      `;
    }

    timelineContainer.innerHTML = timelineHtml;
  }

  // Get estimated delivery date
  getEstimatedDeliveryDate() {
    const orderDate = new Date(this.order.orderDate);
    let daysToAdd = 7; // Default 7 days

    // Check if order has shipping method
    if (this.order.shippingMethod) {
      switch (this.order.shippingMethod.toLowerCase()) {
        case "express":
          daysToAdd = 3;
          break;
        case "overnight":
          daysToAdd = 1;
          break;
        default:
          daysToAdd = 7;
      }
    }

    const estimatedDate = new Date(orderDate);
    estimatedDate.setDate(orderDate.getDate() + daysToAdd);

    return estimatedDate.toLocaleDateString();
  }

  // Render order items
  renderOrderItems() {
    const container = document.getElementById("orderItems");

    if (!this.order.orderItems || this.order.orderItems.length === 0) {
      container.innerHTML = '<div class="empty-state">No items found</div>';
      return;
    }

    container.innerHTML = this.order.orderItems
      .map(
        (item) => `
        <div class="order-item">
          <div class="item-image">
            <img src="${item.productImage || "https://via.placeholder.com/60"}" alt="${item.productName}">
          </div>
          <div class="item-details">
            <div class="item-name">${item.productName}</div>
            <div class="item-price">$${item.price.toFixed(2)}</div>
          </div>
          <div class="item-quantity">x${item.quantity}</div>
          <div class="item-total">$${(item.price * item.quantity).toFixed(2)}</div>
        </div>
      `,
      )
      .join("");
  }

  // Render order totals
  renderOrderTotals() {
    const container = document.getElementById("orderTotals");

    // Calculate totals from order items if subtotal not provided
    const subtotal =
      this.order.subtotal ||
      this.order.orderItems.reduce(
        (sum, item) => sum + item.price * item.quantity,
        0,
      );
    const shipping = this.order.shippingCost || 0;
    const tax = this.order.tax || subtotal * 0.1;
    const discount = this.order.discount || 0;
    const total =
      this.order.totalAmount || subtotal + shipping + tax - discount;

    container.innerHTML = `
      <div class="totals-row">
        <span>Subtotal</span>
        <span>$${subtotal.toFixed(2)}</span>
      </div>
      <div class="totals-row">
        <span>Shipping</span>
        <span>${shipping === 0 ? "Free" : `$${shipping.toFixed(2)}`}</span>
      </div>
      <div class="totals-row">
        <span>Tax (10%)</span>
        <span>$${tax.toFixed(2)}</span>
      </div>
      ${
        discount > 0
          ? `
      <div class="totals-row">
        <span>Discount</span>
        <span>-$${discount.toFixed(2)}</span>
      </div>
      `
          : ""
      }
      <div class="totals-row total">
        <span>Total</span>
        <span>$${total.toFixed(2)}</span>
      </div>
    `;
  }

  // Render shipping address
  renderShippingAddress() {
    const container = document.getElementById("shippingAddress");

    if (!this.order.shippingAddress) {
      container.innerHTML = "<p>No shipping address available</p>";
      return;
    }

    const address = this.order.shippingAddress;
    container.innerHTML = `
      <div class="shipping-address">
        <p class="address-name">${address.firstName || ""} ${address.lastName || ""}</p>
        <p>${address.address || ""} ${address.apartment || ""}</p>
        <p>${address.city || ""}, ${address.state || ""} ${address.zipCode || ""}</p>
        <p>${address.country || ""}</p>
        <p><i class="fas fa-phone"></i> ${this.order.contactInfo?.phone || "N/A"}</p>
      </div>
    `;
  }

  // Setup tracking form
  setupTrackingForm() {
    const form = document.getElementById("trackingForm");

    form.addEventListener("submit", (e) => {
      e.preventDefault();

      const input = document.getElementById("trackingInput");
      const orderId = input.value.trim();

      if (!orderId) {
        this.showToast("Please enter an order ID", "error");
        return;
      }

      if (!/^\d+$/.test(orderId)) {
        this.showToast("Please enter a valid numeric order ID", "error");
        return;
      }

      this.trackOrder(orderId);
    });
  }

  // Setup try again button
  setupTryAgain() {
    const tryAgainBtn = document.getElementById("tryAgainBtn");

    tryAgainBtn.addEventListener("click", () => {
      document.getElementById("trackingInput").value = "";
      document.getElementById("trackingResult").style.display = "none";
      document.getElementById("notFoundState").style.display = "none";
    });
  }

  // Setup back to top
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

  // Initialize order tracking
  async init() {
    this.setupTrackingForm();
    this.setupTryAgain();
    this.setupBackToTop();
    this.setupSearch();
    this.setupUserDropdown();

    // Update placeholder text for order ID only
    const trackingInput = document.getElementById("trackingInput");
    if (trackingInput) {
      trackingInput.placeholder = "Enter order ID...";
    }

    // Check for order ID in URL
    this.getOrderIdFromUrl();

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      await this.updateWishlistCount();
      auth.startTokenRefreshTimer();
    }
  }
}

// Initialize order tracking when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new OrderTracking();
});
