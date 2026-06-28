// js/utils/constants.js

// API Configuration
export const API_CONFIG = {
  BASE_URL: "https://localhost:7227/api",
  TIMEOUT: 30000,
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY: 1000,
};

// App Configuration
export const APP_CONFIG = {
  NAME: "E-Commerce Platform",
  VERSION: "1.0.0",
  DEFAULT_THEME: "light",
  TOKEN_EXPIRY_BUFFER: 5 * 60 * 1000, // 5 minutes
};

// Storage Keys
export const STORAGE_KEYS = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
  USER: "user",
  THEME: "theme",
  REMEMBER_ME: "rememberMe",
};

// HTTP Status Codes
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  SERVER_ERROR: 500,
};

// Error Messages
export const ERROR_MESSAGES = {
  NETWORK_ERROR: "Network error. Please check your connection.",
  SERVER_ERROR: "Server error. Please try again later.",
  UNAUTHORIZED: "Invalid email or password.",
  SESSION_EXPIRED: "Your session has expired. Please login again.",
  DEFAULT: "An error occurred. Please try again.",
};

// Validation Rules
export const VALIDATION_RULES = {
  EMAIL: {
    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    message: "Please enter a valid email address",
  },
  PASSWORD: {
    minLength: 6,
    message: "Password must be at least 6 characters",
  },
};

// Demo Credentials
export const DEMO_CREDENTIALS = {
  admin: {
    email: "admin@example.com",
    password: "Admin123",
    role: "Admin"
  },
  customer: {
    email: "ali@gmail.com",
    password: "Flash.com5252",
    role: "Customer",
  },
};
