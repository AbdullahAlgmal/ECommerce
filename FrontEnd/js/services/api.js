// js/services/api.js

import {
  API_CONFIG,
  HTTP_STATUS,
  ERROR_MESSAGES,
  STORAGE_KEYS,
} from "../utils/constants.js";
import { storage } from "../utils/helpers.js";

class ApiService {
  constructor() {
    this.baseURL = API_CONFIG.BASE_URL;
    this.timeout = API_CONFIG.TIMEOUT;
  }

  // Get auth headers
  getAuthHeaders() {
    const token = storage.get(
      STORAGE_KEYS.ACCESS_TOKEN,
      !storage.get(STORAGE_KEYS.REMEMBER_ME),
    );
    return {
      "Content-Type": "application/json",
      Authorization: token ? `Bearer ${token}` : "",
    };
  }

  // Handle response
  async handleResponse(response) {
    const data = await response.json();

    if (!response.ok) {
      if (response.status === HTTP_STATUS.UNAUTHORIZED) {
        storage.remove(STORAGE_KEYS.ACCESS_TOKEN);
        storage.remove(STORAGE_KEYS.REFRESH_TOKEN);
        storage.remove(STORAGE_KEYS.USER);
        window.location.href = "/";
      }
      throw new Error(data.message || ERROR_MESSAGES.DEFAULT);
    }

    return data;
  }

  // Handle error
  handleError(error) {
    console.error("API Error:", error);
    throw error;
  }

  // Request method
  async request(endpoint, method = "GET", data = null, requiresAuth = false) {
    const url = `${this.baseURL}${endpoint}`;
    const headers = requiresAuth
      ? this.getAuthHeaders()
      : { "Content-Type": "application/json" };

    const config = {
      method,
      headers,
      body: data ? JSON.stringify(data) : null,
    };

    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), this.timeout);

      const response = await fetch(url, {
        ...config,
        signal: controller.signal,
      });
      clearTimeout(timeoutId);

      return await this.handleResponse(response);
    } catch (error) {
      if (error.name === "AbortError") {
        throw new Error(ERROR_MESSAGES.NETWORK_ERROR);
      }
      return this.handleError(error);
    }
  }

  // Auth endpoints
  async login(email, password) {
    return this.request("/Auth/login", "POST", { email, password });
  }

  async register(userData) {
    return this.request("/Auth/register", "POST", userData);
  }

  async refreshToken(email, refreshToken) {
    return this.request("/Auth/refresh-token", "POST", { email, refreshToken });
  }

  async logout(email, refreshToken) {
    return this.request("/Auth/logout", "POST", { email, refreshToken });
  }

  async getCurrentUser() {
    return this.request("/Auth/me", "GET", null, true);
  }

  async validateToken() {
    return this.request("/Auth/validate", "GET", null, true);
  }

  // User endpoints
  async getUserById(id) {
    return this.request(`/Users/${id}`, "GET", null, true);
  }

  async updateUser(id, userData) {
    return this.request(`/Users/${id}`, "PUT", userData, true);
  }
}

export const api = new ApiService();
