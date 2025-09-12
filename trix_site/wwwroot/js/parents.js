// Parents page – specific JS only
// Shared behaviors (mobile menu toggle, FAQ, animations, smooth scroll, etc.)
// are handled in ~/js/site.js via the shared layout.

document.addEventListener('DOMContentLoaded', () => {
    // Close mobile menu when clicking on a link inside it
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

    // Place any parents-page exclusive logic here...
});
