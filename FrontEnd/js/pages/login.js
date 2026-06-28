import { auth } from "../services/auth.js";
import { STORAGE_KEYS, DEMO_CREDENTIALS } from "../utils/constants.js";
import { storage } from "../utils/helpers.js";

window.addEventListener("DOMContentLoaded", () => {
    const rememberMe = storage.get(STORAGE_KEYS.REMEMBER_ME);
    if (rememberMe) {
        const emailInput = document.getElementById("email");
        emailInput.value = storage.get(STORAGE_KEYS.USER)?.email || "";
        document.getElementById("remember").checked = true;
    }
});

let form = document.querySelector("form");

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    let formData = new FormData(form);

    auth
      .login(
        formData.get("Email"),
        formData.get("Password"),
        formData.get("remember") === "on",
      )
        .then((response) => {
        if (response.success) {
          window.location.href = "../../shop.html";
        } else {
          alert(response.message);
        }
      })
      .catch((error) => {
        console.error("Login error:", error);
        alert("An error occurred during login. Please try again.");
      });
});

let customerbtn = document.getElementById("customer-demo-btn");

customerbtn.addEventListener("click", async (e) => {
    let emailInput = document.getElementById("email");
    emailInput.value = DEMO_CREDENTIALS.customer.email;
    let passwordInput = document.getElementById("password");
    passwordInput.value = DEMO_CREDENTIALS.customer.password;
});

let adminbtn = document.getElementById("admin-demo-btn");

adminbtn.addEventListener("click", async (e) => {
    let emailInput = document.getElementById("email");
    emailInput.value = DEMO_CREDENTIALS.admin.email;
    let passwordInput = document.getElementById("password");
    passwordInput.value = DEMO_CREDENTIALS.admin.password;
});