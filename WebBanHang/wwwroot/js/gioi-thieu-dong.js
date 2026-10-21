/**
 * Gioi Thieu Dong - Bulletproof GSAP & Scroll Interactions
 * WebBanHang B2B Wholesale & Credit Management
 */
(function () {
    let scrollHandler = null;
    let canvasCleanup = null;

    // Smooth scroll helper with navbar offset
    window.SmoothScroll = function (selector) {
        if (!selector) return;
        const el = document.querySelector(selector);
        if (el) {
            const nav = document.querySelector('.dong-nav');
            const navHeight = nav ? nav.offsetHeight : 70;
            const elementPosition = el.getBoundingClientRect().top + window.scrollY;
            const offsetPosition = elementPosition - navHeight;

            window.scrollTo({
                top: offsetPosition,
                behavior: 'smooth'
            });
        }
    };

    // Main animation initialization
    window.initDongAnimations = function () {
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        // Initialize ambient moving canvas network (if motion permitted)
        if (!prefersReducedMotion) {
            if (canvasCleanup) {
                canvasCleanup();
                canvasCleanup = null;
            }
            canvasCleanup = initAmbientCanvas();
        }

        // Fallback: If GSAP is missing, use native IntersectionObserver
        if (typeof gsap === 'undefined') {
            console.warn('GSAP not detected, activating native IntersectionObserver fallback.');
            initNativeFallback(prefersReducedMotion);
            initBackToTop();
            return;
        }

        if (typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
            // Kill existing triggers to prevent stacking on re-render
            ScrollTrigger.getAll().forEach(t => t.kill());
        }

        if (prefersReducedMotion) {
            initBackToTop();
            return;
        }

        // 1. Mesh gradient subtle movement
        const mesh = document.querySelector('.hero-mesh');
        if (mesh) {
            gsap.to(mesh, {
                x: 120,
                y: 60,
                duration: 12,
                repeat: -1,
                yoyo: true,
                ease: 'sine.inOut'
            });
        }

        // 2. Floating elements in Hero
        if (document.querySelector('.float-1')) {
            gsap.to('.float-1', { y: -25, x: 15, duration: 4.5, repeat: -1, yoyo: true, ease: 'sine.inOut' });
        }
        if (document.querySelector('.float-2')) {
            gsap.to('.float-2', { y: 25, x: -15, duration: 5.5, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 0.5 });
        }
        if (document.querySelector('.float-3')) {
            gsap.to('.float-3', { y: -18, x: 20, duration: 6, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 1.2 });
        }

        // 3. Scroll Reveal for elements
        if (typeof ScrollTrigger !== 'undefined') {
            const revealElements = gsap.utils.toArray('.reveal');
            revealElements.forEach((el) => {
                // If element is already in initial view, animate immediately
                const rect = el.getBoundingClientRect();
                if (rect.top < window.innerHeight * 0.9) {
                    gsap.from(el, {
                        y: 25,
                        opacity: 0,
                        duration: 0.6,
                        ease: 'power2.out',
                        clearProps: 'transform,opacity'
                    });
                } else {
                    gsap.from(el, {
                        scrollTrigger: {
                            trigger: el,
                            start: 'top 90%',
                            toggleActions: 'play none none none'
                        },
                        y: 30,
                        opacity: 0,
                        duration: 0.7,
                        ease: 'power2.out',
                        clearProps: 'transform,opacity'
                    });
                }
            });

            // 4. Counter Animation with thousand formatting and suffix
            const counters = gsap.utils.toArray('.stat-counter');
            counters.forEach((el) => {
                const target = parseFloat(el.getAttribute('data-target')) || 0;
                let suffix = '';
                if (target === 10000 || target === 500) suffix = '+';
                else if (target === 99) suffix = '%';
                else if (target === 24) suffix = '/7';

                const obj = { val: 0 };
                gsap.to(obj, {
                    val: target,
                    duration: 1.8,
                    ease: 'power2.out',
                    scrollTrigger: {
                        trigger: el,
                        start: 'top 90%',
                        toggleActions: 'play none none none'
                    },
                    onUpdate: function () {
                        el.innerText = Math.floor(obj.val).toLocaleString('vi-VN') + suffix;
                    }
                });
            });

            // Refresh ScrollTrigger positions after Blazor layout finishes
            setTimeout(() => {
                ScrollTrigger.refresh();
            }, 150);
        }

        // 5. Back to top button listener
        initBackToTop();

        // 6. Magnetic buttons effect (desktop only)
        if (window.matchMedia('(hover: hover) and (pointer: fine)').matches) {
            document.querySelectorAll('.btn-magnetic').forEach(btn => {
                btn.addEventListener('mousemove', (e) => {
                    const rect = btn.getBoundingClientRect();
                    const x = e.clientX - rect.left - rect.width / 2;
                    const y = e.clientY - rect.top - rect.height / 2;
                    gsap.to(btn, { x: x * 0.25, y: y * 0.25, duration: 0.35, ease: 'power2.out' });
                });
                btn.addEventListener('mouseleave', () => {
                    gsap.to(btn, { x: 0, y: 0, duration: 0.5, ease: 'power2.out' });
                });
            });
        }
    };

    function initBackToTop() {
        const backToTopBtn = document.getElementById('back-to-top');
        const nav = document.querySelector('.dong-nav');

        if (scrollHandler) {
            window.removeEventListener('scroll', scrollHandler);
        }

        scrollHandler = function () {
            const scrollY = window.scrollY;
            if (backToTopBtn) {
                if (scrollY > 400) {
                    backToTopBtn.classList.add('show');
                } else {
                    backToTopBtn.classList.remove('show');
                }
            }
            if (nav) {
                if (scrollY > 20) {
                    nav.classList.add('scrolled');
                } else {
                    nav.classList.remove('scrolled');
                }
            }
        };

        window.addEventListener('scroll', scrollHandler, { passive: true });
        scrollHandler();
    }

    function initNativeFallback(prefersReducedMotion) {
        if (prefersReducedMotion) return;

        const counters = document.querySelectorAll('.stat-counter');
        counters.forEach(c => {
            const target = parseFloat(c.getAttribute('data-target')) || 0;
            c.innerText = target.toLocaleString('vi-VN');
        });

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'none';
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.reveal').forEach(el => observer.observe(el));
    }

    // Ambient animated canvas (Constellation / Data particles network)
    function initAmbientCanvas() {
        const canvas = document.getElementById('dong-ambient-canvas');
        if (!canvas) return null;
        const ctx = canvas.getContext('2d');
        if (!ctx) return null;

        let width = canvas.width = window.innerWidth;
        let height = canvas.height = window.innerHeight;

        const onResize = () => {
            width = canvas.width = window.innerWidth;
            height = canvas.height = window.innerHeight;
        };
        window.addEventListener('resize', onResize);

        // Rich particle count for full-screen immersive presence
        const particleCount = Math.min(Math.max(Math.floor((width * height) / 22000), 45), 75);
        const particles = [];
        const colors = ['#38bdf8', '#60a5fa', '#34d399', '#fbbf24', '#818cf8'];

        for (let i = 0; i < particleCount; i++) {
            particles.push({
                x: Math.random() * width,
                y: Math.random() * height,
                vx: (Math.random() - 0.5) * 0.55,
                vy: (Math.random() - 0.5) * 0.55,
                radius: Math.random() * 2.2 + 1.2,
                color: colors[Math.floor(Math.random() * colors.length)],
                alpha: Math.random() * 0.5 + 0.4
            });
        }

        let animFrameId = null;

        function animate() {
            ctx.clearRect(0, 0, width, height);

            // Connect nearby nodes with luminous lines
            for (let i = 0; i < particles.length; i++) {
                for (let j = i + 1; j < particles.length; j++) {
                    const dx = particles[i].x - particles[j].x;
                    const dy = particles[i].y - particles[j].y;
                    const dist = Math.sqrt(dx * dx + dy * dy);

                    if (dist < 160) {
                        ctx.beginPath();
                        ctx.moveTo(particles[i].x, particles[i].y);
                        ctx.lineTo(particles[j].x, particles[j].y);
                        const alpha = (1 - dist / 160) * 0.32;
                        ctx.strokeStyle = `rgba(56, 189, 248, ${alpha})`;
                        ctx.lineWidth = 1;
                        ctx.stroke();
                    }
                }
            }

            // Move and draw particles with glow halo
            for (let i = 0; i < particles.length; i++) {
                const p = particles[i];
                p.x += p.vx;
                p.y += p.vy;

                if (p.x < 0) p.x = width;
                else if (p.x > width) p.x = 0;
                if (p.y < 0) p.y = height;
                else if (p.y > height) p.y = 0;

                ctx.save();
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                ctx.fillStyle = p.color;
                ctx.globalAlpha = p.alpha;
                ctx.shadowBlur = 10;
                ctx.shadowColor = p.color;
                ctx.fill();
                ctx.restore();
            }

            animFrameId = requestAnimationFrame(animate);
        }

        animate();

        return () => {
            window.removeEventListener('resize', onResize);
            if (animFrameId) cancelAnimationFrame(animFrameId);
        };
    }

    // Cleanup when Blazor disposes the component
    window.destroyDongAnimations = function () {
        if (typeof ScrollTrigger !== 'undefined') {
            ScrollTrigger.getAll().forEach(t => t.kill());
        }
        if (scrollHandler) {
            window.removeEventListener('scroll', scrollHandler);
            scrollHandler = null;
        }
        if (canvasCleanup) {
            canvasCleanup();
            canvasCleanup = null;
        }
    };
})();
