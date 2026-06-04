// js/components/validation.js

import { validationHelpers } from "../utils/helpers.js";

class FormValidator {
  constructor(formId) {
    this.form = document.getElementById(formId);
    this.fields = new Map();
    this.init();
  }

  // Add field validation
  addField(fieldId, rules, errorId = null) {
    this.fields.set(fieldId, {
      rules,
      errorElement: errorId ? document.getElementById(errorId) : null,
      inputElement: document.getElementById(fieldId),
    });
  }

  // Validate single field
  validateField(fieldId) {
    const field = this.fields.get(fieldId);
    if (!field || !field.inputElement) return true;

    const value = field.inputElement.value.trim();
    let isValid = true;
    let errorMessage = "";

    for (const rule of field.rules) {
      const result = rule.validate(value);
      if (!result.isValid) {
        isValid = false;
        errorMessage = result.message;
        break;
      }
    }

    // Update UI
    if (!isValid) {
      field.inputElement.classList.add("error");
      if (field.errorElement) {
        field.errorElement.textContent = errorMessage;
      }
    } else {
      field.inputElement.classList.remove("error");
      if (field.errorElement) {
        field.errorElement.textContent = "";
      }
    }

    return isValid;
  }

  // Validate all fields
  validateAll() {
    let isValid = true;
    for (const [fieldId] of this.fields) {
      if (!this.validateField(fieldId)) {
        isValid = false;
      }
    }
    return isValid;
  }

  // Clear all errors
  clearErrors() {
    for (const [fieldId, field] of this.fields) {
      field.inputElement?.classList.remove("error");
      if (field.errorElement) {
        field.errorElement.textContent = "";
      }
    }
  }

  // Initialize event listeners
  init() {
    if (!this.form) return;

    // Add input event listeners
    for (const [fieldId] of this.fields) {
      const input = document.getElementById(fieldId);
      if (input) {
        input.addEventListener("input", () => this.validateField(fieldId));
        input.addEventListener("blur", () => this.validateField(fieldId));
      }
    }

    // Form submit handler
    this.form.addEventListener("submit", (e) => {
      if (!this.validateAll()) {
        e.preventDefault();
      }
    });
  }
}

// Predefined validation rules
export const ValidationRules = {
  required: (message = "This field is required") => ({
    validate: (value) => ({
      isValid: value !== "",
      message,
    }),
  }),

  email: (message = "Please enter a valid email address") => ({
    validate: (value) => ({
      isValid: validationHelpers.isEmail(value),
      message,
    }),
  }),

  minLength: (min, message = `Must be at least ${min} characters`) => ({
    validate: (value) => ({
      isValid: value.length >= min,
      message,
    }),
  }),

  maxLength: (max, message = `Must be at most ${max} characters`) => ({
    validate: (value) => ({
      isValid: value.length <= max,
      message,
    }),
  }),

  match: (fieldId, message = "Fields do not match") => ({
    validate: (value) => {
      const otherField = document.getElementById(fieldId);
      return {
        isValid: value === otherField?.value,
        message,
      };
    },
  }),

  pattern: (regex, message = "Invalid format") => ({
    validate: (value) => ({
      isValid: regex.test(value),
      message,
    }),
  }),
};

export { FormValidator };
