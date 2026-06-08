// js/components/dashboard.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { db } from "../services/indexedDB.js";
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

        await this.updateCartCount();
        await this.updateWishlistCount();
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

  // Load dashboard statistics from IndexedDB and server
  async loadStatistics() {
    const isAdmin = window.location.pathname.includes("admin.html");

    try {
      if (isAdmin) {
        const cachedProducts = await db.getAll("products");
        const totalProducts = cachedProducts.length;

        const ordersResponse = await api.request(
          "/orders/statistics",
          "GET",
          null,
          true,
        );

        document.getElementById("totalProducts").textContent =
          totalProducts || 0;

        if (ordersResponse.success) {
          document.getElementById("totalOrders").textContent =
            ordersResponse.data.totalOrders || 0;
          document.getElementById("totalRevenue").textContent =
            `$${(ordersResponse.data.totalRevenue || 0).toFixed(2)}`;
        }

        const usersResponse = await api.request(
          "/users/statistics",
          "GET",
          null,
          true,
        );
        if (usersResponse.success) {
          document.getElementById("totalUsers").textContent =
            usersResponse.data;
        }
      } else {
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
          `/orders/user/${this.user?.id}`,
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

  // Load wishlist items from IndexedDB
  async loadWishlistItems() {
    const container = document.getElementById("wishlistItems");
    if (!container) return;

    try {
      const userId = this.user?.id;
      if (!userId) {
        container.innerHTML =
          '<div class="empty-state"><i class="fas fa-heart-broken"></i><h3>Please login to view wishlist</h3></div>';
        return;
      }

      const wishlist = await db.getAll("wishlist");
      const userWishlist = wishlist.filter((item) => item.userId === userId);

      // Get full product details for wishlist items
      const allProducts = await db.getAll("products");
      const wishlistProducts = [];

      for (const wishlistItem of userWishlist) {
        const product = allProducts.find(
          (p) => p.id === wishlistItem.productId,
        );
        if (product) {
          wishlistProducts.push({
            ...product,
            wishlistId: wishlistItem.id,
            addedAt: wishlistItem.addedAt,
          });
        }
      }

      if (wishlistProducts.length === 0) {
        container.innerHTML = `
          <div class="empty-state">
            <i class="fas fa-heart"></i>
            <h3>Your wishlist is empty</h3>
            <p>Start adding products you love to your wishlist!</p>
            <button class="register-btn" onclick="window.location.href='../shop.html'">Start Shopping</button>
          </div>
        `;
        return;
      }

      let wishlistHtml = `
        <div class="wishlist-grid">
      `;

      for (const product of wishlistProducts) {
        wishlistHtml += `
          <div class="wishlist-card" data-product-id="${product.id}">
            <div class="wishlist-image">
              <img src="${product.images?.[0]?.url || product.mainImageUrl || "https://via.placeholder.com/300"}" alt="${product.name}">
              <button class="remove-wishlist-btn" data-id="${product.wishlistId}" data-product-id="${product.id}">
                <i class="fas fa-trash-alt"></i>
              </button>
            </div>
            <div class="wishlist-info">
              <h3>${product.name}</h3>
              <p class="product-description">${product.description?.substring(0, 100) || ""}${product.description?.length > 100 ? "..." : ""}</p>
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
              <div class="wishlist-actions">
                <button class="add-to-cart-btn" data-id="${product.id}" ${product.quantity === 0 ? "disabled" : ""}>
                  <i class="fas fa-shopping-cart"></i> Add to Cart
                </button>
                <button class="view-product-btn" data-id="${product.id}">
                  <i class="fas fa-eye"></i> View Details
                </button>
              </div>
              <div class="added-date">
                <small>Added on: ${new Date(product.addedAt).toLocaleDateString()}</small>
              </div>
            </div>
          </div>
        `;
      }

      wishlistHtml += `</div>`;
      container.innerHTML = wishlistHtml;

      // Setup event listeners for wishlist buttons
      this.setupWishlistEvents();
    } catch (error) {
      console.error("Failed to load wishlist:", error);
      container.innerHTML =
        '<div class="empty-state"><i class="fas fa-exclamation-circle"></i><h3>Failed to load wishlist</h3><p>Please try again later</p></div>';
    }
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

  // Setup wishlist event listeners
  setupWishlistEvents() {
    // Remove from wishlist buttons
    document.querySelectorAll(".remove-wishlist-btn").forEach((btn) => {
      btn.removeEventListener("click", this.handleRemoveFromWishlist);
      this.handleRemoveFromWishlist = async (e) => {
        e.stopPropagation();
        const wishlistId = parseInt(btn.dataset.id);
        const productId = parseInt(btn.dataset.productId);
        await this.removeFromWishlist(wishlistId, productId);
      };
      btn.addEventListener("click", this.handleRemoveFromWishlist);
    });

    // Add to cart buttons
    document.querySelectorAll(".add-to-cart-btn").forEach((btn) => {
      btn.removeEventListener("click", this.handleAddToCart);
      this.handleAddToCart = async (e) => {
        e.stopPropagation();
        const productId = parseInt(btn.dataset.id);
        await this.addToCartFromWishlist(productId);
      };
      btn.addEventListener("click", this.handleAddToCart);
    });

    // View product buttons
    document.querySelectorAll(".view-product-btn").forEach((btn) => {
      btn.removeEventListener("click", this.handleViewProduct);
      this.handleViewProduct = (e) => {
        e.stopPropagation();
        const productId = btn.dataset.id;
        window.location.href = `../product-details.html?id=${productId}`;
      };
      btn.addEventListener("click", this.handleViewProduct);
    });
  }

  // Remove from wishlist
  async removeFromWishlist(wishlistId, productId) {
    try {
      await db.delete("wishlist", wishlistId);
      this.showToast("Removed from wishlist", "success");
      await this.updateWishlistCount();
      await this.loadWishlistItems();
    } catch (error) {
      console.error("Failed to remove from wishlist:", error);
      this.showToast(error.message, "error");
    }
  }

  // Add to cart from wishlist
  async addToCartFromWishlist(productId) {
    if (!this.user) {
      this.showToast("Please login to add items to cart", "error");
      setTimeout(() => {
        window.location.href = "../index.html";
      }, 1500);
      return;
    }

    try {
      const userId = this.user.id;
      const allProducts = await db.getAll("products");
      const product = allProducts.find((p) => p.id === productId);

      if (!product) {
        this.showToast("Product not found", "error");
        return;
      }

      const cart = await db.getAll("cart");
      const existingItem = cart.find(
        (item) => item.productId === productId && item.userId === userId,
      );

      if (existingItem) {
        existingItem.quantity += 1;
        await db.put("cart", existingItem);
      } else {
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

  // Load all products from IndexedDB (admin only)
  async loadAllProducts() {
    const tableContainer = document.getElementById("allProductsTable");
    if (!tableContainer) return;

    try {
      let products = await db.getAll("products");

      if (products.length === 0) {
        const response = await api.request("/products", "GET");
        if (response.success) {
          products = response.data;
          for (const product of products) {
            await db.put("products", {
              ...product,
              inStock: product.quantity > 0,
              cachedAt: new Date().toISOString(),
            });
          }
        }
      }

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
            <td>${product.categoryName || "N/A"}</td>
            <td><button class="quick-action-btn" onclick="editProduct(${product.id})" style="padding: 4px 12px;">Edit</button></td>
          </tr>
        `;
      }

      tableHtml += `</tbody></table>`;
      tableContainer.innerHTML = tableHtml;
    } catch (error) {
      console.error("Failed to load products:", error);
      tableContainer.innerHTML =
        '<div class="empty-state"><i class="fas fa-exclamation-circle"></i><h3>Failed to load products</h3></div>';
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

  // Update cart count from IndexedDB
  async updateCartCount() {
    if (!this.user) return;

    const userId = this.user.id;
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
  }

  // Update wishlist count from IndexedDB
  async updateWishlistCount() {
    if (!this.user) return;

    const userId = this.user.id;
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
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

        navItems.forEach((nav) => nav.classList.remove("active"));
        item.classList.add("active");

        pages.forEach((p) => (p.style.display = "none"));
        const activePage = document.getElementById(`${page}Page`);
        if (activePage) activePage.style.display = "block";

        const pageTitle = document.getElementById("pageTitle");
        if (pageTitle)
          pageTitle.textContent = item.querySelector("span").textContent;

        this.currentPage = page;
        this.loadPageData(page);
      });
    });

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
      case "wishlist":
        this.loadWishlistItems();
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

        this.updateRequirement(requirements.length, hasMinLength);
        this.updateRequirement(requirements.lowercase, hasLowercase);
        this.updateRequirement(requirements.uppercase, hasUppercase);
        this.updateRequirement(requirements.number, hasNumber);

        let strength = 0;
        if (hasMinLength) strength++;
        if (hasLowercase) strength++;
        if (hasUppercase) strength++;
        if (hasNumber) strength++;

        if (password.length > 0) {
          strengthContainer.style.display = "block";
        } else {
          strengthContainer.style.display = "none";
        }

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

      if (newPasswordInput) {
        newPasswordInput.addEventListener("input", (e) => {
          checkPasswordStrength(e.target.value);
        });
      }

      document.querySelectorAll(".toggle-password").forEach((button) => {
        button.addEventListener("click", () => {
          const targetId = button.dataset.target;
          const input = document.getElementById(targetId);
          if (input) {
            const type =
              input.getAttribute("type") === "password" ? "text" : "password";
            input.setAttribute("type", type);
            const icon = button.querySelector("i");
            icon.classList.toggle("fa-eye");
            icon.classList.toggle("fa-eye-slash");
          }
        });
      });

      passwordForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const button = passwordForm.querySelector('button[type="submit"]');
        this.setButtonLoading(button, true);

        const currentPassword =
          document.getElementById("currentPassword")?.value || "";
        const newPassword = document.getElementById("newPassword")?.value || "";
        const confirmPassword =
          document.getElementById("confirmNewPassword")?.value || "";

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
            { currentPassword, newPassword },
            true,
          );

          if (response.success) {
            this.showToast("Password changed successfully!", "success");
            passwordForm.reset();
            if (strengthContainer) strengthContainer.style.display = "none";
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
    if (!element) return;
    if (isValid) {
      element.classList.add("valid");
      const icon = element.querySelector("i");
      if (icon) icon.className = "fas fa-check-circle";
    } else {
      element.classList.remove("valid");
      const icon = element.querySelector("i");
      if (icon) icon.className = "fas fa-circle";
    }
  }

  setButtonLoading(button, isLoading) {
    if (!button) return;
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
    if (!auth.isAuthenticated()) {
      window.location.href = "../index.html";
      return;
    }

    await this.loadUserInfo();
    auth.startTokenRefreshTimer();

    this.updateDateTime();
    setInterval(() => this.updateDateTime(), 1000);

    await this.loadStatistics();
    await this.loadRecentOrders();

    this.setupNavigation();
    this.setupLogout();
    this.setupProfileForm();

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
