// js/components/header.js

import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

// Update cart count from IndexedDB
export async function updateCartCount() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id || "guest";
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
}

// Update wishlist count from IndexedDB
export async function updateWishlistCount() {
    if (!auth.isAuthenticated()) return;

    const userId = auth.user?.id || "guest";
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
}

// Setup user dropdown
export function setupUserDropdown() {
    const userBtn = document.getElementById("userBtn");
    const dropdown = document.getElementById("userDropdown");

    if (userBtn && dropdown) {
        userBtn.removeEventListener("click", this.handleUserClick);
        this.handleUserClick = (e) => {
            e.stopPropagation();
            dropdown.classList.toggle("show");
        };
        userBtn.addEventListener("click", this.handleUserClick);

        document.removeEventListener("click", this.handleDocumentClick);
        this.handleDocumentClick = (e) => {
            if (!userBtn.contains(e.target) && !dropdown.contains(e.target)) {
                dropdown.classList.remove("show");
            }
        };
        document.addEventListener("click", this.handleDocumentClick);
    }

    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
        logoutBtn.removeEventListener("click", this.handleLogout);
        this.handleLogout = async () => {
            await auth.logout();
            window.location.href = "index.html";
        };
        logoutBtn.addEventListener("click", this.handleLogout);
    }
}

    // Setup search (local filtering)
export function setupSearch() {
    const searchToggle = document.getElementById("searchToggle");
    const searchBar = document.getElementById("searchBar");
    const closeSearch = document.getElementById("closeSearch");
    const searchInput = document.getElementById("searchInput");

    if (searchToggle && searchBar) {
    searchToggle.removeEventListener("click", this.handleSearchToggle);
    this.handleSearchToggle = () => {
        searchBar.style.display =
        searchBar.style.display === "none" ? "block" : "none";
        if (searchBar.style.display === "block") {
        searchInput.focus();
        }
    };
    searchToggle.addEventListener("click", this.handleSearchToggle);

    closeSearch.removeEventListener("click", this.handleCloseSearch);
    this.handleCloseSearch = () => {
        searchBar.style.display = "none";
        searchInput.value = "";
        this.filters.searchTerm = "";
        this.applyFilters();
    };
    closeSearch.addEventListener("click", this.handleCloseSearch);

    let searchTimeout;
    searchInput.removeEventListener("input", this.handleSearchInput);
    this.handleSearchInput = () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
        this.filters.searchTerm = searchInput.value;
        this.applyFilters();
        }, 300);
    };
    searchInput.addEventListener("input", this.handleSearchInput);
    }
}

