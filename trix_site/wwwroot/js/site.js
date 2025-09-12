
/*!
 * 12trix · site.js (shared)
 * Lightweight utilities + common UI initializers
 * No dependencies, RTL-safe
 */
(function () {
  // ===== Utilities =====
  const $  = (sel, root=document) => root.querySelector(sel);
  const $$ = (sel, root=document) => Array.from(root.querySelectorAll(sel));

  function on(event, selector, handler, root=document) {
    root.addEventListener(event, function(e){
      const target = e.target.closest(selector);
      if (target && root.contains(target)) handler(e, target);
    });
  }

  const debounce = (fn, wait=200) => {
    let t; return (...args) => { clearTimeout(t); t=setTimeout(()=>fn(...args), wait); };
  };

  function track(eventName, params={}) {
    try {
      if (window.gtag) { window.gtag('event', eventName, params); }
      else if (window.dataLayer) { window.dataLayer.push({event: eventName, ...params}); }
      else { /* fallback */ console.debug('track:', eventName, params); }
    } catch(e) { /* noop */ }
  }

  // ===== Mobile Menu =====
  function toggleMobileMenu(force) {
    const menu = $('#mobileMenu');
    if (!menu) return;
    const isOpen = typeof force === 'boolean' ? !force : menu.classList.contains('open');
    menu.classList.toggle('open', !isOpen);
    document.body.classList.toggle('menu-open', !isOpen);
  }

  function initMobileMenu() {
    on('click', '.mobile-toggle', () => toggleMobileMenu());
    // close on ESC
    document.addEventListener('keydown', (e)=>{ if (e.key === 'Escape') toggleMobileMenu(false); });
    // close on outside click
    document.addEventListener('click', (e)=>{
      const menu = $('#mobileMenu'); if (!menu) return;
      const within = e.target.closest('#mobileMenu, .mobile-toggle');
      if (!within && menu.classList.contains('open')) toggleMobileMenu(false);
    });
  }

  // ===== FAQ (accordion) =====
  function initFAQ() {
    $$('[data-accordion], .faq-list').forEach(list => {
      on('click', '.faq-q', (e, q) => {
        const item = q.closest('.faq-item') || q.parentElement;
        const a = item.querySelector('.faq-a');
        const expanded = item.classList.toggle('open');
        if (a) a.style.maxHeight = expanded ? (a.scrollHeight + 'px') : '0px';
        // collapse siblings (single-open)
        $$('.faq-item.open', list).forEach(el => { if (el!==item){ el.classList.remove('open'); const aa = el.querySelector('.faq-a'); if (aa) aa.style.maxHeight='0px'; }});
        track('faq_toggle', {question: q.textContent?.trim() || ''});
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
        track.style.transform = `translateX(${-(idx*w)}px)`;
      }

      const relayout = debounce(layout, 100);
      window.addEventListener('resize', relayout);
      layout();

      if (prev) prev.addEventListener('click', ()=> goTo(idx-1));
      if (next) next.addEventListener('click', ()=> goTo(idx+1));

      // swipe (touch)
      let startX=0, currX=0, dragging=false;
      track.addEventListener('touchstart', (e)=>{ dragging=true; startX=e.touches[0].clientX; }, {passive:true});
      track.addEventListener('touchmove', (e)=>{ if(!dragging) return; currX=e.touches[0].clientX; }, {passive:true});
      track.addEventListener('touchend', ()=> {
        if(!dragging) return; dragging=false;
        const dx = currX - startX;
        if (Math.abs(dx) > 40) { dx>0 ? goTo(idx-1) : goTo(idx+1); }
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
        if (el) el.scrollIntoView({behavior:'smooth', block:'start'});
      }
    });
  }

  // ===== In-View Animations =====
  function initInViewAnimations() {
    const targets = $$('.target-card, .feature-card, .testimonial-card, .animate-on-scroll, .way-card, .process-step, [data-animate]');
    if (!('IntersectionObserver' in window) || targets.length === 0) return;
    const io = new IntersectionObserver((entries)=>{
      entries.forEach(en=>{
        if (en.isIntersecting) {
          en.target.classList.add('in-view');
          io.unobserve(en.target);
        }
      });
    }, { rootMargin: '0px 0px -10% 0px', threshold: 0.15 });
    targets.forEach(t=> io.observe(t));
  }

  // ===== Lazy Images (data-src -> src) =====
  function initLazyImages() {
    const imgs = $$('img[data-src]');
    if (!('IntersectionObserver' in window) || imgs.length === 0) {
      imgs.forEach(img => { if (!img.getAttribute('src')) img.src = img.dataset.src; });
      return;
    }
    const io = new IntersectionObserver((entries)=>{
      entries.forEach(en=>{
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
    $$('a[target="_blank"]').forEach(a=>{
      const rel = (a.getAttribute('rel') || '').split(/\s+/);
      if (!rel.includes('noopener')) rel.push('noopener');
      if (!rel.includes('noreferrer')) rel.push('noreferrer');
      a.setAttribute('rel', rel.join(' ').trim());
    });
  }

  // ===== Analytics hooks (examples) =====
  function initAnalyticsHooks() {
    on('click', 'a[data-track], button[data-track]', (e, el)=>{
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
  function ready(fn){ if(document.readyState!=='loading') fn(); else document.addEventListener('DOMContentLoaded', fn); }

  ready(function(){
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


// CTA → Modal logic for Schools page
// CTA → Modal logic for Schools page
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

    // ניקוי שגיאות קודמות
    function clearLeadErrors(form) {
        form?.querySelectorAll('.input-error').forEach(el => el.classList.remove('input-error'));
        form?.querySelectorAll('.field-error').forEach(el => el.textContent = '');
    }

    // ולידציה: שדות חובה + אימייל בסיסי
    function validateLeadForm(form) {
        let ok = true;
        const setErr = (name, text) => {
            ok = false;
            const input = form.querySelector(`[name="${name}"]`);
            const err = form.querySelector(`.field-error[data-for="${name}"]`);
            if (input) input.classList.add('input-error');
            if (err) err.textContent = text || 'שדה חובה';
        };

        const required = ['full_name', 'school_name', 'email'];
        required.forEach(n => {
            const v = (form.querySelector(`[name="${n}"]`)?.value || '').trim();
            if (!v) setErr(n);
        });

        const email = (form.querySelector('[name="email"]')?.value || '').trim();
        if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            setErr('email', 'כתובת דוא״ל לא תקינה');
        }
        return ok;
    }

    function openLeadModal(type, ctx = {}) {
        const modalWrap = $('#leadModalOverlay');
        const modal = $('#leadModal');
        if (!modalWrap || !modal) return;

        // אפס מצב תצוגה בכל פתיחה
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

        // איפוס לטופס הבא
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

    function showLeadThanks() {
        const body = $('#leadBody');
        const thanks = $('#leadThanks');
        if (!body || !thanks) return;

        body.style.display = 'none';
        thanks.hidden = false;

        // סגירה אוטומטית אחרי 20 שניות
        if (window.__leadCloseTimer) clearTimeout(window.__leadCloseTimer);
        window.__leadCloseTimer = setTimeout(() => closeLeadModal(), 20000);

        // כפתור סגירה ידנית (ביטול הטיימר)
        thanks.querySelector('[data-lead-close]')?.addEventListener('click', () => {
            clearTimeout(window.__leadCloseTimer);
            closeLeadModal();
        }, { once: true });
    }

    // Global clicks: פתיחת מודל לפי data-cta וסגירה
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-cta]');
        if (btn) {
            const type = btn.dataset.cta;
            const section = btn.dataset.section || '';

            if (type === 'challenge') return; // מעבר ישיר
            if (btn.tagName === 'A') e.preventDefault();

            // mailto? אפשר גם לפתוח מודל (fallback)
            openLeadModal(type, { section });
            return;
        }

        if (e.target.matches('[data-lead-close]') || e.target.id === 'leadModalOverlay') {
            closeLeadModal();
        }
    });

    // מאזין מואצל – תופס גם אם ה-Partial נטען אחרי
    document.addEventListener('submit', async (e) => {
        const form = e.target;
        if (!form || form.id !== 'leadForm') return;

        e.preventDefault();

        const msg = $('#leadMsg');
        clearLeadErrors(form);
        if (msg) msg.textContent = '';

        // honeypot
        if ($('#leadWebsite')?.value?.trim()) {
            if (msg) msg.textContent = 'אירעה שגיאה. נסו שוב.';
            return;
        }

        // ולידציה בצד-לקוח
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
                headers: { 'X-Requested-With': 'XMLHttpRequest' } // מציין שזה AJAX
            });

            if (!res.ok) {
                if (msg) msg.textContent = 'לא הצלחנו לשלוח. בדקו את השדות ונסו שוב.';
                console.error('SubmitLead error:', res.status, await res.text());
                return;
            }

            if (window.gtag) gtag('event', 'lead_form_submit', { cta_type: fd.get('cta_type') || '' });

            // תודה בתוך המודל + אוטו-סגירה
            showLeadThanks();

        } catch (err) {
            if (msg) msg.textContent = 'שגיאה זמנית בשליחה. נסו שוב.';
            console.error(err);
        }
    });

    // חשיפה אם תרצה לפתוח מתסריט
    window.openLeadModal = openLeadModal;
})();

