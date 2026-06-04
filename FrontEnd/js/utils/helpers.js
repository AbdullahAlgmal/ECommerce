// js/utils/helpers.js

import { STORAGE_KEYS } from "./constants.js";

// Storage Helpers
export const storage = {
  set: (key, value, isSession = false) => {
    const storage = isSession ? sessionStorage : localStorage;
    storage.setItem(key, JSON.stringify(value));
  },

  get: (key, isSession = false) => {
    const storage = isSession ? sessionStorage : localStorage;
    const item = storage.getItem(key);
    return item ? JSON.parse(item) : null;
  },

  remove: (key, isSession = false) => {
    const storage = isSession ? sessionStorage : localStorage;
    storage.removeItem(key);
  },

  clear: (isSession = false) => {
    const storage = isSession ? sessionStorage : localStorage;
    storage.clear();
  },
};

// Date Helpers
export const dateHelpers = {
  format: (date, format = "YYYY-MM-DD HH:mm:ss") => {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    const hours = String(d.getHours()).padStart(2, "0");
    const minutes = String(d.getMinutes()).padStart(2, "0");
    const seconds = String(d.getSeconds()).padStart(2, "0");

    return format
      .replace("YYYY", year)
      .replace("MM", month)
      .replace("DD", day)
      .replace("HH", hours)
      .replace("mm", minutes)
      .replace("ss", seconds);
  },

  isExpired: (expiryDate) => {
    return new Date(expiryDate) < new Date();
  },
};

// String Helpers
export const stringHelpers = {
  capitalize: (str) => {
    return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
  },

  truncate: (str, length = 50) => {
    if (str.length <= length) return str;
    return str.substring(0, length) + "...";
  },

  slugify: (str) => {
    return str
      .toLowerCase()
      .replace(/[^\w\s]/g, "")
      .replace(/\s+/g, "-");
  },
};

// DOM Helpers
export const domHelpers = {
  showElement: (element) => {
    element.classList.remove("hidden");
  },

  hideElement: (element) => {
    element.classList.add("hidden");
  },

  toggleElement: (element) => {
    element.classList.toggle("hidden");
  },

  addClass: (element, className) => {
    element.classList.add(className);
  },

  removeClass: (element, className) => {
    element.classList.remove(className);
  },

  setLoading: (button, isLoading, loadingText = "Loading...") => {
    if (isLoading) {
      button.disabled = true;
      button.dataset.originalText = button.innerHTML;
      button.innerHTML = `<i class="fas fa-spinner fa-spin"></i> ${loadingText}`;
    } else {
      button.disabled = false;
      button.innerHTML = button.dataset.originalText;
    }
  },
};

// Validation Helpers
export const validationHelpers = {
  isEmail: (email) => {
    const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return pattern.test(email);
  },

  isPhone: (phone) => {
    const pattern =
      /^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,5}[-\s\.]?[0-9]{1,5}$/;
    return pattern.test(phone);
  },

  isStrongPassword: (password) => {
    const hasMinLength = password.length >= 6;
    const hasLowercase = /[a-z]/.test(password);
    const hasUppercase = /[A-Z]/.test(password);
    const hasNumber = /[0-9]/.test(password);

    return {
      isValid: hasMinLength && hasLowercase && hasUppercase && hasNumber,
      checks: { hasMinLength, hasLowercase, hasUppercase, hasNumber },
    };
  },
};

// URL Helpers
export const urlHelpers = {
  getQueryParam: (param) => {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
  },

  redirect: (url) => {
    window.location.href = url;
  },

  redirectToDashboard: (role) => {
    const dashboards = {
      admin: "/dashboard/admin.html",
      customer: "/dashboard/customer.html",
      seller: "/dashboard/seller.html",
    };
    window.location.href =
      dashboards[role.toLowerCase()] || "/dashboard/customer.html";
  },
};
