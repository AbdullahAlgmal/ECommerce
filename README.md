# 🛒 E-Commerce Platform

A full-featured e-commerce platform built with ASP.NET Core Web API and modern JavaScript frontend. This project demonstrates a complete online shopping experience with user authentication, product management, shopping cart, wishlist, order processing, and payment integration.

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Running the Application](#-running-the-application)
- [API Documentation](#-api-documentation)
- [Frontend Pages](#-frontend-pages)
- [Database Scripts](#-database-scripts)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### 🛡️ Authentication & Authorization
- JWT-based authentication with refresh tokens
- Role-based authorization (Admin, Customer, Seller)
- User registration and login
- Password hashing with BCrypt
- Token refresh mechanism
- Secure logout

### 🛍️ Shopping Features
- Product browsing with filtering and pagination
- Advanced product search with categories
- Product details with image gallery
- Shopping cart management
- Wishlist functionality
- Product reviews and ratings
- Order history and tracking

### 👤 User Dashboard
- **Customer Dashboard**: Order history, wishlist, profile management
- **Admin Dashboard**: User management, product management, order management

### 💳 Payment & Shipping
- Multiple payment methods (Credit Card, PayPal, Cash on Delivery)
- Shipping method selection (Standard, Express, Overnight)
- Order tracking
- Order status management
- Invoice generation

### 🎨 UI/UX Features
- Dark/Light mode toggle
- Responsive design for all devices
- Real-time cart and wishlist updates
- Toast notifications
- Loading states
- Form validation
- Smooth animations

### 🔧 Technical Features
- Offline-first with IndexedDB
- Background sync with server
- Rate limiting
- API versioning
- Comprehensive error handling
- CORS configuration
- Swagger/OpenAPI documentation

---

## 🛠️ Technology Stack

### Backend
| Technology | Purpose |
|------------|---------|
| ASP.NET Core 8.0 | Web API Framework |
| Entity Framework Core 8.0 | ORM |
| SQL Server | Database |
| JWT | Authentication |
| BCrypt | Password Hashing |
| Swagger/OpenAPI | API Documentation |
| FluentValidation | Request Validation |

### Frontend
| Technology | Purpose |
|------------|---------|
| HTML5 | Markup |
| SCSS | Styling |
| JavaScript (ES6+) | Logic |
| Font Awesome | Icons |
| IndexedDB | Offline Storage |

### Tools & Libraries
| Tool | Purpose |
|------|---------|
| Git | Version Control |
| npm | Package Management |
| SASS | CSS Preprocessor |
| Postman | API Testing |

---
