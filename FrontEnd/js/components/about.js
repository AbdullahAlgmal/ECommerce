// js/components/about.js

import { auth } from "../services/auth.js";
import { db } from "../services/indexedDB.js";

class AboutPage {
  constructor() {
    this.currentTestimonialIndex = 0;
    this.testimonials = [
      {
        name: "Sarah Johnson",
        title: "Regular Customer",
        text: "Absolutely love shopping here! The quality of products is exceptional and the customer service is outstanding. My orders always arrive on time and in perfect condition.",
        rating: 5,
        avatar: "https://randomuser.me/api/portraits/women/1.jpg",
      },
      {
        name: "Michael Chen",
        title: "Tech Enthusiast",
        text: "Best online shopping experience I've ever had. The website is easy to navigate, prices are competitive, and the checkout process is smooth. Highly recommend!",
        rating: 5,
        avatar: "https://randomuser.me/api/portraits/men/2.jpg",
      },
      {
        name: "Emily Rodriguez",
        title: "Fashion Blogger",
        text: "I've been shopping here for over a year and I'm always impressed. Great selection, fast shipping, and easy returns. My go-to online store!",
        rating: 5,
        avatar: "https://randomuser.me/api/portraits/women/3.jpg",
      },
      {
        name: "David Kim",
        title: "Business Owner",
        text: "Professional service from start to finish. Their bulk ordering process is efficient and their support team is very responsive. Will continue to be a loyal customer.",
        rating: 4,
        avatar: "https://randomuser.me/api/portraits/men/4.jpg",
      },
      {
        name: "Lisa Thompson",
        title: "Home Decor Expert",
        text: "The product quality exceeded my expectations. Everything from packaging to delivery was handled with care. I've recommended this store to all my friends!",
        rating: 5,
        avatar: "https://randomuser.me/api/portraits/women/5.jpg",
      },
    ];

    this.team = [
      {
        name: "John Anderson",
        position: "CEO & Founder",
        bio: "Former tech executive with 15+ years of experience in e-commerce.",
        image: "https://randomuser.me/api/portraits/men/11.jpg",
        social: { linkedin: "#", twitter: "#" },
      },
      {
        name: "Sarah Miller",
        position: "Chief Operations Officer",
        bio: "Operations expert dedicated to delivering excellence in logistics.",
        image: "https://randomuser.me/api/portraits/women/12.jpg",
        social: { linkedin: "#", twitter: "#" },
      },
      {
        name: "David Wilson",
        position: "Head of Product",
        bio: "Product visionary focused on creating delightful user experiences.",
        image: "https://randomuser.me/api/portraits/men/13.jpg",
        social: { linkedin: "#", twitter: "#" },
      },
      {
        name: "Emily Brown",
        position: "Customer Success Manager",
        bio: "Passionate about ensuring every customer has an exceptional experience.",
        image: "https://randomuser.me/api/portraits/women/14.jpg",
        social: { linkedin: "#", twitter: "#" },
      },
    ];

    this.init();
  }

  // Show toast message
  showToast(message, type = "success") {
    const toast = document.getElementById("toast");
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

  // Animate counter
  animateCounter(element, target, suffix = "") {
    let current = 0;
    const increment = target / 50;
    const timer = setInterval(() => {
      current += increment;
      if (current >= target) {
        element.textContent = target + suffix;
        clearInterval(timer);
      } else {
        element.textContent = Math.floor(current) + suffix;
      }
    }, 20);
  }

  // Animate stats on scroll
  animateStats() {
    const stats = document.querySelectorAll(".stat-number[data-count]");

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const element = entry.target;
            const target = parseInt(element.dataset.count);
            const isFloat = element.dataset.count.includes(".");

            if (!isFloat) {
              this.animateCounter(element, target, "k");
            } else {
              element.textContent = target + "★";
            }

            observer.unobserve(element);
          }
        });
      },
      { threshold: 0.5 },
    );

    stats.forEach((stat) => observer.observe(stat));
  }

  // Render team members
  renderTeam() {
    const teamGrid = document.getElementById("teamGrid");
    if (!teamGrid) return;

    teamGrid.innerHTML = this.team
      .map(
        (member) => `
            <div class="team-card">
                <div class="team-image">
                    <img src="${member.image}" alt="${member.name}">
                </div>
                <div class="team-info">
                    <h3>${member.name}</h3>
                    <div class="team-position">${member.position}</div>
                    <p class="team-bio">${member.bio}</p>
                    <div class="team-social">
                        <a href="${member.social.linkedin}"><i class="fab fa-linkedin-in"></i></a>
                        <a href="${member.social.twitter}"><i class="fab fa-twitter"></i></a>
                        <a href="#"><i class="fab fa-envelope"></i></a>
                    </div>
                </div>
            </div>
        `,
      )
      .join("");
  }

  // Render testimonials
  renderTestimonials() {
    const track = document.getElementById("testimonialsTrack");
    if (!track) return;

    track.innerHTML = this.testimonials
      .map(
        (testimonial) => `
            <div class="testimonial-card">
                <div class="testimonial-avatar">
                    <img src="${testimonial.avatar}" alt="${testimonial.name}">
                </div>
                <div class="testimonial-text">"${testimonial.text}"</div>
                <div class="testimonial-name">${testimonial.name}</div>
                <div class="testimonial-title">${testimonial.title}</div>
                <div class="testimonial-rating">
                    ${this.renderStars(testimonial.rating)}
                </div>
            </div>
        `,
      )
      .join("");

    this.setupTestimonialSlider();
  }

  // Render stars
  renderStars(rating) {
    let stars = "";
    for (let i = 1; i <= 5; i++) {
      if (i <= rating) {
        stars += '<i class="fas fa-star"></i>';
      } else {
        stars += '<i class="far fa-star"></i>';
      }
    }
    return stars;
  }

  // Setup testimonial slider
  setupTestimonialSlider() {
    const track = document.getElementById("testimonialsTrack");
    const prevBtn = document.getElementById("sliderPrev");
    const nextBtn = document.getElementById("sliderNext");
    const dotsContainer = document.getElementById("sliderDots");

    if (!track || !prevBtn || !nextBtn) return;

    const totalSlides = this.testimonials.length;
    const slideWidth = 100;

    // Create dots
    dotsContainer.innerHTML = "";
    for (let i = 0; i < totalSlides; i++) {
      const dot = document.createElement("div");
      dot.classList.add("dot");
      if (i === this.currentTestimonialIndex) dot.classList.add("active");
      dot.addEventListener("click", () => this.goToSlide(i));
      dotsContainer.appendChild(dot);
    }

    // Update slide position
    const updateSlider = () => {
      track.style.transform = `translateX(-${this.currentTestimonialIndex * slideWidth}%)`;
      document.querySelectorAll(".dot").forEach((dot, index) => {
        dot.classList.toggle("active", index === this.currentTestimonialIndex);
      });
    };

    // Next slide
    nextBtn.addEventListener("click", () => {
      if (this.currentTestimonialIndex < totalSlides - 1) {
        this.currentTestimonialIndex++;
        updateSlider();
      }
    });

    // Previous slide
    prevBtn.addEventListener("click", () => {
      if (this.currentTestimonialIndex > 0) {
        this.currentTestimonialIndex--;
        updateSlider();
      }
    });

    // Auto play
    let autoPlayInterval = setInterval(() => {
      if (this.currentTestimonialIndex < totalSlides - 1) {
        this.currentTestimonialIndex++;
        updateSlider();
      } else {
        this.currentTestimonialIndex = 0;
        updateSlider();
      }
    }, 5000);

    // Pause on hover
    const slider = document.querySelector(".testimonials-slider");
    slider.addEventListener("mouseenter", () =>
      clearInterval(autoPlayInterval),
    );
    slider.addEventListener("mouseleave", () => {
      autoPlayInterval = setInterval(() => {
        if (this.currentTestimonialIndex < totalSlides - 1) {
          this.currentTestimonialIndex++;
          updateSlider();
        } else {
          this.currentTestimonialIndex = 0;
          updateSlider();
        }
      }, 5000);
    });
  }

  // Go to specific slide
  goToSlide(index) {
    this.currentTestimonialIndex = index;
    const track = document.getElementById("testimonialsTrack");
    track.style.transform = `translateX(-${index * 100}%)`;
    document.querySelectorAll(".dot").forEach((dot, i) => {
      dot.classList.toggle("active", i === index);
    });
  }

  // Setup back to top button
  setupBackToTop() {
    const backToTop = document.getElementById("backToTop");

    window.addEventListener("scroll", () => {
      if (window.scrollY > 300) {
        backToTop.classList.add("visible");
      } else {
        backToTop.classList.remove("visible");
      }
    });

    backToTop.addEventListener("click", () => {
      window.scrollTo({ top: 0, behavior: "smooth" });
    });
  }

  // Setup fade-in animations on scroll
  setupScrollAnimations() {
    const animatedElements = document.querySelectorAll(
      ".story-content, .mission-card, .vision-card, .value-card, .team-card, .stat-item",
    );

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.style.animation = "fadeInUp 0.6s ease forwards";
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1 },
    );

    animatedElements.forEach((el) => {
      el.style.opacity = "0";
      observer.observe(el);
    });
  }

  // Update cart count
  async updateCartCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const cart = await db.getAll("cart");
    const userCart = cart.filter((item) => item.userId === userId);
    const count = userCart.reduce((sum, item) => sum + item.quantity, 0);

    const cartCount = document.getElementById("cartCount");
    if (cartCount) cartCount.textContent = count;
  }

  // Update wishlist count
  async updateWishlistCount() {
    if (!auth.user) return;

    const userId = auth.user.id;
    const wishlist = await db.getAll("wishlist");
    const count = wishlist.filter((item) => item.userId === userId).length;

    const wishlistCount = document.getElementById("wishlistCount");
    if (wishlistCount) wishlistCount.textContent = count;
  }

  // Setup search
  setupSearch() {
    const searchToggle = document.getElementById("searchToggle");
    const searchBar = document.getElementById("searchBar");
    const closeSearch = document.getElementById("closeSearch");
    const searchInput = document.getElementById("searchInput");

    if (searchToggle && searchBar) {
      searchToggle.addEventListener("click", () => {
        searchBar.style.display =
          searchBar.style.display === "none" ? "block" : "none";
        if (searchBar.style.display === "block") {
          searchInput.focus();
        }
      });

      closeSearch.addEventListener("click", () => {
        searchBar.style.display = "none";
        searchInput.value = "";
      });

      let searchTimeout;
      searchInput.addEventListener("input", () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
          const searchTerm = searchInput.value;
          if (searchTerm) {
            window.location.href = `shop.html?search=${encodeURIComponent(searchTerm)}`;
          }
        }, 300);
      });
    }
  }

  // Setup user dropdown
  setupUserDropdown() {
    const userBtn = document.getElementById("userBtn");
    const dropdown = document.getElementById("userDropdown");

    if (userBtn && dropdown) {
      userBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        dropdown.classList.toggle("show");
      });

      document.addEventListener("click", (e) => {
        if (!userBtn.contains(e.target) && !dropdown.contains(e.target)) {
          dropdown.classList.remove("show");
        }
      });
    }

    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", async () => {
        await auth.logout();
        window.location.href = "index.html";
      });
    }
  }

  // Initialize about page
  async init() {
    this.renderTeam();
    this.renderTestimonials();
    this.animateStats();
    this.setupBackToTop();
    this.setupScrollAnimations();
    this.setupSearch();
    this.setupUserDropdown();

    if (auth.isAuthenticated()) {
      await this.updateCartCount();
      await this.updateWishlistCount();
      auth.startTokenRefreshTimer();
    }
  }
}

// Initialize about page when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new AboutPage();
});
