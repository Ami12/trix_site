/*!
 * 12trix · site.js (shared)
 * Lightweight utilities + common UI initializers
 * No dependencies, RTL-safe
 */
(function () {
    // ===== Utilities =====
    const $ = (sel, root = document) => root.querySelector(sel);
    const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    function on(event, selector, handler, root = document) {
        root.addEventListener(event, function (e) {
            const target = e.target.closest(selector);
            if (target && root.contains(target)) handler(e, target);
        });
    }

    const debounce = (fn, wait = 200) => {
        let t; return (...args) => { clearTimeout(t); t = setTimeout(() => fn(...args), wait); };
    };

    function track(eventName, params = {}) {
        try {
            if (window.gtag) { window.gtag('event', eventName, params); }
            else if (window.dataLayer) { window.dataLayer.push({ event: eventName, ...params }); }
            else { /* fallback */ console.debug('track:', eventName, params); }
        } catch (e) { /* noop */ }
    }

    // ===== Mobile Menu =====
    function toggleMobileMenu(force) {
        const menu = $('#mobileMenu');
        if (!menu) return;
        const isOpen = typeof force === 'boolean' ? !force : menu.classList.contains('open');
        // תאימות: גם 'open' וגם 'active' עבור CSS ישן/חדש
        menu.classList.toggle('open', !isOpen);
        menu.classList.toggle('active', !isOpen);
        document.body.classList.toggle('menu-open', !isOpen);
    }

    function initMobileMenu() {
        on('click', '.mobile-toggle', () => toggleMobileMenu());
        // close on ESC
        document.addEventListener('keydown', (e) => { if (e.key === 'Escape') toggleMobileMenu(false); });
        // close on outside click
        document.addEventListener('click', (e) => {
            const menu = $('#mobileMenu'); if (!menu) return;
            const within = e.target.closest('#mobileMenu, .mobile-toggle');
            if (!within && (menu.classList.contains('open') || menu.classList.contains('active'))) toggleMobileMenu(false);
        });
    }

    // ===== FAQ (accordion) =====
    function initFAQ() {
        $$('[data-accordion], .faq-list').forEach(list => {
            on('click', '.faq-q', (e, q) => {
                const item = q.closest('.faq-item') || q.parentElement;
                const a = item.querySelector('.faq-a');
                const expanded = item.classList.toggle('open');
                item.classList.toggle('active', expanded); // תאימות ל-CSS ישן
                if (a) a.style.maxHeight = expanded ? (a.scrollHeight + 'px') : '0px';
                // collapse siblings (single-open)
                $$('.faq-item.open', list).forEach(el => {
                    if (el !== item) {
                        el.classList.remove('open', 'active');
                        const aa = el.querySelector('.faq-a');
                        if (aa) aa.style.maxHeight = '0px';
                    }
                });
                track('faq_toggle', { question: q.textContent?.trim() || '' });
            }, list);
        });
    }

    // ===== Carousel (basic) =====
    function initCarousel() {
        $$('[data-carousel]').forEach(carousel => {
            const track = $('.carousel-track', carousel);
            const slides = $$('.carousel-slide', track);
            const prev = $('.carousel-prev', carousel);
            const next = $('.carousel-next', carousel);
            if (!track || slides.length === 0) return;

            let idx = 0;
            function layout() {
                const w = carousel.clientWidth;
                track.style.width = (w * slides.length) + 'px';
                slides.forEach(s => s.style.width = w + 'px');
                goTo(idx);
            }
            function goTo(i) {
                idx = (i + slides.length) % slides.length;
                const w = carousel.clientWidth;
                track.style.transform = `translateX(${-(idx * w)}px)`;
            }

            const relayout = debounce(layout, 100);
            window.addEventListener('resize', relayout);
            layout();

            if (prev) prev.addEventListener('click', () => goTo(idx - 1));
            if (next) next.addEventListener('click', () => goTo(idx + 1));

            // swipe (touch)
            let startX = 0, currX = 0, dragging = false;
            track.addEventListener('touchstart', (e) => { dragging = true; startX = e.touches[0].clientX; }, { passive: true });
            track.addEventListener('touchmove', (e) => { if (!dragging) return; currX = e.touches[0].clientX; }, { passive: true });
            track.addEventListener('touchend', () => {
                if (!dragging) return; dragging = false;
                const dx = currX - startX;
                if (Math.abs(dx) > 40) { dx > 0 ? goTo(idx - 1) : goTo(idx + 1); }
            });
        });
    }

    // ===== Smooth Scroll =====
    function initSmoothScroll() {
        on('click', 'a[data-scroll]', (e, a) => {
            const href = a.getAttribute('href') || '';
            if (href.startsWith('#')) {
                e.preventDefault();
                const el = document.getElementById(href.slice(1));
                if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    }

    // ===== In-View Animations =====
    function initInViewAnimations() {
        const targets = $$('.target-card, .feature-card, .testimonial-card, .animate-on-scroll, .way-card, .process-step, [data-animate]');
        if (!('IntersectionObserver' in window) || targets.length === 0) return;

        let i = 0;
        const io = new IntersectionObserver((entries) => {
            entries.forEach(en => {
                if (!en.isIntersecting) return;
                const el = en.target;

                // סטאגר קל (100ms) לקלפים מסומנים; אפשר לשלוט ידנית עם data-delay="200"
                const baseDelay = (el.classList.contains('animate-on-scroll') || el.classList.contains('way-card') || el.classList.contains('process-step')) ? (i++ * 100) : 0;
                const extra = parseInt(el.getAttribute('data-delay') || '0', 10);
                const delay = baseDelay + (isNaN(extra) ? 0 : extra);

                setTimeout(() => {
                    el.classList.add('in-view');   // המחלקה שהאתר משתמש בה
                    el.classList.add('visible');   // תאימות לקוד/‏CSS ישן
                    io.unobserve(el);
                }, delay);
            });
        }, { rootMargin: '0px 0px -10% 0px', threshold: 0.15 });

        targets.forEach(t => io.observe(t));
    }

    // ===== Lazy Images (data-src -> src) =====
    function initLazyImages() {
        const imgs = $$('img[data-src]');
        if (!('IntersectionObserver' in window) || imgs.length === 0) {
            imgs.forEach(img => { if (!img.getAttribute('src')) img.src = img.dataset.src; });
            return;
        }
        const io = new IntersectionObserver((entries) => {
            entries.forEach(en => {
                if (en.isIntersecting) {
                    const img = en.target;
                    if (img.dataset.src) img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    io.unobserve(img);
                }
            });
        }, { rootMargin: '100px', threshold: 0.01 });
        imgs.forEach(img => io.observe(img));
    }

    // ===== External links hardening =====
    function initExternalLinks() {
        $$('a[target="_blank"]').forEach(a => {
            const rel = (a.getAttribute('rel') || '').split(/\s+/);
            if (!rel.includes('noopener')) rel.push('noopener');
            if (!rel.includes('noreferrer')) rel.push('noreferrer');
            a.setAttribute('rel', rel.join(' ').trim());
        });
    }

    // ===== Analytics hooks (examples) =====
    function initAnalyticsHooks() {
        on('click', 'a[data-track], button[data-track]', (e, el) => {
            const name = el.getAttribute('data-track') || 'click';
            track(name, {
                href: el.getAttribute('href') || '',
                text: (el.textContent || '').trim(),
                id: el.id || '',
                classes: el.className || ''
            });
        });
    }

    // ===== Document ready =====
    function ready(fn) { if (document.readyState !== 'loading') fn(); else document.addEventListener('DOMContentLoaded', fn); }

    ready(function () {
        initMobileMenu();
        initFAQ();
        initCarousel();
        initSmoothScroll();
        initInViewAnimations();
        initLazyImages();
        initExternalLinks();
        initAnalyticsHooks();

        // expose small API if needed
        window.Site = { on, $, $$, debounce, track, toggleMobileMenu };
    });
})();


// ===== CTA → Lead Modal + Contact Thanks =====
(function () {
    const $ = (sel, root = document) => root.querySelector(sel);
    const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    const COPY = {
        demo: { title: 'הדגמה חיה לצוות מתמטיקה', intro: '15–20 דק׳ זום. נראה איך מודדים התקדמות ומקימים “שבוע נושא”.' },
        quote: { title: 'הצעת מחיר מותאמת לבית הספר', intro: 'סמנו כמה כיתות/שכבות ותצורת הפעלה — ונשלח הצעה מסודרת.' },
        week: { title: 'שבוע נושא – מוכן להפעלה', intro: 'בחרו נושא וכיתות — נשלח מערך מוכן + גישה לאפליקציות.' },
        access: { title: 'גישה למורים + הדרכת צוות', intro: 'מקימים גישה למערכת ומזמנים הדרכת צוות קצרה.' },
        email: { title: 'שליחת פנייה במייל', intro: 'מעדיפים מייל? השאירו פרטים ונחזור אליכם.' },
        challenge: { title: 'פרטים לגבי האתגר', intro: 'מלאו את הפרטים ובקרוב נצא לדרך' }
    };

    function buildExtras(type) {
        if (type === 'quote') {
            return `
        <label>מס׳ כיתות משוער
          <input name="classes" type="number" min="1">
        </label>
      `;
        }
        if (type === 'week') {
            return `
        <label>נושא
          <select name="topic">
            <option value="fractions">שברים</option>
            <option value="times-table">לוח הכפל</option>
            <option value="percent">אחוזים</option>
          </select>
        </label>
        <label>שכבות יעד
          <input name="grades" placeholder="דוג׳: ד׳–ו׳">
        </label>
      `;
        }
        if (type === 'access') {
            return `
        <label>מס׳ מורים
          <input name="teachers" type="number" min="1">
        </label>
      `;
        }
        return '';
    }

    // ===== Form error helpers =====
    function clearLeadErrors(form) {
        form?.querySelectorAll('.input-error').forEach(el => el.classList.remove('input-error'));
        form?.querySelectorAll('.field-error').forEach(el => el.textContent = '');
    }

    function validateLeadForm(form) {
        // נקה שגיאות קודמות
        clearLeadErrors(form);

        let ok = true;
        const getEl = (n) => form.elements[n] || form.elements[n?.toLowerCase?.()] || form.elements[n?.toUpperCase?.()];
        const getVal = (n) => ((getEl(n)?.value) || '').trim();
        const setErr = (names, text) => {
            ok = false;
            const el = Array.isArray(names) ? (getEl(names[0]) || getEl(names[1])) : getEl(names);
            if (el) el.classList.add('input-error');
            const sel = Array.isArray(names)
                ? `.field-error[data-for="${names[0]}"], .field-error[data-for="${names[1]}"]`
                : `.field-error[data-for="${names}"]`;
            const err = form.querySelector(sel);
            if (err) err.textContent = text || 'שדה חובה';
        };

        // האם זה טופס "צור קשר"?
        const ctaType = getVal('Cta_Type') || getVal('cta_type');
        const section = getVal('Section') || getVal('section');
        const isContact = /^contact/i.test(ctaType) || /^contact-/i.test(section);

        // שדות חובה
        const required = isContact
            ? [['Full_Name', 'full_name'], ['Email', 'email'], ['Notes', 'notes']]              // צור קשר
            : [['Full_Name', 'full_name'], ['School_Name', 'school_name'], ['Email', 'email']]; // שאר ה-CTA

        required.forEach(([A, B]) => {
            const v = getVal(A) || getVal(B);
            if (!v) setErr([A, B]);
        });

        const email = getVal('Email') || getVal('email');
        if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            setErr(['Email', 'email'], 'כתובת דוא״ל לא תקינה');
        }

        return ok;
    }

    // ===== Lead Modal (schools) =====
    function openLeadModal(type, ctx = {}) {
        const modalWrap = $('#leadModalOverlay');
        const modal = $('#leadModal');
        if (!modalWrap || !modal) return;

        // reset view
        const body = $('#leadBody');
        const thanks = $('#leadThanks');
        const form = $('#leadForm');
        const msg = $('#leadMsg');

        if (thanks) thanks.hidden = true;
        if (body) body.style.display = '';
        if (msg) msg.textContent = '';
        clearLeadErrors(form);

        const c = COPY[type] || { title: 'יצירת קשר', intro: 'נשמח לחזור אליכם עם פרטים.' };
        $('#leadTitle').textContent = c.title;
        $('#leadIntro').textContent = c.intro;
        $('#leadCtaType').value = type || '';
        $('#leadSection').value = ctx.section || '';
        $('#leadExtras').innerHTML = buildExtras(type);

        modalWrap.classList.add('open');
        modalWrap.setAttribute('aria-hidden', 'false');
        modal.focus();

        if (window.gtag) gtag('event', 'lead_form_open', { cta_type: type, section: ctx.section || '' });
    }

    function closeLeadModal() {
        const modalWrap = $('#leadModalOverlay');
        if (!modalWrap) return;
        if (window.__leadCloseTimer) { clearTimeout(window.__leadCloseTimer); window.__leadCloseTimer = null; }

        modalWrap.classList.remove('open');
        modalWrap.setAttribute('aria-hidden', 'true');

        // reset for next open
        const body = $('#leadBody');
        const thanks = $('#leadThanks');
        const form = $('#leadForm');
        const msg = $('#leadMsg');
        if (thanks) thanks.hidden = true;
        if (body) body.style.display = '';
        if (msg) msg.textContent = '';
        clearLeadErrors(form);
        form?.reset();
    }

    // תודה בתוך מודל (בתי ספר)
    function showLeadThanks() {
        const body = $('#leadBody');
        const thanks = $('#leadThanks');
        if (!body || !thanks) return;

        body.style.display = 'none';
        thanks.hidden = false;

        // סגירה אוטומטית אחרי 20 שניות
        if (window.__leadCloseTimer) clearTimeout(window.__leadCloseTimer);
        window.__leadCloseTimer = setTimeout(() => closeLeadModal(), 20000);

        // כפתור סגירה ידנית
        thanks.querySelector('[data-lead-close]')?.addEventListener('click', () => {
            clearTimeout(window.__leadCloseTimer);
            closeLeadModal();
        }, { once: true });
    }

    // ===== Thanks overlay נפרד (למשל בעמוד Contact) =====
    function openLeadThanks() {
        const ov = document.getElementById('leadThanks');
        // אם אין אוברליי נפרד – אין מה לעשות כאן
        if (!ov || document.getElementById('leadBody')) return;

        ov.classList.add('open');                // CSS מציג כשיש .open
        ov.removeAttribute('hidden');
        ov.setAttribute('aria-hidden', 'false');

        const hint = document.getElementById('leadAutoClose');
        let secs = 20;
        if (hint) hint.textContent = 'החלון ייסגר אוטומטית בעוד 20 שניות…';

        clearInterval(ov._secTimer);
        clearTimeout(ov._autoTimer);

        if (hint) {
            ov._secTimer = setInterval(() => {
                secs--;
                if (secs <= 0) { closeLeadThanks(); return; }
                hint.textContent = `החלון ייסגר אוטומטית בעוד ${secs} שניות…`;
            }, 1000);
        } else {
            ov._autoTimer = setTimeout(closeLeadThanks, 20000);
        }
    }

    function closeLeadThanks() {
        const ov = document.getElementById('leadThanks');
        if (!ov || document.getElementById('leadBody')) return; // אם זה מודל פנימי – לא סוגרים כאן
        ov.classList.remove('open');
        ov.setAttribute('aria-hidden', 'true');
        ov.setAttribute('hidden', '');
        clearInterval(ov._secTimer);
        clearTimeout(ov._autoTimer);
    }

    // ===== Global clicks =====
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-cta]');
        if (btn) {
            const type = btn.dataset.cta;
            const section = btn.dataset.section || '';
            if (btn.tagName === 'A') e.preventDefault();
            openLeadModal(type, { section });
            return;
        }

        // סגירות
        if (e.target.matches('[data-lead-close]')) {
            closeLeadModal();
            closeLeadThanks();
            return;
        }
        if (e.target.id === 'leadModalOverlay') closeLeadModal();
        if (e.target.id === 'leadThanks') closeLeadThanks();
    });

    // ===== Submit handler (delegated) =====
    document.addEventListener('submit', async (e) => {
        const form = e.target;
        if (!form || form.id !== 'leadForm') return;

        e.preventDefault();

        const msg = document.getElementById('leadMsg');
        clearLeadErrors(form);
        if (msg) msg.textContent = '';

        // honeypot
        if (document.getElementById('leadWebsite')?.value?.trim()) {
            if (msg) msg.textContent = 'אירעה שגיאה. נסו שוב.';
            return;
        }

        // client validation
        if (!validateLeadForm(form)) {
            if (msg) msg.textContent = 'יש למלא את שדות החובה המסומנים.';
            return;
        }

        const fd = new FormData(form);

        try {
            const res = await fetch(form.action, {
                method: 'POST',
                body: fd,
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!res.ok) {
                if (msg) msg.textContent = 'לא הצלחנו לשלוח. בדקו את השדות ונסו שוב.';
                console.error('SubmitLead error:', res.status, await res.text());
                return;
            }

            // אנליטיקס
            if (window.gtag) gtag('event', 'lead_form_submit', { cta_type: fd.get('Cta_Type') || fd.get('cta_type') || '' });

            // אם יש מודל בתי הספר (leadBody+leadThanks) – הצג תודה פנימית; אחרת נסה אוברליי נפרד
            if (document.getElementById('leadModalOverlay') && document.getElementById('leadBody')) {
                showLeadThanks();
            } else if (document.getElementById('leadThanks')) {
                // סגור מודל קיים אם יש (בטוח לסגור — לא יעשה כלום אם לא קיים)
                closeLeadModal();
                openLeadThanks();
            }

            // ניקוי טופס והודעות
            if (msg) msg.textContent = 'קיבלנו!';
            form.querySelectorAll('.input-error').forEach(el => el.classList.remove('input-error'));
            form.querySelectorAll('.field-error').forEach(el => el.textContent = '');
            form.reset();

        } catch (err) {
            if (msg) msg.textContent = 'שגיאה זמנית בשליחה. נסו שוב.';
            console.error(err);
        }
    });

    // חשיפה אם תרצה לפתוח מתסריט
    window.openLeadModal = openLeadModal;

})();
