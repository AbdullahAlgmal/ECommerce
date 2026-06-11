// js/components/faq.js

import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

class FAQPage {
  constructor() {
    this.currentCategory = "all";
    this.searchTerm = "";
    this.faqs = [
      // Ordering Questions
      {
        id: 1,
        question: "How do I place an order?",
        answer:
          "To place an order, simply browse our products, add items to your cart, and proceed to checkout. You'll need to provide your shipping information and payment details to complete the purchase.",
        category: "ordering",
      },
      {
        id: 2,
        question: "Can I modify or cancel my order after placing it?",
        answer:
          "You can modify or cancel your order within 1 hour of placing it. Please contact our customer support team immediately for assistance. Once an order is processed for shipping, modifications may not be possible.",
        category: "ordering",
      },
      {
        id: 3,
        question: "How do I track my order?",
        answer:
          "Once your order ships, you'll receive a confirmation email with a tracking number. You can also track your order by logging into your account and visiting the 'My Orders' section.",
        category: "ordering",
      },
      {
        id: 4,
        question: "What payment methods do you accept?",
        answer:
          "We accept all major credit cards (Visa, MasterCard, American Express), PayPal, Apple Pay, Google Pay, and bank transfers. All payments are processed securely.",
        category: "payments",
      },
      {
        id: 5,
        question: "Is it safe to use my credit card on your website?",
        answer:
          "Yes! We use industry-standard SSL encryption to protect your personal and payment information. We never store your full credit card details on our servers.",
        category: "payments",
      },
      {
        id: 6,
        question: "Do you offer cash on delivery?",
        answer:
          "Yes, we offer cash on delivery for select locations. Please check availability during checkout.",
        category: "payments",
      },
      {
        id: 7,
        question: "How long does shipping take?",
        answer:
          "Standard shipping typically takes 3-7 business days. Express shipping takes 1-3 business days. Delivery times may vary based on your location.",
        category: "shipping",
      },
      {
        id: 8,
        question: "Do you offer international shipping?",
        answer:
          "Yes, we ship to over 50 countries worldwide. International shipping typically takes 7-14 business days. Shipping costs and delivery times vary by destination.",
        category: "shipping",
      },
      {
        id: 9,
        question: "How much does shipping cost?",
        answer:
          "Shipping costs are calculated based on your location and order weight. We offer free standard shipping on orders over $50 within the continental US.",
        category: "shipping",
      },
      {
        id: 10,
        question: "Can I track my international order?",
        answer:
          "Yes, all international orders come with tracking information. You'll receive a tracking number via email once your order ships.",
        category: "shipping",
      },
      {
        id: 11,
        question: "What is your return policy?",
        answer:
          "We accept returns within 30 days of delivery. Items must be unused, in original packaging, and with all tags attached. Some exclusions apply.",
        category: "returns",
      },
      {
        id: 12,
        question: "How do I initiate a return?",
        answer:
          "To initiate a return, log into your account, go to 'My Orders', select the order, and click 'Return Item'. Follow the instructions to print a return label.",
        category: "returns",
      },
      {
        id: 13,
        question: "How long does it take to process a refund?",
        answer:
          "Refunds are processed within 3-5 business days after we receive your returned item. The refund will be credited to your original payment method.",
        category: "returns",
      },
      {
        id: 14,
        question: "Do you offer exchanges?",
        answer:
          "Yes, we offer exchanges for defective or damaged items. Please contact our customer support team to process an exchange.",
        category: "returns",
      },
      {
        id: 15,
        question: "How do I create an account?",
        answer:
          "Click on the 'Register' button at the top of the page. Fill in your personal information, create a password, and you're ready to start shopping!",
        category: "account",
      },
      {
        id: 16,
        question: "I forgot my password. How do I reset it?",
        answer:
          "Click on 'Forgot Password' on the login page. Enter your email address, and we'll send you a link to reset your password.",
        category: "account",
      },
      {
        id: 17,
        question: "How do I update my account information?",
        answer:
          "Log into your account and go to 'Profile Settings'. You can update your name, email, phone number, and password there.",
        category: "account",
      },
      {
        id: 18,
        question: "Can I delete my account?",
        answer:
          "Yes, please contact our customer support team to request account deletion. Note that this action is permanent.",
        category: "account",
      },
      {
        id: 19,
        question: "Are your products authentic?",
        answer:
          "Yes, we source all products directly from manufacturers or authorized distributors. All products are 100% authentic and backed by manufacturer warranties.",
        category: "products",
      },
      {
        id: 20,
        question: "Do you offer product warranties?",
        answer:
          "Product warranties vary by manufacturer. Please check the product page for specific warranty information. Most electronics come with a 1-year warranty.",
        category: "products",
      },
      {
        id: 21,
        question: "How do I know if a product is in stock?",
        answer:
          "Product pages display real-time stock availability. If an item is out of stock, you can sign up for email notifications when it becomes available.",
        category: "products",
      },
      {
        id: 22,
        question: "Can I leave a product review?",
        answer:
          "Yes! After purchasing a product, you can leave a review by going to 'My Orders' and clicking 'Write a Review'. We appreciate your feedback!",
        category: "products",
      },
    ];

    this.filteredFaqs = [...this.faqs];
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

  // Render FAQ items
  renderFaqs() {
    const container = document.getElementById("faqGrid");
    const noResults = document.getElementById("noResults");

    if (this.filteredFaqs.length === 0) {
      container.style.display = "none";
      noResults.style.display = "block";
      return;
    }

    container.style.display = "flex";
    noResults.style.display = "none";

    container.innerHTML = this.filteredFaqs
      .map(
        (faq) => `
            <div class="faq-item" data-category="${faq.category}" data-id="${faq.id}">
                <div class="faq-question">
                    <h3>${this.highlightSearchTerm(faq.question)}</h3>
                    <i class="fas fa-chevron-down faq-icon"></i>
                </div>
                <div class="faq-answer">
                    <div class="answer-content">
                        ${this.highlightSearchTerm(faq.answer)}
                    </div>
                </div>
            </div>
        `,
      )
      .join("");

    this.setupFaqToggle();
  }

  // Highlight search term in text
  highlightSearchTerm(text) {
    if (!this.searchTerm) return text;

    const regex = new RegExp(`(${this.searchTerm})`, "gi");
    return text.replace(regex, "<mark>$1</mark>");
  }

  // Setup FAQ toggle functionality
  setupFaqToggle() {
    document.querySelectorAll(".faq-item").forEach((item) => {
      const question = item.querySelector(".faq-question");

      question.addEventListener("click", () => {
        const isActive = item.classList.contains("active");

        // Close all other FAQs
        document.querySelectorAll(".faq-item").forEach((faq) => {
          faq.classList.remove("active");
        });

        // Toggle current FAQ
        if (!isActive) {
          item.classList.add("active");
        }
      });
    });
  }

  // Filter FAQs by category and search term
  filterFaqs() {
    this.filteredFaqs = this.faqs.filter((faq) => {
      // Category filter
      if (
        this.currentCategory !== "all" &&
        faq.category !== this.currentCategory
      ) {
        return false;
      }

      // Search filter
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        return (
          faq.question.toLowerCase().includes(term) ||
          faq.answer.toLowerCase().includes(term)
        );
      }

      return true;
    });

    this.renderFaqs();
  }

  // Setup category tabs
  setupCategoryTabs() {
    const tabs = document.querySelectorAll(".category-tab");

    tabs.forEach((tab) => {
      tab.addEventListener("click", () => {
        // Update active tab
        tabs.forEach((t) => t.classList.remove("active"));
        tab.classList.add("active");

        // Update category filter
        this.currentCategory = tab.dataset.category;
        this.filterFaqs();
      });
    });
  }

  // Setup FAQ search
  setupFaqSearch() {
    const searchInput = document.getElementById("faqSearchInput");
    const clearBtn = document.getElementById("clearSearchBtn");

    if (searchInput) {
      let searchTimeout;
      searchInput.addEventListener("input", () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
          this.searchTerm = searchInput.value.trim();
          this.filterFaqs();
        }, 300);
      });
    }

    if (clearBtn) {
      clearBtn.addEventListener("click", () => {
        if (searchInput) {
          searchInput.value = "";
          this.searchTerm = "";
          this.filterFaqs();
        }
      });
    }
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

  // Setup live chat button
  setupLiveChat() {
    const liveChatBtn = document.getElementById("liveChatBtn");
    if (liveChatBtn) {
      liveChatBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.showToast("Live chat feature coming soon!", "success");
      });
    }
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

  // Initialize FAQ page
  async init() {
    this.renderFaqs();
    this.setupFaqToggle();
    this.setupCategoryTabs();
    this.setupFaqSearch();
    this.setupBackToTop();
    this.setupSearch();
    this.setupUserDropdown();
    this.setupLiveChat();

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      await this.updateWishlistCount();
      auth.startTokenRefreshTimer();
    }
  }
}

// Initialize FAQ page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new FAQPage();
});
