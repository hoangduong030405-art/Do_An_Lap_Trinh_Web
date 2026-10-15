/* ==========================================================================
   LANDING PAGE JAVASCRIPT - B2B SAAS LEVEL INTERACTIONS
   ECO2415 CAPSTONE 18: B2B DISTRIBUTION & CREDIT MANAGEMENT
   ========================================================================== */

(function () {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // 1. SCROLL PROGRESS BAR & NAVBAR SCROLLED STATE
    function initScrollEffects() {
        function onScroll() {
            const scrollTop = window.scrollY || document.documentElement.scrollTop;
            const scrollHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            const progressBar = document.querySelector('.landing-scroll-progress');
            const navbar = document.querySelector('.landing-navbar');
            
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

        if (window._landingScrollHandler) {
            window.removeEventListener('scroll', window._landingScrollHandler);
        }
        window._landingScrollHandler = onScroll;
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }

    // 2. SCROLLSPY (ACTIVE NAVBAR ITEM INDICATOR WITH ROOTMARGIN -40% 0px -50% 0px)
    function initScrollspy() {
        if (window._landingScrollspyObserver) {
            window._landingScrollspyObserver.disconnect();
        }

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
            rootMargin: '-40% 0px -50% 0px',
            threshold: 0
        });

        window._landingScrollspyObserver = observer;
        sections.forEach(s => observer.observe(s.el));
    }

    // 3. UNIFIED REQUESTANIMATIONFRAME 3D PARALLAX & SCROLL ENGINE
    function initParallaxEngine() {
        if (prefersReducedMotion) return;

        const monitoredSections = document.querySelectorAll('.landing-section, .landing-hero');
        if (!monitoredSections.length) return;

        const activeSections = new Set();

        if (window._landingSectionObserver) {
            window._landingSectionObserver.disconnect();
        }

        const sectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    activeSections.add(entry.target);
                    scheduleRender();
                } else {
                    activeSections.delete(entry.target);
                }
            });
        }, {
            root: null,
            rootMargin: '100px 0px 100px 0px',
            threshold: 0
        });

        monitoredSections.forEach(s => sectionObserver.observe(s));
        window._landingSectionObserver = sectionObserver;

        let isRenderPending = false;

        function renderParallax() {
            isRenderPending = false;
            const windowHeight = window.innerHeight;
            const scrollY = window.scrollY || document.documentElement.scrollTop;

            activeSections.forEach(section => {
                const rect = section.getBoundingClientRect();
                const sectionHeight = rect.height;
                const sectionMiddle = rect.top + sectionHeight / 2;
                const viewportMiddle = windowHeight / 2;
                
                // Normalized progress: -1 (above viewport) to 0 (centered) to 1 (below viewport)
                const offsetFromCenter = (sectionMiddle - viewportMiddle) / (windowHeight / 2 + sectionHeight / 2);
                const clampedOffset = Math.max(-1.5, Math.min(1.5, offsetFromCenter));

                // 1. Shift all 3D background elements with data-parallax-speed
                const parallaxItems = section.querySelectorAll('[data-parallax-speed]');
                parallaxItems.forEach(item => {
                    const speed = parseFloat(item.getAttribute('data-parallax-speed') || '0.12');
                    const yShift = -clampedOffset * speed * 160;
                    const rotY = clampedOffset * speed * 35;
                    const rotX = -clampedOffset * speed * 20;

                    item.style.transform = `translate3d(0, ${yShift.toFixed(1)}px, 0) rotateY(${rotY.toFixed(1)}deg) rotateX(${rotX.toFixed(1)}deg)`;
                });

                // 2. Scroll-driven Hero Mockup 3D tilt & Z-layer separation
                if (section.id === 'gioi-thieu' && window.innerWidth >= 992) {
                    const mainCard = section.querySelector('#mainDashboardCard');
                    const float1 = section.querySelector('#floatCard1');
                    const float2 = section.querySelector('#floatCard2');
                    const float3 = section.querySelector('#floatCard3');

                    if (mainCard) {
                        const heroScrollRatio = Math.min(1, Math.max(0, scrollY / 600));
                        const tiltX = heroScrollRatio * 8; // tilts up to 8 deg on scroll down
                        const scale = 1 - heroScrollRatio * 0.04; // subtle scale down
                        mainCard.style.transform = `perspective(1000px) rotateX(${tiltX.toFixed(1)}deg) scale(${scale.toFixed(3)}) translateZ(0)`;

                        // Outward Z-layer and translate separation
                        if (float1) {
                            const z1 = 28 + heroScrollRatio * 32;
                            float1.style.transform = `translate3d(${heroScrollRatio * 15}px, ${-heroScrollRatio * 12}px, ${z1.toFixed(0)}px)`;
                        }
                        if (float2) {
                            const z2 = 36 + heroScrollRatio * 40;
                            float2.style.transform = `translate3d(${-heroScrollRatio * 18}px, ${heroScrollRatio * 14}px, ${z2.toFixed(0)}px)`;
                        }
                        if (float3) {
                            const z3 = 20 + heroScrollRatio * 25;
                            float3.style.transform = `translate3d(0px, ${heroScrollRatio * 16}px, ${z3.toFixed(0)}px)`;
                        }
                    }
                }
            });
        }

        function scheduleRender() {
            if (!isRenderPending) {
                isRenderPending = true;
                requestAnimationFrame(renderParallax);
            }
        }

        if (window._landingParallaxScrollHandler) {
            window.removeEventListener('scroll', window._landingParallaxScrollHandler);
        }
        window._landingParallaxScrollHandler = scheduleRender;
        window.addEventListener('scroll', scheduleRender, { passive: true });
        scheduleRender();
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

    // 5. SCROLL REVEALS (FADE-UP & 3D STAGGER) VIA INTERSECTION OBSERVER
    function initScrollReveals() {
        if (prefersReducedMotion) {
            document.querySelectorAll('.reveal-fade-up, .reveal-3d-card, .landing-timeline-step').forEach(el => el.classList.add('is-revealed'));
            return;
        }

        const reveals = document.querySelectorAll('.reveal-fade-up, .reveal-3d-card, .landing-timeline-step');
        if (!reveals.length) return;

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-revealed');
                    obs.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.10,
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

    // 8. 3D CARD TILT & SPECULAR GLARE FOR CARDS (.lp-tilt-card & .tilt-effect)
    function initTiltEffect() {
        if (prefersReducedMotion || window.innerWidth < 992) return;

        const tiltCards = document.querySelectorAll('.lp-tilt-card, .tilt-effect');
        tiltCards.forEach(card => {
            let isTicking = false;

            card.addEventListener('mousemove', (e) => {
                if (isTicking) return;
                isTicking = true;

                requestAnimationFrame(() => {
                    const rect = card.getBoundingClientRect();
                    const x = e.clientX - rect.left;
                    const y = e.clientY - rect.top;
                    const centerX = rect.width / 2;
                    const centerY = rect.height / 2;

                    // Restrict rotation to max 6 degrees
                    const rawRotateX = ((y - centerY) / centerY) * -6;
                    const rawRotateY = ((x - centerX) / centerX) * 6;
                    const rotateX = Math.max(-6, Math.min(6, rawRotateX));
                    const rotateY = Math.max(-6, Math.min(6, rawRotateY));

                    card.style.transform = `perspective(1000px) rotateX(${rotateX.toFixed(1)}deg) rotateY(${rotateY.toFixed(1)}deg) translateY(-6px) translateZ(10px)`;
                    card.style.setProperty('--glare-x', `${x}px`);
                    card.style.setProperty('--glare-y', `${y}px`);

                    // Float inner element forward
                    const floatInner = card.querySelector('.lp-tilt-float');
                    if (floatInner) {
                        floatInner.style.transform = `translateZ(28px) scale(1.03)`;
                    }

                    isTicking = false;
                });
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(0) translateZ(0)';
                const floatInner = card.querySelector('.lp-tilt-float');
                if (floatInner) {
                    floatInner.style.transform = 'translateZ(0) scale(1)';
                }
            });
        });
    }

    // GLOBAL INITIALIZER CALLED ONCE OR AFTER BLAZOR NAVIGATES
    window.initLandingPageJs = function () {
        initScrollEffects();
        initScrollspy();
        initParallaxEngine();
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

