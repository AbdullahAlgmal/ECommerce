// js/components/register.js

import { api } from "../services/api.js";
import { auth } from "../services/auth.js";
import { urlHelpers, validationHelpers } from "../utils/helpers.js";

class RegisterForm {
  constructor() {
    this.form = document.getElementById("registerForm");
    this.registerBtn = document.getElementById("registerBtn");
    this.toast = document.getElementById("toast");

    // Form fields
    this.firstNameInput = document.getElementById("firstName");
    this.lastNameInput = document.getElementById("lastName");
    this.emailInput = document.getElementById("email");
    this.phoneInput = document.getElementById("phone");
    this.passwordInput = document.getElementById("password");
    this.confirmPasswordInput = document.getElementById("confirmPassword");
    this.dateOfBirthInput = document.getElementById("dateOfBirth");
    this.roleSelect = document.getElementById("role");
    this.termsCheckbox = document.getElementById("termsCheckbox");

    // Password strength elements
    this.strengthBar = document.getElementById("strengthBar");
    this.passwordRequirements = {
      length: document.getElementById("reqLength"),
      lowercase: document.getElementById("reqLowercase"),
      uppercase: document.getElementById("reqUppercase"),
      number: document.getElementById("reqNumber"),
    };

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
    const btnText = this.registerBtn.querySelector(".btn-text");
    const btnLoader = this.registerBtn.querySelector(".btn-loader");

    if (isLoading) {
      this.registerBtn.disabled = true;
      btnText.style.display = "none";
      btnLoader.style.display = "inline-block";
    } else {
      this.registerBtn.disabled = false;
      btnText.style.display = "inline-block";
      btnLoader.style.display = "none";
    }
  }

  // Show field error
  showFieldError(field, message) {
    const errorSpan = document.getElementById(`${field}Error`);
    const input = document.getElementById(field);

    if (errorSpan) errorSpan.textContent = message;
    if (input) input.classList.add("error");
  }

  // Clear field error
  clearFieldError(field) {
    const errorSpan = document.getElementById(`${field}Error`);
    const input = document.getElementById(field);

    if (errorSpan) errorSpan.textContent = "";
    if (input) input.classList.remove("error");
  }

  // Clear all errors
  clearAllErrors() {
    const fields = [
      "firstName",
      "lastName",
      "email",
      "phone",
      "password",
      "confirmPassword",
      "dateOfBirth",
      "role",
      "terms",
    ];
    fields.forEach((field) => this.clearFieldError(field));
  }

  // Check password strength
  checkPasswordStrength(password) {
    const checks = validationHelpers.isStrongPassword(password).checks;

    // Update requirements UI
    this.updateRequirement(
      this.passwordRequirements.length,
      checks.hasMinLength,
    );
    this.updateRequirement(
      this.passwordRequirements.lowercase,
      checks.hasLowercase,
    );
    this.updateRequirement(
      this.passwordRequirements.uppercase,
      checks.hasUppercase,
    );
    this.updateRequirement(this.passwordRequirements.number, checks.hasNumber);

    // Calculate strength
    let strength = 0;
    if (checks.hasMinLength) strength++;
    if (checks.hasLowercase) strength++;
    if (checks.hasUppercase) strength++;
    if (checks.hasNumber) strength++;

    // Update strength bar
    this.strengthBar.className = "strength-bar";

    if (strength <= 1) {
      this.strengthBar.classList.add("weak");
    } else if (strength === 2) {
      this.strengthBar.classList.add("fair");
    } else if (strength === 3) {
      this.strengthBar.classList.add("good");
    } else if (strength === 4) {
      this.strengthBar.classList.add("strong");
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

  // Validate form inputs
  validateForm(data) {
    let isValid = true;

    // First Name validation
    if (!data.firstName) {
      this.showFieldError("firstName", "First name is required");
      isValid = false;
    } else if (data.firstName.length < 2) {
      this.showFieldError(
        "firstName",
        "First name must be at least 2 characters",
      );
      isValid = false;
    } else {
      this.clearFieldError("firstName");
    }

    // Last Name validation
    if (!data.lastName) {
      this.showFieldError("lastName", "Last name is required");
      isValid = false;
    } else if (data.lastName.length < 2) {
      this.showFieldError(
        "lastName",
        "Last name must be at least 2 characters",
      );
      isValid = false;
    } else {
      this.clearFieldError("lastName");
    }

    // Email validation
    if (!data.email) {
      this.showFieldError("email", "Email is required");
      isValid = false;
    } else if (!validationHelpers.isEmail(data.email)) {
      this.showFieldError("email", "Please enter a valid email address");
      isValid = false;
    } else {
      this.clearFieldError("email");
    }

    // Phone validation
    if (!data.phone) {
      this.showFieldError("phone", "Phone number is required");
      isValid = false;
    } else if (data.phone.length < 10) {
      this.showFieldError("phone", "Please enter a valid phone number");
      isValid = false;
    } else {
      this.clearFieldError("phone");
    }

    // Password validation
    if (!data.password) {
      this.showFieldError("password", "Password is required");
      isValid = false;
    } else if (data.password.length < 6) {
      this.showFieldError("password", "Password must be at least 6 characters");
      isValid = false;
    } else {
      this.clearFieldError("password");
    }

    // Confirm Password validation
    if (!data.confirmPassword) {
      this.showFieldError("confirmPassword", "Please confirm your password");
      isValid = false;
    } else if (data.password !== data.confirmPassword) {
      this.showFieldError("confirmPassword", "Passwords do not match");
      isValid = false;
    } else {
      this.clearFieldError("confirmPassword");
    }

    // Date of Birth validation
    if (!data.dateOfBirth) {
      this.showFieldError("dateOfBirth", "Date of birth is required");
      isValid = false;
    } else {
      const age = this.calculateAge(new Date(data.dateOfBirth));
      if (age < 18) {
        this.showFieldError("dateOfBirth", "You must be at least 18 years old");
        isValid = false;
      } else {
        this.clearFieldError("dateOfBirth");
      }
    }

    // Role validation
    if (!data.role) {
      this.showFieldError("role", "Please select an account type");
      isValid = false;
    } else {
      this.clearFieldError("role");
    }

    // Terms validation
    if (!data.terms) {
      this.showFieldError("terms", "You must agree to the Terms of Service");
      isValid = false;
    } else {
      this.clearFieldError("terms");
    }

    return isValid;
  }

  // Calculate age
  calculateAge(birthDate) {
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (
      monthDiff < 0 ||
      (monthDiff === 0 && today.getDate() < birthDate.getDate())
    ) {
      age--;
    }

    return age;
  }

  // Handle registration
  async handleRegister(formData) {
    this.setLoading(true);

    try {
      const response = await api.register(formData);

      if (response.success) {
        this.showToast(
          "Registration successful! Redirecting to login...",
          "success",
        );

        setTimeout(() => {
          window.location.href = "index.html";
        }, 2000);
      } else {
        this.showToast(
          response.message || "Registration failed. Please try again.",
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

  // Toggle password visibility
  togglePassword(button) {
    const targetId = button.dataset.target;
    const input = document.getElementById(targetId);
    const type =
      input.getAttribute("type") === "password" ? "text" : "password";
    input.setAttribute("type", type);

    const icon = button.querySelector("i");
    icon.classList.toggle("fa-eye");
    icon.classList.toggle("fa-eye-slash");
  }

  // Initialize event listeners
  init() {
    if (!this.form) return;

    // Password strength checker
    this.passwordInput.addEventListener("input", (e) => {
      this.checkPasswordStrength(e.target.value);
    });

    // Confirm password validation
    this.confirmPasswordInput.addEventListener("input", () => {
      if (this.passwordInput.value !== this.confirmPasswordInput.value) {
        this.showFieldError("confirmPassword", "Passwords do not match");
      } else {
        this.clearFieldError("confirmPassword");
      }
    });

    // Toggle password buttons
    document.querySelectorAll(".toggle-password").forEach((button) => {
      button.addEventListener("click", () => this.togglePassword(button));
    });

    // Input event listeners to clear errors
    this.firstNameInput.addEventListener("input", () =>
      this.clearFieldError("firstName"),
    );
    this.lastNameInput.addEventListener("input", () =>
      this.clearFieldError("lastName"),
    );
    this.emailInput.addEventListener("input", () =>
      this.clearFieldError("email"),
    );
    this.phoneInput.addEventListener("input", () =>
      this.clearFieldError("phone"),
    );
    this.roleSelect.addEventListener("change", () =>
      this.clearFieldError("role"),
    );
    this.termsCheckbox.addEventListener("change", () =>
      this.clearFieldError("terms"),
    );

    // Set minimum date for date of birth (18 years ago)
    const today = new Date();
    const minDate = new Date(
      today.getFullYear() - 18,
      today.getMonth(),
      today.getDate(),
    );
    this.dateOfBirthInput.max = minDate.toISOString().split("T")[0];

    // Form submission
    this.form.addEventListener("submit", async (e) => {
      e.preventDefault();

      const formData = {
        firstName: this.firstNameInput.value.trim(),
        lastName: this.lastNameInput.value.trim(),
        email: this.emailInput.value.trim(),
        phone: this.phoneInput.value.trim(),
        password: this.passwordInput.value,
        confirmPassword: this.confirmPasswordInput.value,
        dateOfBirth: this.dateOfBirthInput.value,
        role: this.roleSelect.value,
        terms: this.termsCheckbox.checked,
      };

      
      if (this.validateForm(formData)) {
      // Remove confirmPassword before sending to API
      delete formData.confirmPassword;
      delete formData.terms;
        await this.handleRegister(formData);
      }
    });
  }
}

// Initialize register form when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new RegisterForm();
});

export default RegisterForm;
