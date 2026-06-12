// js/components/checkout.js

import { auth } from "../services/auth.js";
import { api } from "../services/api.js";
import { db } from "../services/indexedDB.js";

class CheckoutPage {
  constructor() {
    this.cartItems = [];
    this.subtotal = 0;
    this.shippingCost = 5.99;
    this.taxRate = 0.1;
    this.discount = 0;
    this.promoApplied = false;
    this.selectedPaymentMethod = "card";
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
    try {
      const userId = auth.user?.id;
      if (!userId) {
        window.location.href = "cart.html";
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
        window.location.href = "cart.html";
        return;
      }

      this.calculateTotals();
      this.renderOrderSummary();
      this.updateSummaryTotals();

      // Pre-fill user info if available
      this.prefillUserInfo();
    } catch (error) {
      console.error("Failed to load cart:", error);
      this.showToast("Failed to load cart", "error");
    }
  }

  // Calculate totals
  calculateTotals() {
    this.subtotal = this.cartItems.reduce(
      (sum, item) => sum + item.productPrice * item.quantity,
      0,
    );

    // Free shipping over $50
    if (this.subtotal > 50) {
      this.shippingCost = 0;
    } else {
      // Get selected shipping method
      const selectedShipping = document.querySelector(
        'input[name="shipping"]:checked',
      );
      if (selectedShipping) {
        const shippingValue = selectedShipping.value;
        switch (shippingValue) {
          case "standard":
            this.shippingCost = 5.99;
            break;
          case "express":
            this.shippingCost = 12.99;
            break;
          case "overnight":
            this.shippingCost = 24.99;
            break;
          default:
            this.shippingCost = 5.99;
        }
      }
    }

    // Add COD fee if selected
    if (this.selectedPaymentMethod === "cod") {
      this.shippingCost += 2.99;
    }

    this.tax = (this.subtotal - this.discount) * this.taxRate;
    this.total = this.subtotal - this.discount + this.shippingCost + this.tax;
  }

  // Render order summary
  renderOrderSummary() {
    const container = document.getElementById("summaryItems");

    if (this.cartItems.length === 0) {
      container.innerHTML = `
                <div class="empty-summary">
                    <i class="fas fa-shopping-cart"></i>
                    <p>Your cart is empty</p>
                </div>
            `;
      return;
    }

    container.innerHTML = this.cartItems
      .map(
        (item) => `
            <div class="summary-item">
                <div class="summary-item-image">
                    <img src="${item.productImage || "https://via.placeholder.com/60"}" alt="${item.productName}">
                </div>
                <div class="summary-item-details">
                    <div class="summary-item-name">${item.productName}</div>
                    <div class="summary-item-price">$${item.productPrice.toFixed(2)}</div>
                </div>
                <div class="summary-item-quantity">x${item.quantity}</div>
                <div class="summary-item-total">$${(item.productPrice * item.quantity).toFixed(2)}</div>
            </div>
        `,
      )
      .join("");
  }

  // Update summary totals
  updateSummaryTotals() {
    document.getElementById("summarySubtotal").textContent =
      `$${this.subtotal.toFixed(2)}`;
    document.getElementById("summaryShipping").textContent =
      this.shippingCost === 0 ? "Free" : `$${this.shippingCost.toFixed(2)}`;
    document.getElementById("summaryTax").textContent =
      `$${this.tax.toFixed(2)}`;

    if (this.discount > 0) {
      document.querySelector(".discount-row").style.display = "flex";
      document.getElementById("summaryDiscount").textContent =
        `-$${this.discount.toFixed(2)}`;
    }

    document.getElementById("summaryTotal").textContent =
      `$${this.total.toFixed(2)}`;
  }

  // Pre-fill user information from API
  async prefillUserInfo() {
    try {
      // Get current user from API
      const response = await api.getCurrentUser();
      if (response.success && response.data) {
        const user = response.data;
        document.getElementById("email").value = user.email || "";
        document.getElementById("phone").value = user.phone || "";
        document.getElementById("firstName").value = user.firstName || "";
        document.getElementById("lastName").value = user.lastName || "";
      }

      // Get default address from API
      const addressesResponse = await api.request(
        "/addresses/user/default",
        "GET",
        null,
        true,
      );
      if (addressesResponse.success && addressesResponse.data) {
        const address = addressesResponse.data;
        document.getElementById("address").value =
          `${address.houseNumber} ${address.streetBlock}`;
        document.getElementById("city").value = address.city || "";
        document.getElementById("state").value = address.province || "";
        document.getElementById("zipCode").value = address.zipCode || "";
        document.getElementById("country").value = address.country || "";
      }
    } catch (error) {
      console.error("Failed to prefill user info:", error);
    }
  }

  // Setup shipping method listener
  setupShippingMethod() {
    const shippingOptions = document.querySelectorAll('input[name="shipping"]');
    shippingOptions.forEach((option) => {
      option.addEventListener("change", () => {
        this.calculateTotals();
        this.updateSummaryTotals();
      });
    });
  }

  // Setup payment method tabs
  setupPaymentMethods() {
    const tabs = document.querySelectorAll(".payment-tab");
    const forms = {
      card: document.getElementById("cardPayment"),
      paypal: document.getElementById("paypalPayment"),
      cod: document.getElementById("codPayment"),
    };

    tabs.forEach((tab) => {
      tab.addEventListener("click", () => {
        // Update active tab
        tabs.forEach((t) => t.classList.remove("active"));
        tab.classList.add("active");

        // Update visible form
        Object.values(forms).forEach((form) => {
          if (form) form.classList.remove("active");
        });

        const method = tab.dataset.method;
        this.selectedPaymentMethod = method;
        if (forms[method]) {
          forms[method].classList.add("active");
        }

        // Recalculate totals (for COD fee)
        this.calculateTotals();
        this.updateSummaryTotals();
      });
    });

    // PayPal button
    const paypalBtn = document.querySelector(".paypal-btn");
    if (paypalBtn) {
      paypalBtn.addEventListener("click", () => {
        this.showToast("PayPal integration coming soon!", "success");
      });
    }
  }

  // Setup promo code
  setupPromoCode() {
    const applyBtn = document.getElementById("applyPromoBtn");
    const promoInput = document.getElementById("promoCodeInput");
    const promoMessage = document.getElementById("promoMessage");

    applyBtn.addEventListener("click", () => {
      const code = promoInput.value.trim().toUpperCase();

      if (code === "SAVE10" && !this.promoApplied) {
        this.discount = this.subtotal * 0.1;
        this.promoApplied = true;
        this.calculateTotals();
        this.updateSummaryTotals();

        promoMessage.textContent = "Promo code applied! 10% discount";
        promoMessage.classList.add("success");
        promoMessage.classList.remove("error");
        applyBtn.disabled = true;
        promoInput.disabled = true;

        this.showToast("Promo code applied!", "success");
      } else if (this.promoApplied) {
        promoMessage.textContent = "Promo code already applied";
        promoMessage.classList.add("error");
        promoMessage.classList.remove("success");
      } else {
        promoMessage.textContent = "Invalid promo code";
        promoMessage.classList.add("error");
        promoMessage.classList.remove("success");
      }

      setTimeout(() => {
        promoMessage.textContent = "";
      }, 3000);
    });
  }

  // Validate form
  validateForm() {
    const requiredFields = [
      "email",
      "phone",
      "firstName",
      "lastName",
      "address",
      "city",
      "state",
      "zipCode",
      "country",
    ];

    // Check if we're using card payment and need card details
    const isCardPayment = this.selectedPaymentMethod === "card";
    if (isCardPayment) {
      requiredFields.push("cardNumber", "cardName", "expiryDate", "cvv");
    }

    for (const field of requiredFields) {
      const input = document.getElementById(field);
      if (input && !input.value.trim()) {
        this.showToast(
          `Please fill in ${input.previousElementSibling?.textContent || field}`,
          "error",
        );
        input.focus();
        return false;
      }
    }

    // Email validation
    const email = document.getElementById("email").value;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      this.showToast("Please enter a valid email address", "error");
      return false;
    }

    // Phone validation
    const phone = document.getElementById("phone").value;
    if (phone.length < 10) {
      this.showToast("Please enter a valid phone number", "error");
      return false;
    }

    // Card validation (if using card payment)
    if (isCardPayment) {
      const cardNumber = document
        .getElementById("cardNumber")
        .value.replace(/\s/g, "");
      if (cardNumber.length < 13 || cardNumber.length > 19) {
        this.showToast("Please enter a valid card number", "error");
        return false;
      }

      const expiryDate = document.getElementById("expiryDate").value;
      if (!/^(0[1-9]|1[0-2])\/([0-9]{2})$/.test(expiryDate)) {
        this.showToast("Please enter a valid expiry date (MM/YY)", "error");
        return false;
      }

      const cvv = document.getElementById("cvv").value;
      if (cvv.length < 3 || cvv.length > 4) {
        this.showToast("Please enter a valid CVV", "error");
        return false;
      }
    }

    return true;
  }

  // Format card number
  formatCardNumber() {
    const cardNumber = document.getElementById("cardNumber");
    if (cardNumber) {
      cardNumber.addEventListener("input", (e) => {
        let value = e.target.value.replace(/\s/g, "");
        if (value.length > 16) value = value.slice(0, 16);
        value = value.replace(/(\d{4})/g, "$1 ").trim();
        e.target.value = value;
      });
    }
  }

  // Format expiry date
  formatExpiryDate() {
    const expiryDate = document.getElementById("expiryDate");
    if (expiryDate) {
      expiryDate.addEventListener("input", (e) => {
        let value = e.target.value.replace(/\D/g, "");
        if (value.length >= 2) {
          value = value.slice(0, 2) + "/" + value.slice(2, 4);
        }
        e.target.value = value;
      });
    }
  }

  // Format CVV
  formatCVV() {
    const cvv = document.getElementById("cvv");
    if (cvv) {
      cvv.addEventListener("input", (e) => {
        e.target.value = e.target.value.replace(/\D/g, "").slice(0, 4);
      });
    }
  }

  // Create order via API
  async createOrder() {
    // Prepare order data for API
    const orderData = {
      userId: auth.user.id,
      items: this.cartItems.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
        price: item.productPrice,
      })),
      subtotal: this.subtotal,
      shippingCost: this.shippingCost,
      tax: this.tax,
      discount: this.discount,
      total: this.total,
      paymentMethod: this.selectedPaymentMethod,
      shippingAddress: {
        firstName: document.getElementById("firstName").value,
        lastName: document.getElementById("lastName").value,
        address: document.getElementById("address").value,
        apartment: document.getElementById("apartment").value,
        city: document.getElementById("city").value,
        state: document.getElementById("state").value,
        zipCode: document.getElementById("zipCode").value,
        country: document.getElementById("country").value,
      },
      contactInfo: {
        email: document.getElementById("email").value,
        phone: document.getElementById("phone").value,
      },
      orderNotes: document.getElementById("orderNotes").value,
      paymentDetails:
        this.selectedPaymentMethod === "card"
          ? {
              cardLastFour: document
                .getElementById("cardNumber")
                .value.slice(-4),
              cardType: this.getCardType(
                document.getElementById("cardNumber").value,
              ),
            }
          : null,
    };

    try {
      // Submit order to API
      const response = await api.request("/orders", "POST", orderData, true);

      if (!response.success) {
        throw new Error(response.message || "Failed to create order");
      }

      const newOrderId = response.data.id;

      // Clear cart from IndexedDB (local cache)
      for (const item of this.cartItems) {
        await db.delete("cart", item.cartId);
      }

      // Store order ID for confirmation page
      sessionStorage.setItem("lastOrderId", newOrderId);
      sessionStorage.setItem("orderTotal", this.total.toFixed(2));

      // Redirect to order confirmation
      window.location.href = `order-confirmation.html?id=${newOrderId}`;
    } catch (error) {
      console.error("API Error:", error);
      throw error;
    }
  }

  // Helper function to detect card type
  getCardType(cardNumber) {
    const cleaned = cardNumber.replace(/\s/g, "");
    if (/^4/.test(cleaned)) return "Visa";
    if (/^5[1-5]/.test(cleaned)) return "Mastercard";
    if (/^3[47]/.test(cleaned)) return "Amex";
    return "Unknown";
  }

  // Handle form submission
  async handleSubmit(e) {
    e.preventDefault();

    if (!this.validateForm()) {
      return;
    }

    const placeOrderBtn = document.getElementById("placeOrderBtn");
    const btnText = placeOrderBtn.querySelector(".btn-text");
    const btnLoader = placeOrderBtn.querySelector(".btn-loader");

    // Show loading state
    placeOrderBtn.disabled = true;
    btnText.style.display = "none";
    btnLoader.style.display = "inline-block";

    try {
      await this.createOrder();
    } catch (error) {
      console.error("Order failed:", error);
      this.showToast(
        error.message || "Failed to place order. Please try again.",
        "error",
      );

      // Reset button
      placeOrderBtn.disabled = false;
      btnText.style.display = "inline-block";
      btnLoader.style.display = "none";
    }
  }

  // Initialize checkout
  async init() {
    // Check authentication
    if (!auth.isAuthenticated()) {
      this.showToast("Please login to checkout", "error");
      setTimeout(() => {
        window.location.href = "index.html";
      }, 1500);
      return;
    }

    await auth.getCurrentUser();
    await this.loadCartItems();

    this.setupShippingMethod();
    this.setupPaymentMethods();
    this.setupPromoCode();
    this.formatCardNumber();
    this.formatExpiryDate();
    this.formatCVV();

    const form = document.getElementById("checkoutForm");
    form.addEventListener("submit", (e) => this.handleSubmit(e));

    // Start token refresh timer
    auth.startTokenRefreshTimer();
  }
}

// Initialize checkout when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new CheckoutPage();
});
