// js/components/dashboard.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { storage, urlHelpers } from "../utils/helpers.js";

class Dashboard {
  constructor() {
    this.currentPage = "overview";
    this.user = null;
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

  // Update date and time
  updateDateTime() {
    const now = new Date();
    const options = {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    };
    const dateTimeElement = document.getElementById("currentDateTime");
    if (dateTimeElement) {
      dateTimeElement.textContent = now.toLocaleDateString("en-US", options);
    }
  }

  // Load user info
  async loadUserInfo() {
    try {
      const response = await api.getCurrentUser();
      if (response.success) {
        this.user = response.data;
        const userNameElement = document.getElementById("userName");
        if (userNameElement) {
          userNameElement.textContent = `${this.user.firstName} ${this.user.lastName}`;
        }
      }
    } catch (error) {
      console.error("Failed to load user info:", error);
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        window.location.href = "../index.html";
      }
    }
  }

  // Load dashboard statistics
  async loadStatistics() {
    const isAdmin = window.location.pathname.includes("admin.html");

    try {
      if (isAdmin) {
        // Admin statistics
        const usersResponse = await api.request(
          "/users/statistics",
          "GET",
          null,
          true,
        );
        const productsResponse = await api.request(
          "/products/statistics",
          "GET",
          null,
          true,
        );
        const ordersResponse = await api.request(
          "/orders/statistics",
          "GET",
          null,
          true,
        );

        if (usersResponse.success) {
          document.getElementById("totalUsers").textContent =
            usersResponse.data;
        }
        if (productsResponse.success) {
          document.getElementById("totalProducts").textContent =
            productsResponse.data.totalProducts || 0;
        }
        if (ordersResponse.success) {
          document.getElementById("totalOrders").textContent =
            ordersResponse.data.totalOrders || 0;
          document.getElementById("totalRevenue").textContent =
            `$${(ordersResponse.data.totalRevenue || 0).toFixed(2)}`;
        }
      } else {
        // Customer statistics
        const ordersResponse = await api.request(
          "/orders/user/statistics",
          "GET",
          null,
          true,
        );
        if (ordersResponse.success) {
          document.getElementById("totalOrders").textContent =
            ordersResponse.data.totalOrders || 0;
          document.getElementById("totalSpent").textContent =
            `$${(ordersResponse.data.totalSpent || 0).toFixed(2)}`;
          document.getElementById("activeOrders").textContent =
            ordersResponse.data.activeOrders || 0;
        }
      }
    } catch (error) {
      console.error("Failed to load statistics:", error);
    }
  }

  // Load recent orders
  async loadRecentOrders() {
    const tableContainer = document.getElementById("recentOrdersTable");
    if (!tableContainer) return;

    try {
      const isAdmin = window.location.pathname.includes("admin.html");
      let orders = [];

      if (isAdmin) {
        const response = await api.request("/orders", "GET", null, true);
        if (response.success) orders = response.data.slice(0, 5);
      } else {
        const response = await api.request(
          "/orders/user/" + this.user?.id,
          "GET",
          null,
          true,
        );
        if (response.success) orders = response.data.slice(0, 5);
      }

      if (orders.length === 0) {
        tableContainer.innerHTML = `
                    <div class="empty-state">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>No orders yet</h3>
                        <p>Start shopping to see your orders here</p>
                    </div>
                `;
        return;
      }

      let tableHtml = `
                <table>
                    <thead>
                        <tr>
                            <th>Order ID</th>
                            <th>Date</th>
                            <th>Total</th>
                            <th>Status</th>
                            ${isAdmin ? "<th>Customer</th>" : ""}
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

      for (const order of orders) {
        const statusBadge = this.getStatusBadge(order.status);
        tableHtml += `
                    <tr>
                        <td>#${order.id}</td>
                        <td>${order.orderDate}</td>
                        <td>$${order.totalAmount.toFixed(2)}</td>
                        <td>${statusBadge}</td>
                        ${isAdmin ? `<td>${order.userFullName || "N/A"}</td>` : ""}
                        <td><button class="quick-action-btn" onclick="viewOrder(${order.id})" style="padding: 4px 12px;">View</button></td>
                    </tr>
                `;
      }

      tableHtml += `</tbody></table>`;
      tableContainer.innerHTML = tableHtml;
    } catch (error) {
      console.error("Failed to load recent orders:", error);
      tableContainer.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-exclamation-circle"></i>
                    <h3>Failed to load orders</h3>
                    <p>Please try again later</p>
                </div>
            `;
    }
  }

  // Get status badge HTML
  getStatusBadge(status) {
    const statusMap = {
      1: { text: "Pending", class: "badge-warning" },
      2: { text: "Processing", class: "badge-info" },
      3: { text: "Shipped", class: "badge-info" },
      4: { text: "Delivered", class: "badge-success" },
      5: { text: "Cancelled", class: "badge-error" },
      6: { text: "Refunded", class: "badge-warning" },
    };
    const s = statusMap[status] || { text: "Unknown", class: "badge-warning" };
    return `<span class="badge ${s.class}">${s.text}</span>`;
  }

  // Load all users (admin only)
  async loadAllUsers() {
    const tableContainer = document.getElementById("allUsersTable");
    if (!tableContainer) return;

    try {
      const response = await api.request("/users", "GET", null, true);
      const users = response.success ? response.data : [];

      if (users.length === 0) {
        tableContainer.innerHTML =
          '<div class="empty-state"><i class="fas fa-users"></i><h3>No users found</h3></div>';
        return;
      }

      let tableHtml = `
                <table>
                    <thead>
                        <tr><th>ID</th><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th>Action</th></tr>
                    </thead>
                    <tbody>
            `;

      for (const user of users) {
        const statusBadge = user.isActive
          ? '<span class="badge badge-success">Active</span>'
          : '<span class="badge badge-error">Inactive</span>';
        tableHtml += `
                    <tr>
                        <td>${user.id}</td>
                        <td>${user.firstName} ${user.lastName}</td>
                        <td>${user.email}</td>
                        <td>${user.role}</td>
                        <td>${statusBadge}</td>
                        <td><button class="quick-action-btn" onclick="editUser(${user.id})" style="padding: 4px 12px;">Edit</button></td>
                    </tr>
                `;
      }

      tableHtml += `</tbody></table>`;
      tableContainer.innerHTML = tableHtml;
    } catch (error) {
      console.error("Failed to load users:", error);
    }
  }

  // Load all products (admin only)
  async loadAllProducts() {
    const tableContainer = document.getElementById("allProductsTable");
    if (!tableContainer) return;

    try {
      const response = await api.request("/products", "GET");
      const products = response.success ? response.data : [];

      if (products.length === 0) {
        tableContainer.innerHTML =
          '<div class="empty-state"><i class="fas fa-box"></i><h3>No products found</h3></div>';
        return;
      }

      let tableHtml = `
                <table>
                    <thead>
                        <tr><th>ID</th><th>Name</th><th>Price</th><th>Stock</th><th>Category</th><th>Action</th></tr>
                    </thead>
                    <tbody>
            `;

      for (const product of products) {
        const stockBadge =
          product.quantity > 0
            ? '<span class="badge badge-success">In Stock</span>'
            : '<span class="badge badge-error">Out of Stock</span>';
        tableHtml += `
                    <tr>
                        <td>${product.id}</td>
                        <td>${product.name}</td>
                        <td>$${product.price.toFixed(2)}</td>
                        <td>${product.quantity}</td>
                        <td>${stockBadge}</td>
                        <td><button class="quick-action-btn" onclick="editProduct(${product.id})" style="padding: 4px 12px;">Edit</button></td>
                    </tr>
                `;
      }

      tableHtml += `</tbody></table>`;
      tableContainer.innerHTML = tableHtml;
    } catch (error) {
      console.error("Failed to load products:", error);
    }
  }

  // Load all orders (admin only)
  async loadAllOrders() {
    const tableContainer = document.getElementById("allOrdersTable");
    if (!tableContainer) return;

    try {
      const response = await api.request("/orders", "GET", null, true);
      const orders = response.success ? response.data : [];

      if (orders.length === 0) {
        tableContainer.innerHTML =
          '<div class="empty-state"><i class="fas fa-shopping-cart"></i><h3>No orders found</h3></div>';
        return;
      }

      let tableHtml = `
                <table>
                    <thead>
                        <tr><th>Order ID</th><th>Customer</th><th>Date</th><th>Total</th><th>Status</th><th>Action</th></tr>
                    </thead>
                    <tbody>
            `;

      for (const order of orders) {
        const statusBadge = this.getStatusBadge(order.status);
        tableHtml += `
                    <tr>
                        <td>#${order.id}</td>
                        <td>${order.userFullName || "N/A"}</td>
                        <td>${order.orderDate}</td>
                        <td>$${order.totalAmount.toFixed(2)}</td>
                        <td>${statusBadge}</td>
                        <td><button class="quick-action-btn" onclick="viewOrder(${order.id})" style="padding: 4px 12px;">View</button></td>
                    </tr>
                `;
      }

      tableHtml += `</tbody></table>`;
      tableContainer.innerHTML = tableHtml;
    } catch (error) {
      console.error("Failed to load orders:", error);
    }
  }

  // Navigation handler
  setupNavigation() {
    const navItems = document.querySelectorAll(".nav-item");
    const pages = document.querySelectorAll(".dashboard-page");

    navItems.forEach((item) => {
      item.addEventListener("click", (e) => {
        e.preventDefault();
        const page = item.dataset.page;
        if (!page) return;

        // Update active state
        navItems.forEach((nav) => nav.classList.remove("active"));
        item.classList.add("active");

        // Show selected page
        pages.forEach((p) => (p.style.display = "none"));
        const activePage = document.getElementById(`${page}Page`);
        if (activePage) activePage.style.display = "block";

        // Update page title
        const pageTitle = document.getElementById("pageTitle");
        if (pageTitle)
          pageTitle.textContent = item.querySelector("span").textContent;

        this.currentPage = page;

        // Load page-specific data
        this.loadPageData(page);
      });
    });

    // Handle view-all links
    const viewAllLinks = document.querySelectorAll(".view-all");
    viewAllLinks.forEach((link) => {
      link.addEventListener("click", (e) => {
        e.preventDefault();
        const page = link.dataset.page;
        if (page) {
          const navItem = document.querySelector(
            `.nav-item[data-page="${page}"]`,
          );
          if (navItem) navItem.click();
        }
      });
    });
  }

  // Load page-specific data
  loadPageData(page) {
    const isAdmin = window.location.pathname.includes("admin.html");

    switch (page) {
      case "users":
        if (isAdmin) this.loadAllUsers();
        break;
      case "products":
        if (isAdmin) this.loadAllProducts();
        break;
      case "orders":
        if (isAdmin) this.loadAllOrders();
        break;
    }
  }

  // Handle logout
  setupLogout() {
    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", async () => {
        await auth.logout();
        window.location.href = "../index.html";
      });
    }
  }

  // Profile Form Handler
  setupProfileForm() {
    const profileForm = document.getElementById("profileForm");
    if (profileForm && this.user) {
      // Populate form with user data
      document.getElementById("profileFirstName").value =
        this.user.firstName || "";
      document.getElementById("profileLastName").value =
        this.user.lastName || "";
      document.getElementById("profileEmail").value = this.user.email || "";
      document.getElementById("profilePhone").value = this.user.phone || "";
      document.getElementById("profileDob").value = this.user.dateofBirth || "";

      profileForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const button = profileForm.querySelector('button[type="submit"]');
        this.setButtonLoading(button, true);

        const updateData = {
          firstName: document.getElementById("profileFirstName").value.trim(),
          lastName: document.getElementById("profileLastName").value.trim(),
          phone: document.getElementById("profilePhone").value.trim(),
          dateOfBirth: document.getElementById("profileDob").value,
        };

        // Validation
        if (!updateData.firstName || updateData.firstName.length < 2) {
          this.showToast("First name must be at least 2 characters", "error");
          this.setButtonLoading(button, false);
          return;
        }

        if (!updateData.lastName || updateData.lastName.length < 2) {
          this.showToast("Last name must be at least 2 characters", "error");
          this.setButtonLoading(button, false);
          return;
        }

        try {
          const response = await api.updateUser(this.user.id, updateData);
          if (response.success) {
            this.user = { ...this.user, ...updateData };
            this.showToast("Profile updated successfully!", "success");
            // Update sidebar name
            const userNameSpan = document.getElementById("userName");
            if (userNameSpan) {
              userNameSpan.textContent = `${updateData.firstName} ${updateData.lastName}`;
            }
          } else {
            this.showToast(response.message || "Update failed", "error");
          }
        } catch (error) {
          this.showToast(error.message || "An error occurred", "error");
        } finally {
          this.setButtonLoading(button, false);
        }
      });
    }

    // Password change form
    const passwordForm = document.getElementById("passwordForm");
    if (passwordForm) {
      // Password strength checker
      const newPasswordInput = document.getElementById("newPassword");
      const strengthBar = document.getElementById("strengthBar");
      const strengthContainer = document.getElementById("passwordStrength");
      const requirements = {
        length: document.getElementById("reqLength"),
        lowercase: document.getElementById("reqLowercase"),
        uppercase: document.getElementById("reqUppercase"),
        number: document.getElementById("reqNumber"),
      };

      const checkPasswordStrength = (password) => {
        const hasMinLength = password.length >= 6;
        const hasLowercase = /[a-z]/.test(password);
        const hasUppercase = /[A-Z]/.test(password);
        const hasNumber = /[0-9]/.test(password);

        // Update requirements
        this.updateRequirement(requirements.length, hasMinLength);
        this.updateRequirement(requirements.lowercase, hasLowercase);
        this.updateRequirement(requirements.uppercase, hasUppercase);
        this.updateRequirement(requirements.number, hasNumber);

        // Calculate strength
        let strength = 0;
        if (hasMinLength) strength++;
        if (hasLowercase) strength++;
        if (hasUppercase) strength++;
        if (hasNumber) strength++;

        // Show/hide strength meter
        if (password.length > 0) {
          strengthContainer.style.display = "block";
        } else {
          strengthContainer.style.display = "none";
        }

        // Update strength bar
        strengthBar.className = "strength-bar";
        if (strength <= 1) {
          strengthBar.classList.add("weak");
        } else if (strength === 2) {
          strengthBar.classList.add("fair");
        } else if (strength === 3) {
          strengthBar.classList.add("good");
        } else if (strength === 4) {
          strengthBar.classList.add("strong");
        }
      };

      newPasswordInput.addEventListener("input", (e) => {
        checkPasswordStrength(e.target.value);
      });

      // Toggle password visibility
      document.querySelectorAll(".toggle-password").forEach((button) => {
        button.addEventListener("click", () => {
          const targetId = button.dataset.target;
          const input = document.getElementById(targetId);
          const type =
            input.getAttribute("type") === "password" ? "text" : "password";
          input.setAttribute("type", type);
          const icon = button.querySelector("i");
          icon.classList.toggle("fa-eye");
          icon.classList.toggle("fa-eye-slash");
        });
      });

      passwordForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const button = passwordForm.querySelector('button[type="submit"]');
        this.setButtonLoading(button, true);

        const currentPassword =
          document.getElementById("currentPassword").value;
        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword =
          document.getElementById("confirmNewPassword").value;

        if (!currentPassword) {
          this.showToast("Current password is required", "error");
          this.setButtonLoading(button, false);
          return;
        }

        if (newPassword.length < 6) {
          this.showToast("New password must be at least 6 characters", "error");
          this.setButtonLoading(button, false);
          return;
        }

        if (newPassword !== confirmPassword) {
          this.showToast("Passwords do not match", "error");
          this.setButtonLoading(button, false);
          return;
        }

        try {
          const response = await api.request(
            "/auth/change-password",
            "POST",
            {
              currentPassword: currentPassword,
              newPassword: newPassword,
            },
            true,
          );

          if (response.success) {
            this.showToast("Password changed successfully!", "success");
            passwordForm.reset();
            strengthContainer.style.display = "none";
          } else {
            this.showToast(
              response.message || "Password change failed",
              "error",
            );
          }
        } catch (error) {
          this.showToast(error.message || "An error occurred", "error");
        } finally {
          this.setButtonLoading(button, false);
        }
      });
    }
  }

  updateRequirement(element, isValid) {
    if (isValid) {
      element.classList.add("valid");
      element.querySelector("i").className = "fas fa-check-circle";
    } else {
      element.classList.remove("valid");
      element.querySelector("i").className = "fas fa-circle";
    }
  }

  setButtonLoading(button, isLoading) {
    const btnText = button.querySelector("span:not(.btn-loader)");
    const btnLoader = button.querySelector(".btn-loader");

    if (isLoading) {
      button.disabled = true;
      if (btnText) btnText.style.display = "none";
      if (btnLoader) btnLoader.style.display = "inline-block";
    } else {
      button.disabled = false;
      if (btnText) btnText.style.display = "inline-block";
      if (btnLoader) btnLoader.style.display = "none";
    }
  }

  // Initialize dashboard
  async init() {
    // Check authentication
    if (!auth.isAuthenticated()) {
      window.location.href = "../index.html";
      return;
    }

    await this.loadUserInfo();

    // Start token refresh timer
    auth.startTokenRefreshTimer();

    // Update date/time every second
    this.updateDateTime();
    setInterval(() => this.updateDateTime(), 1000);

    // Load dashboard data
    await this.loadStatistics();
    await this.loadRecentOrders();

    // Setup navigation and actions
    this.setupNavigation();
    this.setupLogout();
    this.setupProfileForm();

    // Load admin data if applicable
    if (window.location.pathname.includes("admin.html")) {
      await this.loadAllUsers();
      await this.loadAllProducts();
      await this.loadAllOrders();
    }
  }
}

// Global functions for onclick handlers
window.viewOrder = (orderId) => {
  window.location.href = `order-details.html?id=${orderId}`;
};

window.editUser = (userId) => {
  window.location.href = `edit-user.html?id=${userId}`;
};

window.editProduct = (productId) => {
  window.location.href = `edit-product.html?id=${productId}`;
};

// Initialize dashboard when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new Dashboard();
});
