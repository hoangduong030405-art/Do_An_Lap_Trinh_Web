/* ==========================================================================
   LANDING PAGE JAVASCRIPT - B2B SAAS LEVEL INTERACTIONS
   ECO2415 CAPSTONE 18: B2B DISTRIBUTION & CREDIT MANAGEMENT
   ========================================================================== */

(function () {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // 1. SCROLL PROGRESS BAR & NAVBAR SCROLLED STATE
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
        onScroll();
    }

    // 2. SCROLLSPY (ACTIVE NAVBAR ITEM INDICATOR)
    function initScrollspy() {
        const navLinks = document.querySelectorAll('.landing-nav-link[data-scrollspy]');
        if (!navLinks.length) return;

        const sections = [];
        navLinks.forEach(link => {
            const sectionId = link.getAttribute('data-scrollspy');
            const section = document.getElementById(sectionId);
            if (section) sections.push({ id: sectionId, el: section, link: link });
        });

        if (!sections.length) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    navLinks.forEach(l => l.classList.remove('active'));
                    const match = sections.find(s => s.el === entry.target);
                    if (match) {
                        match.link.classList.add('active');
                    }
                }
            });
        }, {
            root: null,
            rootMargin: '-20% 0px -70% 0px',
            threshold: 0
        });

        sections.forEach(s => observer.observe(s.el));
    }

    // 3. 3D HERO MOCKUP WORKSPACE PARALLAX (MAX 6 DEGREES)
    function initHeroParallax() {
        if (prefersReducedMotion || window.innerWidth < 992) return;

        const stage = document.getElementById('heroMockupStage');
        const mainCard = document.getElementById('mainDashboardCard');
        const float1 = document.getElementById('floatCard1');
        const float2 = document.getElementById('floatCard2');
        const float3 = document.getElementById('floatCard3');

        if (!stage || !mainCard) return;

        stage.addEventListener('mousemove', (e) => {
            const rect = stage.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            // Restrict rotation to max 5.5 degrees
            const rawRotX = ((y - centerY) / centerY) * -5.5;
            const rawRotY = ((x - centerX) / centerX) * 5.5;
            const rotX = Math.max(-5.5, Math.min(5.5, rawRotX));
            const rotY = Math.max(-5.5, Math.min(5.5, rawRotY));

            mainCard.style.transform = `perspective(1000px) rotateX(${rotX}deg) rotateY(${rotY}deg) translateZ(8px)`;
            
            if (float1) {
                float1.style.transform = `translate(${rotY * 1.5}px, ${-rotX * 1.5}px) translateZ(28px)`;
            }
            if (float2) {
                float2.style.transform = `translate(${-rotY * 1.3}px, ${rotX * 1.3}px) translateZ(36px)`;
            }
            if (float3) {
                float3.style.transform = `translate(${rotY * 1.1}px, ${rotX * 1.1}px) translateZ(20px)`;
            }
        });

        stage.addEventListener('mouseleave', () => {
            mainCard.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateZ(0)';
            if (float1) float1.style.transform = '';
            if (float2) float2.style.transform = '';
            if (float3) float3.style.transform = '';
        });
    }

    // 4. SPOTLIGHT CURSOR FOLLOWER FOR BENTO CARDS
    function initSpotlightCards() {
        if (prefersReducedMotion) return;

        const cards = document.querySelectorAll('.spotlight-card');
        cards.forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                card.style.setProperty('--mouse-x', `${x}px`);
                card.style.setProperty('--mouse-y', `${y}px`);
            });
        });
    }

    // 5. SCROLL REVEALS VIA INTERSECTION OBSERVER
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

    // 6. STAT NUMBER COUNTERS (easeOutExpo)
    function initCounters() {
        const counters = document.querySelectorAll('[data-counter-target]');
        if (!counters.length) return;

        function animateCounter(el) {
            const target = parseFloat(el.getAttribute('data-counter-target') || '0');
            const suffix = el.getAttribute('data-counter-suffix') || '';
            const decimals = parseInt(el.getAttribute('data-counter-decimals') || '0', 10);
            
            if (prefersReducedMotion || target <= 0) {
                el.textContent = target.toLocaleString('vi-VN', { minimumFractionDigits: decimals, maximumFractionDigits: decimals }) + suffix;
                return;
            }

            const duration = 1800; // ms
            const startTime = performance.now();

            function updateCounter(currentTime) {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                
                // easeOutExpo formula
                const easeProgress = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
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

    // 7. AI TYPEWRITER SIMULATION
    function initAiTypewriter() {
        const typewriterEl = document.querySelector('.landing-ai-typewriter');
        if (!typewriterEl) return;

        const textToType = "AVA AI Copilot: Đang thẩm định rủi ro đối tác KH001 (Tạp Hóa Bà Ba). Điểm tín nhiệm: 86/100 (Hạng A - Uy Tín Cao). Lịch sử cấn trừ FIFO 100% đúng hạn. Khuyến nghị Kế toán: Đủ điều kiện duy trì hạn mức tín dụng 20 Trđ.";
        
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

    // 8. 3D CARD TILT FOR PRODUCT CARDS
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
                const rotateX = ((y - centerY) / centerY) * -5;
                const rotateY = ((x - centerX) / centerX) * 5;

                card.style.transform = `perspective(800px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-4px)`;
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(800px) rotateX(0deg) rotateY(0deg) translateY(0)';
            });
        });
    }

    // GLOBAL INITIALIZER CALLED ONCE OR AFTER BLAZOR NAVIGATES
    window.initLandingPageJs = function () {
        initScrollEffects();
        initScrollspy();
        initHeroParallax();
        initSpotlightCards();
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
