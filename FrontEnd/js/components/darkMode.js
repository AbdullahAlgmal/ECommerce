// js/components/darkMode.js

import { storage } from "../utils/helpers.js";
import { STORAGE_KEYS, APP_CONFIG } from "../utils/constants.js";

class DarkMode {
  constructor() {
    this.themeToggle = document.getElementById("themeToggle");
    this.themeIcon = document.getElementById("themeIcon");
    this.init();
  }

  // Get current theme
  getCurrentTheme() {
    return (
      document.documentElement.getAttribute("data-theme") ||
      storage.get(STORAGE_KEYS.THEME) ||
      APP_CONFIG.DEFAULT_THEME
    );
  }

  // Set theme on HTML element
  setTheme(theme) {
    if (theme === "dark") {
      document.documentElement.setAttribute("data-theme", "dark");
    } else {
      document.documentElement.removeAttribute("data-theme");
    }

    storage.set(STORAGE_KEYS.THEME, theme);
    this.updateIcon(theme);

    // Dispatch custom event for other components
    window.dispatchEvent(
      new CustomEvent("themeChanged", { detail: { theme } }),
    );

    console.log(`Theme changed to: ${theme}`);
  }

  // Toggle theme
  toggleTheme() {
    const currentTheme = this.getCurrentTheme();
    const newTheme = currentTheme === "light" ? "dark" : "light";
    this.setTheme(newTheme);
  }

  // Update icon based on theme
  updateIcon(theme) {
    if (this.themeIcon) {
      this.themeIcon.className =
        theme === "light" ? "fas fa-moon" : "fas fa-sun";
    }
  }

  // Load saved theme from storage
  loadSavedTheme() {
    const savedTheme = storage.get(STORAGE_KEYS.THEME);

    if (savedTheme) {
      this.setTheme(savedTheme);
    } else {
      // Check system preference
      const systemPrefersDark = window.matchMedia(
        "(prefers-color-scheme: dark)",
      ).matches;
      if (systemPrefersDark) {
        this.setTheme("dark");
      } else {
        this.setTheme(APP_CONFIG.DEFAULT_THEME);
      }
    }
  }

  // Listen for system theme changes
  listenForSystemThemeChanges() {
    window
      .matchMedia("(prefers-color-scheme: dark)")
      .addEventListener("change", (e) => {
        // Only apply if user hasn't manually set a preference
        if (!storage.get(STORAGE_KEYS.THEME)) {
          this.setTheme(e.matches ? "dark" : "light");
        }
      });
  }

  // Apply theme to all iframes (optional)
  applyThemeToIframes(theme) {
    const iframes = document.querySelectorAll("iframe");
    iframes.forEach((iframe) => {
      try {
        iframe.contentWindow.postMessage({ type: "themeChange", theme }, "*");
      } catch (e) {
        console.warn("Could not apply theme to iframe:", e);
      }
    });
  }

  // Add theme transition class to prevent flash
  addTransitionClass() {
    // Add class to body to enable smooth transitions
    document.body.classList.add("theme-transition");

    // Remove class after transition completes
    setTimeout(() => {
      document.body.classList.remove("theme-transition");
    }, 300);
  }

  // Listen for theme change events from other windows/tabs
  listenForCrossTabThemeChanges() {
    window.addEventListener("storage", (e) => {
      if (e.key === STORAGE_KEYS.THEME && e.newValue) {
        this.setTheme(e.newValue);
      }
    });
  }

  // Initialize dark mode
  init() {
    this.loadSavedTheme();
    this.listenForSystemThemeChanges();
    this.listenForCrossTabThemeChanges();

    if (this.themeToggle) {
      this.themeToggle.addEventListener("click", (e) => {
        e.preventDefault();
        this.addTransitionClass();
        this.toggleTheme();
      });
    }

    // Add CSS for transition
    const style = document.createElement("style");
    style.textContent = `
            .theme-transition * {
                transition: background-color 0.2s ease, color 0.2s ease, border-color 0.2s ease !important;
            }
        `;
    document.head.appendChild(style);
  }
}

// Initialize dark mode when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  new DarkMode();
});

export default DarkMode;
