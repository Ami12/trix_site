// Carousel functionality (home page)
let currentSlide = 0;
let slides = [];
let dots = [];
let totalSlides = 0;

function showSlide(index) {
    if (!slides || slides.length === 0) return;

    // remove active
    slides.forEach(s => s.classList.remove('active'));
    if (dots && dots.length) dots.forEach(d => d.classList.remove('active'));

    // wrap index
    if (index >= totalSlides) {
        currentSlide = 0;
    } else if (index < 0) {
        currentSlide = totalSlides - 1;
    } else {
        currentSlide = index;
    }

    // add active
    slides[currentSlide].classList.add('active');
    if (dots && dots.length) dots[currentSlide].classList.add('active');
}

function moveCarousel(direction) {
    showSlide(currentSlide + direction);
}

function goToSlide(index) {
    showSlide(index);
}

// Init after DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    slides = Array.from(document.querySelectorAll('.carousel-slide'));
    dots = Array.from(document.querySelectorAll('.dot'));
    totalSlides = slides.length;

    if (totalSlides === 0) return;

    // Initial state
    showSlide(0);

    // (Optional) make dots clickable if present
    if (dots && dots.length) {
        dots.forEach((dot, i) => {
            dot.addEventListener('click', () => goToSlide(i));
        });
    }

    // Close mobile menu when clicking a link inside it
    document.querySelectorAll('.mobile-menu a').forEach(link => {
        link.addEventListener('click', () => {
            const menu = document.getElementById('mobileMenu');
            if (window.Site && typeof window.Site.toggleMobileMenu === 'function') {
                window.Site.toggleMobileMenu(false);
            } else if (menu) {
                menu.classList.remove('open');
                document.body.classList.remove('menu-open');
            }
        });
    });

    // Auto-rotate carousel every 5 seconds
    setInterval(() => moveCarousel(1), 5000);
});
