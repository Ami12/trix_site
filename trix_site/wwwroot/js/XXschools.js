// FAQ toggle (סגירה של כולם ופתיחה של הנוכחי)
document.addEventListener('DOMContentLoaded', () => {
    const faqItems = Array.from(document.querySelectorAll('.faq-item'));

    document.querySelectorAll('.faq-q').forEach(q => {
        q.addEventListener('click', () => {
            const faqItem = q.closest('.faq-item');
            const isActive = faqItem && faqItem.classList.contains('active');

            // Close all FAQ items
            faqItems.forEach(item => item.classList.remove('active'));

            // Open clicked item if it wasn't active
            if (faqItem && !isActive) {
                faqItem.classList.add('active');
            }
        });
    });

    // Scroll-triggered animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.classList.add('visible');
                }, index * 100);
                // אחרי שהאלמנט נכנס, אין צורך להמשיך לתצפת עליו
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe animated elements
    document.querySelectorAll('.animate-on-scroll, .way-card, .process-step')
        .forEach(el => observer.observe(el));
});
