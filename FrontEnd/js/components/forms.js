// js/components/forms.js

import { FormValidator, ValidationRules } from "./validation.js";
import { auth } from "../services/auth.js";
import { urlHelpers } from "../utils/helpers.js";

class LoginForm {
  constructor() {
    this.form = document.getElementById("loginForm");
    this.loginBtn = document.getElementById("loginBtn");
    this.emailInput = document.getElementById("email");
    this.passwordInput = document.getElementById("password");
    this.rememberMeCheckbox = document.getElementById("rememberMe");
    this.demoButtons = document.querySelectorAll(".demo-btn");
    this.toast = document.getElementById("toast");
    this.init();
  }

  // Show toast message
  showToast(message, type = "success") {
    const toast = this.toast;
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

  // Set loading state
  setLoading(isLoading) {
    const btnText = this.loginBtn.querySelector(".btn-text");
    const btnLoader = this.loginBtn.querySelector(".btn-loader");

    if (isLoading) {
      this.loginBtn.disabled = true;
      btnText.style.display = "none";
      btnLoader.style.display = "inline-block";
    } else {
      this.loginBtn.disabled = false;
      btnText.style.display = "inline-block";
      btnLoader.style.display = "none";
    }
  }

  // Handle login
  async handleLogin(email, password) {
    this.setLoading(true);

    try {
      const result = await auth.login(
        email,
        password,
        this.rememberMeCheckbox?.checked,
      );

      if (result.success) {
        this.showToast("Login successful! Redirecting...", "success");
        setTimeout(() => {
          urlHelpers.redirectToDashboard(result.user.role);
        }, 1500);
      } else {
        this.showToast(
          result.message || "Login failed. Please try again.",
          "error",
        );
        this.setLoading(false);
      }
    } catch (error) {
      this.showToast(
        error.message || "An error occurred. Please try again.",
        "error",
      );
      this.setLoading(false);
    }
  }

  // Set demo credentials
  setDemoCredentials(email, password) {
    if (this.emailInput) this.emailInput.value = email;
    if (this.passwordInput) this.passwordInput.value = password;

    // Highlight the clicked button
    this.demoButtons.forEach((btn) => {
      btn.style.background = "";
      btn.style.color = "";
    });

    const clickedBtn = Array.from(this.demoButtons).find(
      (btn) => btn.dataset.email === email,
    );
    if (clickedBtn) {
      clickedBtn.style.background = "#667eea";
      clickedBtn.style.color = "white";
    }
  }

  // Toggle password visibility
  togglePassword() {
    const type =
      this.passwordInput.getAttribute("type") === "password"
        ? "text"
        : "password";
    this.passwordInput.setAttribute("type", type);

    const icon = document.querySelector(".toggle-password i");
    icon.classList.toggle("fa-eye");
    icon.classList.toggle("fa-eye-slash");
  }

  // Initialize form validation
  initValidation() {
    const validator = new FormValidator("loginForm");

    validator.addField(
      "email",
      [ValidationRules.required("Email is required"), ValidationRules.email()],
      "emailError",
    );

    validator.addField(
      "password",
      [
        ValidationRules.required("Password is required"),
        ValidationRules.minLength(6, "Password must be at least 6 characters"),
      ],
      "passwordError",
    );

    return validator;
  }

  // Check if already logged in
  async checkExistingSession() {
    if (auth.isAuthenticated()) {
      const user = await auth.getCurrentUser();
      if (user) {
        this.showToast(
          `Welcome back, ${user.firstName}! Redirecting...`,
          "success",
        );
        setTimeout(() => {
          urlHelpers.redirectToDashboard(user.role);
        }, 1500);
      }
    }
  }

  // Initialize
  init() {
    if (!this.form) return;

    // Initialize validator
    const validator = this.initValidation();

    // Form submit handler
    this.form.addEventListener("submit", async (e) => {
      e.preventDefault();

      if (validator.validateAll()) {
        await this.handleLogin(this.emailInput.value, this.passwordInput.value);
      }
    });

    // Password toggle
    const toggleBtn = document.querySelector(".toggle-password");
    if (toggleBtn) {
      toggleBtn.addEventListener("click", () => this.togglePassword());
    }

    // Demo buttons
    this.demoButtons.forEach((btn) => {
      btn.addEventListener("click", () => {
        const email = btn.dataset.email;
        const password = btn.dataset.password;
        this.setDemoCredentials(email, password);

        // Auto-fill and validate
        if (this.emailInput) {
          validator.validateField("email");
        }
        if (this.passwordInput) {
          validator.validateField("password");
        }
      });
    });

    // Enter key support
    this.passwordInput?.addEventListener("keypress", (e) => {
      if (e.key === "Enter") {
        this.form.dispatchEvent(new Event("submit"));
      }
    });

    // Check existing session
    this.checkExistingSession();
  }
}

// Initialize login form when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new LoginForm();
});
