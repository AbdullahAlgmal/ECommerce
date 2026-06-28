import { api } from "../services/api.js";

// ==============================
// Confirm Password Live Validation
// ==============================

const confirmPassword = document.getElementById("confirmPassword");
const password = document.getElementById("password");

confirmPassword.addEventListener("input", () => {
  if (confirmPassword.value !== password.value) {
    confirmPassword.setCustomValidity("Passwords do not match");
  } else {
    confirmPassword.setCustomValidity("");
  }
});

// ==============================
// Set Minimum Date for Date of Birth
// ==============================
const minDate = new Date(Date.now() - 18 * 365.25 * 24 * 60 * 60 * 1000);

document.getElementById("DateOfBirth").max = minDate
  .toISOString()
  .split("T")[0];

// ==============================
// Form Submission Handling
// ==============================

const form = document.getElementById("registerForm");

form.addEventListener("submit", async function (e) {
  e.preventDefault();
    let formData = new FormData(form);
    const data = Object.fromEntries(formData.entries());

    console.log("Form Data:", data); // Log the form data for debugging
  try {
    const response = await api.register(data);

    if (response.success) {
      console.log(
        "Registration successful! Redirecting to login...",
        "success",
      );

      setTimeout(() => {
        window.location.href = "index.html";
      }, 2000);
    } else {
      console.error(
        "Registration failed:",
        response.message || "Pleasetry again.",
      );
    }
  } catch (error) {
    console.error(
      "Registration error:",
      error.message || "An error occurred. Please try again.",
    );
  }
});
