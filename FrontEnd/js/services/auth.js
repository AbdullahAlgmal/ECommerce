// js/services/auth.js

import { api } from "./api.js";
import { storage } from "../utils/helpers.js";
import { STORAGE_KEYS } from "../utils/constants.js";

class AuthService {
  constructor() {
    this.accessToken = null;
    this.refreshToken = null;
    this.user = null;
    this.tokenExpiry = null;
    this.refreshTimer = null;
    this.loadTokens();
  }

  // Load tokens from storage
  loadTokens() {
    const isRemembered = storage.get(STORAGE_KEYS.REMEMBER_ME);
    this.accessToken = storage.get(STORAGE_KEYS.ACCESS_TOKEN, !isRemembered);
    this.refreshToken = storage.get(STORAGE_KEYS.REFRESH_TOKEN, !isRemembered);
    this.user = storage.get(STORAGE_KEYS.USER, !isRemembered);
    this.tokenExpiry = storage.get("tokenExpiry", !isRemembered);
  }

  // Save tokens to storage
  saveTokens(accessToken, refreshToken, user, expiresAt, rememberMe) {
    storage.set(STORAGE_KEYS.REMEMBER_ME, rememberMe);
    storage.set(STORAGE_KEYS.ACCESS_TOKEN, accessToken, !rememberMe);
    storage.set(STORAGE_KEYS.REFRESH_TOKEN, refreshToken, !rememberMe);
    storage.set(STORAGE_KEYS.USER, user, !rememberMe);
    storage.set("tokenExpiry", expiresAt, !rememberMe);

    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    this.user = user;
    this.tokenExpiry = expiresAt;
  }

  // Clear tokens
  clearTokens() {
    const isRemembered = storage.get(STORAGE_KEYS.REMEMBER_ME);
    storage.remove(STORAGE_KEYS.ACCESS_TOKEN, !isRemembered);
    storage.remove(STORAGE_KEYS.REFRESH_TOKEN, !isRemembered);
    storage.remove(STORAGE_KEYS.USER, !isRemembered);
    storage.remove("tokenExpiry", !isRemembered);

    this.accessToken = null;
    this.refreshToken = null;
    this.user = null;
    this.tokenExpiry = null;

    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }
  }

  // Check if authenticated
  isAuthenticated() {
    if (!this.accessToken) return false;
    if (this.tokenExpiry && new Date(this.tokenExpiry) < new Date()) {
      this.clearTokens();
      return false;
    }
    return true;
  }

  // Get current user
  async getCurrentUser() {
    if (!this.isAuthenticated()) return null;

    try {
      const response = await api.getCurrentUser();
      if (response.success) {
        this.user = response.data;
        return this.user;
      }
    } catch (error) {
      console.error("Failed to get current user:", error);
    }
    return null;
  }

  // Login
  async login(email, password, rememberMe = false) {
    try {
      const response = await api.login(email, password);

      if (response.success) {
        this.saveTokens(
          response.data.token,
          response.data.refreshToken,
          response.data.user,
          response.data.expiresAt,
          rememberMe,
        );

        this.startTokenRefreshTimer();

        return { success: true, user: response.data.user };
      }

      return { success: false, message: response.message };
    } catch (error) {
      return { success: false, message: error.message };
    }
  }

  // Logout
  async logout() {
    try {
      if (this.refreshToken && this.user) {
        await api.logout(this.user.email, this.refreshToken);
      }
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      this.clearTokens();
    }
  }

  // Refresh token
  async refreshToken() {
    try {
      const response = await api.refreshToken(
        this.user.email,
        this.refreshToken,
      );

      if (response.success) {
        this.saveTokens(
          response.data.accessToken,
          response.data.refreshToken,
          this.user,
          response.data.accessTokenExpiresAt,
          storage.get(STORAGE_KEYS.REMEMBER_ME),
        );

        this.startTokenRefreshTimer();
        return { success: true };
      }

      this.clearTokens();
      return { success: false };
    } catch (error) {
      this.clearTokens();
      return { success: false };
    }
  }

  // Start token refresh timer
  startTokenRefreshTimer() {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }

    if (this.tokenExpiry) {
      const expiryTime = new Date(this.tokenExpiry);
      const currentTime = new Date();
      const timeUntilExpiry = expiryTime - currentTime;
      const refreshTime = Math.max(timeUntilExpiry - 60000, 10000);

      this.refreshTimer = setTimeout(async () => {
        const result = await this.refreshToken();
        if (!result.success) {
          window.location.href = "/";
        }
      }, refreshTime);
    }
  }
}

export const auth = new AuthService();
