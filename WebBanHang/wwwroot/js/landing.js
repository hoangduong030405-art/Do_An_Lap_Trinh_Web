/* ==========================================================================
   LANDING PAGE JAVASCRIPT - INTERACTIVE UX, SCROLL REVEALS & PERFORMANCE
   ========================================================================== */

(function () {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // 1. SCROLL PROGRESS & NAVBAR SCROLLED STATE
    function initScrollEffects() {
        const progressBar = document.querySelector('.landing-scroll-progress');
        const navbar = document.querySelector('.landing-navbar');

        function onScroll() {
            const scrollTop = window.scrollY || document.documentElement.scrollTop;
            const scrollHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            
            // Progress Bar
            if (progressBar && scrollHeight > 0) {
                const progress = (scrollTop / scrollHeight) * 100;
                progressBar.style.width = progress + '%';
            }

            // Navbar Blur State
            if (navbar) {
                if (scrollTop > 20) {
                    navbar.classList.add('scrolled');
                } else {
                    navbar.classList.remove('scrolled');
                }
            }
        }

        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll(); // Run immediately on page load
    }

    // 2. SCROLL REVEALS VIA INTERSECTION OBSERVER
    function initScrollReveals() {
        if (prefersReducedMotion) {
            document.querySelectorAll('.reveal-fade-up').forEach(el => el.classList.add('is-revealed'));
            return;
        }

        const reveals = document.querySelectorAll('.reveal-fade-up');
        if (!reveals.length) return;

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-revealed');
                    obs.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.12,
            rootMargin: '0px 0px -40px 0px'
        });

        reveals.forEach(el => observer.observe(el));
    }

    // 3. STAT NUMBER COUNTERS
    function initCounters() {
        const counters = document.querySelectorAll('[data-counter-target]');
        if (!counters.length) return;

        function animateCounter(el) {
            const target = parseFloat(el.getAttribute('data-counter-target') || '0');
            const suffix = el.getAttribute('data-counter-suffix') || '';
            const decimals = parseInt(el.getAttribute('data-counter-decimals') || '0', 10);
            
            if (prefersReducedMotion) {
                el.textContent = target.toLocaleString('vi-VN', { minimumFractionDigits: decimals, maximumFractionDigits: decimals }) + suffix;
                return;
            }

            const duration = 1800; // ms
            const startTime = performance.now();

            function updateCounter(currentTime) {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                
                // EaseOutQuad formula
                const easeProgress = 1 - (1 - progress) * (1 - progress);
                const currentVal = target * easeProgress;

                el.textContent = currentVal.toLocaleString('vi-VN', {
                    minimumFractionDigits: decimals,
                    maximumFractionDigits: decimals
                }) + suffix;

                if (progress < 1) {
                    requestAnimationFrame(updateCounter);
                } else {
                    el.textContent = target.toLocaleString('vi-VN', {
                        minimumFractionDigits: decimals,
                        maximumFractionDigits: decimals
                    }) + suffix;
                }
            }

            requestAnimationFrame(updateCounter);
        }

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.2 });

        counters.forEach(c => observer.observe(c));
    }

    // 4. AI TYPEWRITER SIMULATION
    function initAiTypewriter() {
        const typewriterEl = document.querySelector('.landing-ai-typewriter');
        if (!typewriterEl) return;

        const textToType = "AVA AI Copilot: Đang thẩm định rủi ro đối tác KH001 (Tạp Hóa Bà Ba). Điểm tín nhiệm: 86/100 (Hạng A - Uy Tín Cao). Lịch sử cấn trừ FIFO 100% đúng hạn. Khuyến nghị Kế toán trưởng: Đủ điều kiện nâng hạn mức tín dụng từ 20 Trđ lên 35 Trđ.";
        
        let hasRun = false;
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !hasRun) {
                    hasRun = true;
                    if (prefersReducedMotion) {
                        typewriterEl.textContent = textToType;
                        return;
                    }
                    let index = 0;
                    typewriterEl.textContent = "";
                    const interval = setInterval(() => {
                        if (index < textToType.length) {
                            typewriterEl.textContent += textToType.charAt(index);
                            index++;
                        } else {
                            clearInterval(interval);
                        }
                    }, 22);
                }
            });
        }, { threshold: 0.3 });

        observer.observe(typewriterEl);
    }

    // 5. 3D CARD TILT EFFECT (DESKTOP ONLY)
    function initTiltEffect() {
        if (prefersReducedMotion || window.innerWidth < 992) return;

        const tiltCards = document.querySelectorAll('.tilt-effect');
        tiltCards.forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                const rotateX = ((y - centerY) / centerY) * -6;
                const rotateY = ((x - centerX) / centerX) * 6;

                card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-6px)`;
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(0)';
            });
        });
    }

    // GLOBAL INITIALIZER CALLED ONCE OR AFTER BLAZOR NAVIGATES
    window.initLandingPageJs = function () {
        initScrollEffects();
        initScrollReveals();
        initCounters();
        initAiTypewriter();
        initTiltEffect();
    };

    // Auto-bind on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', window.initLandingPageJs);
    } else {
        window.initLandingPageJs();
    }
})();
