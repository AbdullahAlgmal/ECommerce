// js/main.js

import { auth } from "./services/auth.js";
import { storage } from "./utils/helpers.js";
import { STORAGE_KEYS } from "./utils/constants.js";

// Import components (they auto-initialize)
import "./components/darkMode.js";
import "./components/forms.js";

// Global app initialization
class App {
  constructor() {
    this.init();
  }

  // Set up global error handling
  setupErrorHandling() {
    window.addEventListener("unhandledrejection", (event) => {
      console.error("Unhandled promise rejection:", event.reason);
      this.showGlobalError(
        event.reason?.message || "An unexpected error occurred",
      );
    });

    window.addEventListener("error", (event) => {
      console.error("Global error:", event.error);
      this.showGlobalError(
        event.error?.message || "An unexpected error occurred",
      );
    });
  }

  // Show global error
  showGlobalError(message) {
    const toast = document.getElementById("toast");
    if (toast) {
      const toastContent = toast.querySelector(".toast-content");
      const icon = toastContent.querySelector("i");
      const messageSpan = toastContent.querySelector("#toastMessage");

      icon.className = "fas fa-exclamation-circle";
      messageSpan.textContent = message;

      toast.classList.remove("success");
      toast.classList.add("error");
      toast.classList.add("show");

      setTimeout(() => {
        toast.classList.remove("show");
      }, 5000);
    }
  }

  // Set page title
  setPageTitle() {
    const title = document.querySelector("title");
    if (title && title.textContent === "Login | E-Commerce Platform") {
      // Title already set
    }
  }

  // Add CSS transitions for smooth theme switching
  addTransitionStyles() {
    const style = document.createElement("style");
    style.textContent = `
            * {
                transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease, box-shadow 0.3s ease;
            }
        `;
    document.head.appendChild(style);
  }

  // Preload theme
  preloadTheme() {
    const savedTheme = storage.get(STORAGE_KEYS.THEME);
    if (savedTheme) {
      document.documentElement.setAttribute("data-theme", savedTheme);
    }
  }

  // Initialize app
  init() {
    this.preloadTheme();
    this.setPageTitle();
    this.addTransitionStyles();
    this.setupErrorHandling();

    console.log("App initialized successfully");
  }
}

// Initialize app when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new App();
});