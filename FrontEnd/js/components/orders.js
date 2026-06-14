// js/components/orders.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { db } from "../services/indexedDB.js";

class OrdersPage {
  constructor() {
    this.orders = [];
    this.filteredOrders = [];
    this.currentPage = 1;
    this.itemsPerPage = 10;
    this.statusFilter = "all";
    this.dateFilter = "all";
    this.searchTerm = "";
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

  // Load orders from API (user-specific)
  async loadOrders() {
    try {
      const userId = auth.user?.id;
      if (!userId) {
        this.showToast("User not found", "error");
        this.renderEmptyState();
        return;
      }

      const response = await api.request(
        `/orders/user/${userId}`,
        "GET",
        null,
        true,
      );

      if (response.success && response.data) {
        this.orders = response.data;
        this.applyFilters();
      } else {
        this.orders = [];
        this.renderEmptyState();
      }
    } catch (error) {
      console.error("Failed to load orders:", error);
      this.showToast("Failed to load orders", "error");
      this.orders = [];
      this.renderEmptyState();
    }
  }

  // Apply filters
  applyFilters() {
    this.filteredOrders = this.orders.filter((order) => {
      // Status filter
      if (
        this.statusFilter !== "all" &&
        order.status !== this.getStatusValue(this.statusFilter)
      ) {
        return false;
      }

      // Date filter
      if (this.dateFilter !== "all") {
        const orderDate = new Date(order.orderDate);
        const daysAgo = parseInt(this.dateFilter);
        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - daysAgo);

        if (orderDate < cutoffDate) {
          return false;
        }
      }

      // Search filter
      if (this.searchTerm) {
        const orderIdMatch = order.id.toString().includes(this.searchTerm);
        const trackingMatch = order.trackingNumber
          ?.toLowerCase()
          .includes(this.searchTerm.toLowerCase());
        if (!orderIdMatch && !trackingMatch) {
          return false;
        }
      }

      return true;
    });

    this.currentPage = 1;
    this.renderOrders();
    this.renderPagination();
  }

  // Get status value (convert string to number)
  getStatusValue(statusString) {
    const statusMap = {
      pending: 1,
      processing: 2,
      shipped: 3,
      delivered: 4,
      cancelled: 5,
    };
    return statusMap[statusString] || null;
  }

  // Get status badge class
  getStatusClass(status) {
    const statusMap = {
      1: "pending",
      2: "processing",
      3: "shipped",
      4: "delivered",
      5: "cancelled",
    };
    return statusMap[status] || "pending";
  }

  // Get status text
  getStatusText(status) {
    const statusMap = {
      1: "Pending",
      2: "Processing",
      3: "Shipped",
      4: "Delivered",
      5: "Cancelled",
    };
    return statusMap[status] || "Unknown";
  }

  // Render orders
  renderOrders() {
    const container = document.getElementById("ordersList");

    if (this.filteredOrders.length === 0) {
      this.renderEmptyState();
      return;
    }

    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    const paginatedOrders = this.filteredOrders.slice(start, end);

    container.innerHTML = paginatedOrders
      .map(
        (order) => `
            <div class="order-card" data-order-id="${order.id}">
                <div class="order-header">
                    <div class="order-info">
                        <div class="order-number">
                            <span class="label">Order Number</span>
                            <span class="value">#${order.id}</span>
                        </div>
                        <div class="order-date">
                            <span class="label">Order Date</span>
                            <span class="value">${new Date(order.orderDate).toLocaleDateString()}</span>
                        </div>
                        <div class="order-total">
                            <span class="label">Total Amount</span>
                            <span class="value">$${order.totalAmount.toFixed(2)}</span>
                        </div>
                    </div>
                    <div class="order-status">
                        <span class="status-badge ${this.getStatusClass(order.status)}">
                            ${this.getStatusText(order.status)}
                        </span>
                    </div>
                    <div class="order-actions">
                        <button class="view-order-btn" data-id="${order.id}">
                            <i class="fas fa-eye"></i> View Details
                        </button>
                        <button class="track-order-btn" data-id="${order.id}">
                            <i class="fas fa-truck"></i> Track Order
                        </button>
                    </div>
                </div>
                <div class="order-items-preview">
                    <div class="preview-items">
                        ${this.renderOrderPreview(order.orderItems)}
                    </div>
                </div>
                ${
                  order.trackingNumber
                    ? `
                <div class="order-footer">
                    <a href="order-tracking.html?number=${order.trackingNumber}" class="tracking-link">
                        <i class="fas fa-shipping-fast"></i> Track Package: ${order.trackingNumber}
                    </a>
                </div>
                `
                    : ""
                }
            </div>
        `,
      )
      .join("");

    // Add event listeners
    document.querySelectorAll(".view-order-btn").forEach((btn) => {
      btn.addEventListener("click", () =>
        this.showOrderDetails(parseInt(btn.dataset.id)),
      );
    });

    document.querySelectorAll(".track-order-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const order = this.orders.find(
          (o) => o.id === parseInt(btn.dataset.id),
        );
        if (order) {
          window.location.href = `order-tracking.html?number=${order.id}`;
        } else {
          this.showToast("Tracking information not available yet", "error");
        }
      });
    });
  }

  // Render order preview items
  renderOrderPreview(items) {
    if (!items || items.length === 0) return "<span>No items</span>";

    const previewItems = items.slice(0, 3);
    const remainingCount = items.length - 3;

    let html = previewItems
      .map(
        (item) => `
            <div class="preview-item">
                <img src="${item.productImage || "https://via.placeholder.com/40"}" alt="${item.productName}">
                <span>${item.productName} x${item.quantity}</span>
            </div>
        `,
      )
      .join("");

    if (remainingCount > 0) {
      html += `<div class="more-items">+${remainingCount} more</div>`;
    }

    return html;
  }

  // Show order details modal
  async showOrderDetails(orderId) {
    const order = this.orders.find((o) => o.id === orderId);
    if (!order) return;

    const modal = document.getElementById("orderModal");
    const modalBody = document.getElementById("orderModalBody");

    modal.classList.add("active");

    modalBody.innerHTML = `
            <div class="order-details">
                <div class="detail-section">
                    <h4>Order Information</h4>
                    <div class="detail-grid">
                        <div class="detail-item">
                            <span class="label">Order Number</span>
                            <span class="value">#${order.id}</span>
                        </div>
                        <div class="detail-item">
                            <span class="label">Order Date</span>
                            <span class="value">${new Date(order.orderDate).toLocaleString()}</span>
                        </div>
                        <div class="detail-item">
                            <span class="label">Status</span>
                            <span class="value status-badge ${this.getStatusClass(order.status)}">${this.getStatusText(order.status)}</span>
                        </div>
                        <div class="detail-item">
                            <span class="label">Payment Method</span>
                            <span class="value">${order.paymentMethod || "Credit Card"}</span>
                        </div>
                    </div>
                </div>
                
                <div class="detail-section">
                    <h4>Order Items</h4>
                    <div class="order-items-list">
                        ${order.orderItems
                          .map(
                            (item) => `
                            <div class="order-item">
                                <img src="${item.productImage || "https://via.placeholder.com/60"}" alt="${item.productName}">
                                <div class="item-info">
                                    <div class="item-name">${item.productName}</div>
                                    <div class="item-price">$${item.price.toFixed(2)}</div>
                                </div>
                                <div class="item-quantity">x${item.quantity}</div>
                                <div class="item-total">$${(item.price * item.quantity).toFixed(2)}</div>
                            </div>
                        `,
                          )
                          .join("")}
                    </div>
                    <div class="order-summary">
                        <div class="summary-row">
                            <span>Subtotal</span>
                            <span>$${order.subtotal?.toFixed(2) || (order.totalAmount - 10).toFixed(2)}</span>
                        </div>
                        <div class="summary-row">
                            <span>Shipping</span>
                            <span>${order.shippingCost === 0 ? "Free" : `$${order.shippingCost?.toFixed(2) || "5.99"}`}</span>
                        </div>
                        <div class="summary-row">
                            <span>Tax (10%)</span>
                            <span>$${order.tax?.toFixed(2) || (order.totalAmount * 0.09).toFixed(2)}</span>
                        </div>
                        <div class="summary-row total">
                            <span>Total</span>
                            <span>$${order.totalAmount.toFixed(2)}</span>
                        </div>
                    </div>
                </div>
                
                <div class="detail-section">
                    <h4>Shipping Address</h4>
                    <div class="shipping-address">
                        <p>${order.shippingAddress?.firstName || ""} ${order.shippingAddress?.lastName || ""}</p>
                        <p>${order.shippingAddress?.address || ""} ${order.shippingAddress?.apartment || ""}</p>
                        <p>${order.shippingAddress?.city || ""}, ${order.shippingAddress?.state || ""} ${order.shippingAddress?.zipCode || ""}</p>
                        <p>${order.shippingAddress?.country || ""}</p>
                    </div>
                </div>
                
                ${
                  order.trackingNumber
                    ? `
                <div class="detail-section">
                    <h4>Tracking Information</h4>
                    <div class="tracking-info">
                        <p><strong>Tracking Number:</strong> <span class="tracking-number">${order.trackingNumber}</span></p>
                        <div class="tracking-link">
                            <a href="order-tracking.html?number=${order.trackingNumber}">
                                <i class="fas fa-truck"></i> Track your package
                            </a>
                        </div>
                    </div>
                </div>
                `
                    : ""
                }
            </div>
        `;
  }

  // Render empty state
  renderEmptyState() {
    const container = document.getElementById("ordersList");
    container.innerHTML = `
            <div class="empty-orders">
                <i class="fas fa-shopping-bag"></i>
                <h3>No orders found</h3>
                <p>You haven't placed any orders yet.</p>
                <a href="shop.html" class="shop-now-btn">Start Shopping</a>
            </div>
        `;
    document.getElementById("pagination").innerHTML = "";
  }

  // Render pagination
  renderPagination() {
    const container = document.getElementById("pagination");
    const totalPages = Math.ceil(
      this.filteredOrders.length / this.itemsPerPage,
    );

    if (totalPages <= 1) {
      container.innerHTML = "";
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
    container.innerHTML = paginationHtml;

    container.querySelectorAll("[data-page]").forEach((btn) => {
      btn.addEventListener("click", () => {
        this.currentPage = parseInt(btn.dataset.page);
        this.renderOrders();
        this.renderPagination();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    });

    const prevBtn = container.querySelector(".pagination-prev");
    const nextBtn = container.querySelector(".pagination-next");

    if (prevBtn && !prevBtn.disabled) {
      prevBtn.addEventListener("click", () => {
        this.currentPage--;
        this.renderOrders();
        this.renderPagination();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }

    if (nextBtn && !nextBtn.disabled) {
      nextBtn.addEventListener("click", () => {
        this.currentPage++;
        this.renderOrders();
        this.renderPagination();
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }
  }

  // Setup filters
  setupFilters() {
    const statusFilter = document.getElementById("statusFilter");
    const dateFilter = document.getElementById("dateFilter");
    const orderSearch = document.getElementById("orderSearch");

    if (statusFilter) {
      statusFilter.addEventListener("change", () => {
        this.statusFilter = statusFilter.value;
        this.applyFilters();
      });
    }

    if (dateFilter) {
      dateFilter.addEventListener("change", () => {
        this.dateFilter = dateFilter.value;
        this.applyFilters();
      });
    }

    if (orderSearch) {
      let searchTimeout;
      orderSearch.addEventListener("input", () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
          this.searchTerm = orderSearch.value.trim();
          this.applyFilters();
        }, 300);
      });
    }
  }

  // Setup modal close
  setupModal() {
    const modal = document.getElementById("orderModal");
    const closeBtn = document.getElementById("closeModalBtn");

    if (closeBtn) {
      closeBtn.addEventListener("click", () => {
        modal.classList.remove("active");
      });
    }

    modal.addEventListener("click", (e) => {
      if (e.target === modal) {
        modal.classList.remove("active");
      }
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

  // Initialize orders page
  async init() {
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to view your orders", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    await auth.getCurrentUser();
    await this.loadOrders();

    this.setupFilters();
    this.setupModal();
    this.setupBackToTop();
    this.setupSearch();
    this.setupUserDropdown();

    await this.updateCartCount();
    await this.updateWishlistCount();

    auth.startTokenRefreshTimer();
  }
}

// Initialize orders page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new OrdersPage();
});
