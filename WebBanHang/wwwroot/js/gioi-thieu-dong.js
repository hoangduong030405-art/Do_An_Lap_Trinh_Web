/**
 * HUEGO B2B - Next-Gen 3D Interactive WebGL & Antigravity Physics Engine
 * Powered by Three.js & GSAP
 * Enterprise Wholesale Distribution & Automated Trade Credit Management
 */
(function () {
    let scrollHandler = null;
    let threeCleanups = [];

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

    // Helper to submit 1-click role demo login
    window.loginDemoRole = function (username, password, returnUrl) {
        let form = document.getElementById('demoRoleLoginForm');
        if (!form) {
            form = document.createElement('form');
            form.id = 'demoRoleLoginForm';
            form.method = 'POST';
            form.action = '/api/auth/login';
            form.style.display = 'none';

            const uInput = document.createElement('input');
            uInput.type = 'hidden';
            uInput.name = 'username';
            uInput.id = 'demoRoleUser';
            form.appendChild(uInput);

            const pInput = document.createElement('input');
            pInput.type = 'hidden';
            pInput.name = 'password';
            pInput.id = 'demoRolePass';
            form.appendChild(pInput);

            const rInput = document.createElement('input');
            rInput.type = 'hidden';
            rInput.name = 'returnUrl';
            rInput.id = 'demoRoleReturn';
            form.appendChild(rInput);

            document.body.appendChild(form);
        }

        document.getElementById('demoRoleUser').value = username;
        document.getElementById('demoRolePass').value = password;
        document.getElementById('demoRoleReturn').value = returnUrl || '/dashboard';
        form.submit();
    };

    // Main animation initialization
    window.initDongAnimations = function () {
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        // Clean any existing Three.js instances before re-initializing
        destroyDongAnimations();

        // 1. Initialize Three.js Interactive Scenes (if motion not reduced)
        if (!prefersReducedMotion && typeof THREE !== 'undefined') {
            try {
                const bgCleanup = initGlobalThreeBackground();
                if (bgCleanup) threeCleanups.push(bgCleanup);
            } catch (err) {
                console.warn('Global Three.js background fallback:', err);
                init2DCanvasFallback();
            }

            try {
                const heroCleanup = initHero3DScene();
                if (heroCleanup) threeCleanups.push(heroCleanup);
            } catch (err) {
                console.warn('Hero 3D scene fallback:', err);
            }

            try {
                const supplyCleanup = initSupply3DScene();
                if (supplyCleanup) threeCleanups.push(supplyCleanup);
            } catch (err) {
                console.warn('Supply 3D banner fallback:', err);
            }
        } else if (typeof THREE === 'undefined') {
            init2DCanvasFallback();
        }

        // 2. Initialize Product Cards 3D Perspective Tilt & Glare
        initProduct3DInteractions();

        // 3. Initialize Parallax HUD on Hero container
        initHeroParallaxHUD();

        // 4. GSAP ScrollTrigger & Reveal
        if (typeof gsap === 'undefined') {
            console.warn('GSAP not detected, activating native IntersectionObserver fallback.');
            initNativeFallback(prefersReducedMotion);
            initBackToTop();
            return;
        }

        if (typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
            ScrollTrigger.getAll().forEach(t => t.kill());
        }

        if (prefersReducedMotion) {
            initBackToTop();
            return;
        }

        // Scroll Reveal for elements
        if (typeof ScrollTrigger !== 'undefined') {
            const revealElements = gsap.utils.toArray('.reveal');
            revealElements.forEach((el) => {
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

            // Counter Animation with thousand formatting and suffix
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

            setTimeout(() => {
                ScrollTrigger.refresh();
            }, 200);
        }

        // 5. Back to top button listener
        initBackToTop();

        // 6. Magnetic buttons effect
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

    /* =========================================================================
       1. GLOBAL THREE.JS PARTICLE GALAXY & SCROLL FLY-THROUGH BACKGROUND
       ========================================================================= */
    function initGlobalThreeBackground() {
        const canvas = document.getElementById('dong-ambient-canvas');
        if (!canvas) return null;

        let width = canvas.width = window.innerWidth;
        let height = canvas.height = window.innerHeight;

        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(60, width / height, 1, 2000);
        camera.position.z = 100;

        const renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            alpha: true,
            antialias: true,
            powerPreference: 'high-performance'
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));

        // ---------------------------------------------------------------------
        // A. BACKGROUND PARTICLE NETWORK
        // ---------------------------------------------------------------------
        const particleCount = Math.min(Math.max(Math.floor((width * height) / 10000), 90), 160);
        const geometry = new THREE.BufferGeometry();
        const positions = new Float32Array(particleCount * 3);
        const velocities = [];
        const colors = new Float32Array(particleCount * 3);

        const colorPalette = [
            new THREE.Color(0x38bdf8), // cyan
            new THREE.Color(0x10b981), // emerald
            new THREE.Color(0x3b82f6), // blue
            new THREE.Color(0xfbbf24), // amber
            new THREE.Color(0x818cf8)  // indigo
        ];

        for (let i = 0; i < particleCount; i++) {
            positions[i * 3] = (Math.random() - 0.5) * 280;
            positions[i * 3 + 1] = (Math.random() - 0.5) * 220;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 750 - 150;

            velocities.push({
                vx: (Math.random() - 0.5) * 0.12,
                vy: (Math.random() - 0.5) * 0.12,
                vz: (Math.random() - 0.5) * 0.15
            });

            const col = colorPalette[Math.floor(Math.random() * colorPalette.length)];
            colors[i * 3] = col.r;
            colors[i * 3 + 1] = col.g;
            colors[i * 3 + 2] = col.b;
        }

        geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
        geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

        // Circular glow particle texture
        const pCanvas = document.createElement('canvas');
        pCanvas.width = 32;
        pCanvas.height = 32;
        const pCtx = pCanvas.getContext('2d');
        const grad = pCtx.createRadialGradient(16, 16, 0, 16, 16, 16);
        grad.addColorStop(0, 'rgba(255,255,255,1)');
        grad.addColorStop(0.35, 'rgba(56,189,248,0.85)');
        grad.addColorStop(1, 'rgba(56,189,248,0)');
        pCtx.fillStyle = grad;
        pCtx.fillRect(0, 0, 32, 32);
        const pTexture = new THREE.CanvasTexture(pCanvas);

        const pMaterial = new THREE.PointsMaterial({
            size: 3.5,
            map: pTexture,
            transparent: true,
            vertexColors: true,
            opacity: 0.85,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });

        const pointCloud = new THREE.Points(geometry, pMaterial);
        scene.add(pointCloud);

        // Lines linking nearby nodes
        const lineGeo = new THREE.BufferGeometry();
        const linePos = new Float32Array(particleCount * 6 * 3);
        lineGeo.setAttribute('position', new THREE.BufferAttribute(linePos, 3));
        const lineMat = new THREE.LineBasicMaterial({
            color: 0x38bdf8,
            transparent: true,
            opacity: 0.18,
            blending: THREE.AdditiveBlending
        });
        const lineMesh = new THREE.LineSegments(lineGeo, lineMat);
        scene.add(lineMesh);

        // ---------------------------------------------------------------------
        // B. 3D CYBER DOLPHIN GEOMETRY & HOLOGRAM MESHES
        // ---------------------------------------------------------------------
        function createCyberDolphinGeometry() {
            const ringCount = 28;
            const ringSegments = 16;
            const totalLength = 32.0;

            function getProfile(t) {
                let rx, ry, yCenter;
                if (t < 0.08) {
                    const k = t / 0.08;
                    rx = 0.35 + k * 0.45;
                    ry = 0.30 + k * 0.40;
                    yCenter = -0.1 + k * 0.1;
                } else if (t < 0.22) {
                    const k = (t - 0.08) / 0.14;
                    rx = 0.8 + k * 1.5;
                    ry = 0.7 + k * 1.85;
                    yCenter = Math.sin(k * Math.PI) * 0.58;
                } else if (t < 0.50) {
                    const k = (t - 0.22) / 0.28;
                    rx = 2.3 + Math.sin(k * Math.PI * 0.5) * 0.55;
                    ry = 2.55 + Math.sin(k * Math.PI * 0.5) * 0.42;
                    yCenter = 0.25 - k * 0.2;
                } else if (t < 0.85) {
                    const k = (t - 0.50) / 0.35;
                    rx = 2.85 - k * 2.15;
                    ry = 2.95 - k * 1.95;
                    yCenter = 0.05 - k * 0.05;
                } else {
                    const k = (t - 0.85) / 0.15;
                    rx = 0.7 - k * 0.35;
                    ry = 1.0 - k * 0.45;
                    yCenter = 0.0;
                }
                return { rx, ry, yCenter };
            }

            const positions = [];
            const basePositions = [];
            const uvs = [];
            const indices = [];

            for (let r = 0; r <= ringCount; r++) {
                const t = r / ringCount;
                const z = 16.0 - t * totalLength;
                const { rx, ry, yCenter } = getProfile(t);

                for (let s = 0; s < ringSegments; s++) {
                    const angle = (s / ringSegments) * Math.PI * 2;
                    const x = Math.cos(angle) * rx;
                    const y = Math.sin(angle) * ry + yCenter;

                    positions.push(x, y, z);
                    basePositions.push(x, y, z);
                    uvs.push(s / ringSegments, t);
                }
            }

            for (let r = 0; r < ringCount; r++) {
                for (let s = 0; s < ringSegments; s++) {
                    const nextS = (s + 1) % ringSegments;
                    const curRow = r * ringSegments;
                    const nextRow = (r + 1) * ringSegments;

                    const i0 = curRow + s;
                    const i1 = curRow + nextS;
                    const i2 = nextRow + s;
                    const i3 = nextRow + nextS;

                    indices.push(i0, i2, i1);
                    indices.push(i1, i2, i3);
                }
            }

            // Nose tip cap
            const noseIdx = positions.length / 3;
            positions.push(0, -0.05, 16.5);
            basePositions.push(0, -0.05, 16.5);
            uvs.push(0.5, 0);

            for (let s = 0; s < ringSegments; s++) {
                const nextS = (s + 1) % ringSegments;
                indices.push(noseIdx, s, nextS);
            }

            function addFin(verts, faces) {
                const offset = positions.length / 3;
                for (let i = 0; i < verts.length; i += 3) {
                    positions.push(verts[i], verts[i + 1], verts[i + 2]);
                    basePositions.push(verts[i], verts[i + 1], verts[i + 2]);
                    uvs.push(0, 0);
                }
                for (let i = 0; i < faces.length; i++) {
                    indices.push(faces[i] + offset);
                }
            }

            // Dorsal Fin (curved back top fin)
            const dorsalVerts = [
                0, 2.7, 0.5,
                0, 2.6, -2.5,
                0, 2.2, -5.5,
                0, 4.3, -2.0,
                0, 4.0, -4.5,
                0, 5.8, -4.2
            ];
            const dorsalFaces = [
                0, 3, 1, 1, 3, 4, 1, 4, 2, 3, 5, 4,
                0, 1, 3, 1, 4, 3, 1, 2, 4, 3, 4, 5
            ];
            addFin(dorsalVerts, dorsalFaces);

            // Left Pectoral Flipper
            const leftFlipVerts = [
                2.2, -0.6, 5.0,
                2.4, -0.8, 2.0,
                5.4, -1.8, 2.5,
                5.0, -1.9, 0.5,
                7.5, -2.6, 0.2
            ];
            const leftFlipFaces = [
                0, 2, 1, 1, 2, 3, 2, 4, 3,
                0, 1, 2, 1, 3, 2, 2, 3, 4
            ];
            addFin(leftFlipVerts, leftFlipFaces);

            // Right Pectoral Flipper (mirrored X)
            const rightFlipVerts = leftFlipVerts.map((v, i) => (i % 3 === 0 ? -v : v));
            addFin(rightFlipVerts, leftFlipFaces);

            // Horizontal Tail Flukes (Caudal Fin)
            const flukesVerts = [
                0, 0.0, -16.0,
                0, 0.0, -18.5,
                0, 0.0, -20.5,
                -2.5, 0.1, -18.0,
                -5.8, 0.15, -20.2,
                -4.0, 0.05, -20.5,
                2.5, 0.1, -18.0,
                5.8, 0.15, -20.2,
                4.0, 0.05, -20.5
            ];
            const flukesFaces = [
                0, 3, 1, 1, 3, 5, 3, 4, 5, 1, 5, 2,
                0, 1, 3, 1, 5, 3, 3, 5, 4, 1, 2, 5,
                0, 1, 6, 1, 8, 6, 6, 8, 7, 1, 2, 8,
                0, 6, 1, 1, 6, 8, 6, 7, 8, 1, 8, 2
            ];
            addFin(flukesVerts, flukesFaces);

            const geo = new THREE.BufferGeometry();
            geo.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
            geo.setAttribute('uv', new THREE.Float32BufferAttribute(uvs, 2));
            geo.setIndex(indices);
            geo.computeVertexNormals();

            return { geometry: geo, basePositions: new Float32Array(basePositions) };
        }

        const { geometry: dolphinGeo, basePositions: dolphinBasePos } = createCyberDolphinGeometry();
        const dolphinVertexCount = dolphinGeo.attributes.position.count;

        const dolphinGroup = new THREE.Group();

        // 1. Hologram Emerald Wireframe Shell
        const dolphinWireMat = new THREE.MeshBasicMaterial({
            color: 0x10b981, // Emerald neon
            wireframe: true,
            transparent: true,
            opacity: 0.60,
            blending: THREE.AdditiveBlending
        });
        const dolphinWireMesh = new THREE.Mesh(dolphinGeo, dolphinWireMat);
        dolphinGroup.add(dolphinWireMesh);

        // 2. Translucent Inner Energy Body
        const dolphinInnerMat = new THREE.MeshBasicMaterial({
            color: 0x059669,
            wireframe: false,
            transparent: true,
            opacity: 0.18,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        const dolphinInnerMesh = new THREE.Mesh(dolphinGeo, dolphinInnerMat);
        dolphinGroup.add(dolphinInnerMesh);

        // Initial Responsive Scale
        if (width <= 768) {
            dolphinGroup.visible = false;
        } else if (width <= 1024) {
            dolphinGroup.visible = true;
            dolphinGroup.scale.set(0.72, 0.72, 0.72);
        } else {
            dolphinGroup.visible = true;
            dolphinGroup.scale.set(0.95, 0.95, 0.95);
        }

        scene.add(dolphinGroup);

        // ---------------------------------------------------------------------
        // C. DATA BUBBLES / PARTICLE TRAIL BEHIND DOLPHIN
        // ---------------------------------------------------------------------
        const bubbleCount = 90;
        const bubbleGeo = new THREE.BufferGeometry();
        const bubblePositions = new Float32Array(bubbleCount * 3);
        const bubbleColors = new Float32Array(bubbleCount * 3);

        const bubbles = [];
        for (let i = 0; i < bubbleCount; i++) {
            bubblePositions[i * 3] = 0;
            bubblePositions[i * 3 + 1] = -9999; // hide until emitted
            bubblePositions[i * 3 + 2] = 0;

            bubbleColors[i * 3] = 0.2;     // R
            bubbleColors[i * 3 + 1] = 0.95; // G (emerald)
            bubbleColors[i * 3 + 2] = 0.85; // B (cyan)

            bubbles.push({
                alive: false,
                x: 0, y: 0, z: 0,
                vx: 0, vy: 0, vz: 0,
                life: 0,
                maxLife: 1.6
            });
        }

        bubbleGeo.setAttribute('position', new THREE.BufferAttribute(bubblePositions, 3));
        bubbleGeo.setAttribute('color', new THREE.BufferAttribute(bubbleColors, 3));

        // Circular bubble texture with glowing rim
        const bCanvas = document.createElement('canvas');
        bCanvas.width = 32;
        bCanvas.height = 32;
        const bCtx = bCanvas.getContext('2d');
        const bGrad = bCtx.createRadialGradient(16, 16, 0, 16, 16, 16);
        bGrad.addColorStop(0, 'rgba(255, 255, 255, 0.95)');
        bGrad.addColorStop(0.4, 'rgba(52, 211, 153, 0.8)');
        bGrad.addColorStop(0.8, 'rgba(56, 189, 248, 0.4)');
        bGrad.addColorStop(1, 'rgba(16, 185, 129, 0)');
        bCtx.fillStyle = bGrad;
        bCtx.fillRect(0, 0, 32, 32);
        const bTexture = new THREE.CanvasTexture(bCanvas);

        const bubbleMat = new THREE.PointsMaterial({
            size: 1.6,
            map: bTexture,
            transparent: true,
            vertexColors: true,
            opacity: 0.85,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });

        const bubbleParticles = new THREE.Points(bubbleGeo, bubbleMat);
        scene.add(bubbleParticles);

        let bubbleSpawnIdx = 0;
        function spawnBubble(origin) {
            const b = bubbles[bubbleSpawnIdx];
            b.alive = true;
            b.x = origin.x + (Math.random() - 0.5) * 1.8;
            b.y = origin.y + (Math.random() - 0.5) * 1.8;
            b.z = origin.z + (Math.random() - 0.5) * 1.8;
            b.vx = (Math.random() - 0.5) * 0.35;
            b.vy = 0.12 + Math.random() * 0.25; // buoyant upward drift
            b.vz = (Math.random() - 0.5) * 0.35;
            b.life = 0;
            b.maxLife = 1.2 + Math.random() * 0.9;
            bubbleSpawnIdx = (bubbleSpawnIdx + 1) % bubbleCount;
        }

        // ---------------------------------------------------------------------
        // D. SCROLL FLY-THROUGH & INTERACTION DYNAMICS
        // ---------------------------------------------------------------------
        let targetCameraZ = 100;
        let mouseX = 0;
        let mouseY = 0;
        let targetMouseX = 0;
        let targetMouseY = 0;

        let lastScrollY = window.scrollY;
        let scrollVelocity = 0;
        let dolphinTime = 0;
        let swimCycle = 0;
        let currentPitch = 0;
        let frameCount = 0;

        const onScroll = () => {
            const maxScroll = (document.documentElement.scrollHeight - window.innerHeight) || 1;
            const progress = Math.min(Math.max(window.scrollY / maxScroll, 0), 1);
            // Z-axis camera fly-through: camera moves smoothly through deep cyber space
            targetCameraZ = 100 - progress * 550;
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();

        const onMouseMove = (e) => {
            targetMouseX = (e.clientX / window.innerWidth - 0.5) * 2;
            targetMouseY = (e.clientY / window.innerHeight - 0.5) * 2;
        };
        window.addEventListener('mousemove', onMouseMove, { passive: true });

        const onResize = () => {
            width = canvas.width = window.innerWidth;
            height = canvas.height = window.innerHeight;
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
            renderer.setSize(width, height);

            // Responsive Visibility for Cyber Dolphin
            if (width <= 768) {
                dolphinGroup.visible = false;
                bubbleParticles.visible = false;
            } else if (width <= 1024) {
                dolphinGroup.visible = true;
                bubbleParticles.visible = true;
                dolphinGroup.scale.set(0.72, 0.72, 0.72);
            } else {
                dolphinGroup.visible = true;
                bubbleParticles.visible = true;
                dolphinGroup.scale.set(1.15, 1.15, 1.15);
            }
        };
        window.addEventListener('resize', onResize);

        // Trajectory calculator: Smooth 3D Lemniscate / Figure-8
        function getDolphinPoint(t) {
            const zRel = -85 + Math.cos(t * 0.8) * 20;
            const z = camera.position.z + zRel;
            const viewHalfW = Math.tan(30 * Math.PI / 180) * Math.abs(zRel) * camera.aspect;
            const x = Math.sin(t) * (viewHalfW * 0.82);
            const y = Math.sin(2 * t) * 11.5 + Math.cos(t * 0.5) * 4.5;
            return new THREE.Vector3(x, y, z);
        }

        let animId = null;

        function animate() {
            // Track scroll acceleration
            const curScroll = window.scrollY;
            const deltaScroll = curScroll - lastScrollY;
            lastScrollY = curScroll;
            scrollVelocity += (deltaScroll - scrollVelocity) * 0.12;

            // Speed dynamics: Swimming accelerates with user scroll activity
            const scrollBoost = Math.min(Math.abs(scrollVelocity) * 0.035, 2.2);
            const cruiseSpeed = 0.32 + scrollBoost;
            dolphinTime += cruiseSpeed * 0.016;
            swimCycle += (4.2 + scrollBoost * 2.8) * 0.016;

            // Smooth Camera Easing
            camera.position.z += (targetCameraZ - camera.position.z) * 0.06;
            mouseX += (targetMouseX * 18 - mouseX) * 0.04;
            mouseY += (-targetMouseY * 14 - mouseY) * 0.04;
            camera.position.x = mouseX;
            camera.position.y = mouseY;

            // Move Background Constellation Particles
            const pos = geometry.attributes.position.array;
            let lineIdx = 0;
            const linePositions = lineGeo.attributes.position.array;

            for (let i = 0; i < particleCount; i++) {
                pos[i * 3] += velocities[i].vx;
                pos[i * 3 + 1] += velocities[i].vy;
                pos[i * 3 + 2] += velocities[i].vz;

                if (pos[i * 3] < -140) pos[i * 3] = 140;
                if (pos[i * 3] > 140) pos[i * 3] = -140;
                if (pos[i * 3 + 1] < -110) pos[i * 3 + 1] = 110;
                if (pos[i * 3 + 1] > 110) pos[i * 3 + 1] = -110;
                if (pos[i * 3 + 2] < -850) pos[i * 3 + 2] = 100;
                if (pos[i * 3 + 2] > 100) pos[i * 3 + 2] = -850;

                for (let j = i + 1; j < Math.min(i + 8, particleCount); j++) {
                    const dx = pos[i * 3] - pos[j * 3];
                    const dy = pos[i * 3 + 1] - pos[j * 3 + 1];
                    const dz = pos[i * 3 + 2] - pos[j * 3 + 2];
                    const distSq = dx * dx + dy * dy + dz * dz;

                    if (distSq < 1600 && lineIdx < linePositions.length - 6) {
                        linePositions[lineIdx++] = pos[i * 3];
                        linePositions[lineIdx++] = pos[i * 3 + 1];
                        linePositions[lineIdx++] = pos[i * 3 + 2];
                        linePositions[lineIdx++] = pos[j * 3];
                        linePositions[lineIdx++] = pos[j * 3 + 1];
                        linePositions[lineIdx++] = pos[j * 3 + 2];
                    }
                }
            }

            geometry.attributes.position.needsUpdate = true;
            lineGeo.setDrawRange(0, lineIdx / 3);
            lineGeo.attributes.position.needsUpdate = true;

            // -----------------------------------------------------------------
            // CYBER DOLPHIN TRAJECTORY & SWIMMING PHYSICS
            // -----------------------------------------------------------------
            if (dolphinGroup.visible) {
                const pCurr = getDolphinPoint(dolphinTime);
                const pNext = getDolphinPoint(dolphinTime + 0.05);

                dolphinGroup.position.copy(pCurr);
                dolphinGroup.lookAt(pNext);

                // Bank / Roll into curve
                const bankAngle = -Math.cos(dolphinTime) * 0.28;
                dolphinGroup.rotateZ(bankAngle);

                // Dynamic Pitch based on scroll velocity (diving or breaching)
                const targetPitch = -Math.max(-0.25, Math.min(0.25, scrollVelocity * 0.012));
                currentPitch += (targetPitch - currentPitch) * 0.1;
                dolphinGroup.rotateX(currentPitch);

                // Spine undulation (traveling wave down caudal peduncle to flukes)
                const dPos = dolphinGeo.attributes.position.array;
                for (let i = 0; i < dolphinVertexCount; i++) {
                    const z0 = dolphinBasePos[i * 3 + 2];
                    const tailFactor = Math.max(0, Math.min(1, (3.5 - z0) / 22));
                    const wave = Math.sin(swimCycle * 5.0 + z0 * 0.24) * (tailFactor * tailFactor * 2.8);
                    const sway = Math.cos(swimCycle * 5.0 * 0.5 + z0 * 0.16) * (tailFactor * 0.7);

                    dPos[i * 3] = dolphinBasePos[i * 3] + sway;
                    dPos[i * 3 + 1] = dolphinBasePos[i * 3 + 1] + wave;
                    dPos[i * 3 + 2] = dolphinBasePos[i * 3 + 2];
                }
                dolphinGeo.attributes.position.needsUpdate = true;

                // Emit luminous bubble trail from tail & blowhole
                frameCount++;
                if (frameCount % 2 === 0) {
                    dolphinGroup.updateMatrixWorld();
                    const tailWorld = new THREE.Vector3(0, 0, -18.5).applyMatrix4(dolphinGroup.matrixWorld);
                    spawnBubble(tailWorld);

                    if (frameCount % 6 === 0) {
                        const blowWorld = new THREE.Vector3(0, 2.4, 4.0).applyMatrix4(dolphinGroup.matrixWorld);
                        spawnBubble(blowWorld);
                    }
                }

                // Update active bubbles
                const bPosArr = bubbleGeo.attributes.position.array;
                for (let i = 0; i < bubbleCount; i++) {
                    const b = bubbles[i];
                    if (b.alive) {
                        b.x += b.vx;
                        b.y += b.vy;
                        b.z += b.vz;
                        b.life += 0.016;

                        if (b.life >= b.maxLife) {
                            b.alive = false;
                            bPosArr[i * 3 + 1] = -9999;
                        } else {
                            bPosArr[i * 3] = b.x;
                            bPosArr[i * 3 + 1] = b.y;
                            bPosArr[i * 3 + 2] = b.z;
                        }
                    }
                }
                bubbleGeo.attributes.position.needsUpdate = true;
            }

            renderer.render(scene, camera);
            animId = requestAnimationFrame(animate);
        }

        animate();

        return () => {
            window.removeEventListener('scroll', onScroll);
            window.removeEventListener('mousemove', onMouseMove);
            window.removeEventListener('resize', onResize);
            if (animId) cancelAnimationFrame(animId);

            geometry.dispose();
            pMaterial.dispose();
            lineGeo.dispose();
            lineMat.dispose();
            pTexture.dispose();

            dolphinGeo.dispose();
            dolphinWireMat.dispose();
            dolphinInnerMat.dispose();

            bubbleGeo.dispose();
            bubbleMat.dispose();
            bTexture.dispose();

            renderer.dispose();
        };
    }

    /* =========================================================================
       2. HERO SECTION 3D HOLOGRAPHIC SUPPLY CHAIN GLOBE & FLOATING FMCG CUBES
       ========================================================================= */
    function initHero3DScene() {
        const canvas = document.getElementById('hero-3d-scene');
        const container = document.getElementById('hero-3d-container') || (canvas ? canvas.parentElement : null);
        if (!canvas || !container) return null;

        let width = container.clientWidth || 540;
        let height = container.clientHeight || 440;

        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(42, width / height, 0.1, 100);
        camera.position.set(0, 0.6, 7.6);

        const renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            alpha: true,
            antialias: true,
            powerPreference: 'high-performance'
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

        // Group holding the entire interactive 3D supply chain universe
        const heroGroup = new THREE.Group();
        scene.add(heroGroup);

        // A. Central 3D Holographic Supply Chain Globe
        // Outer Wireframe Globe (Cyan Neon)
        const outerGlobeGeo = new THREE.IcosahedronGeometry(1.85, 2);
        const outerGlobeMat = new THREE.MeshBasicMaterial({
            color: 0x38bdf8,
            wireframe: true,
            transparent: true,
            opacity: 0.45
        });
        const outerGlobe = new THREE.Mesh(outerGlobeGeo, outerGlobeMat);
        heroGroup.add(outerGlobe);

        // Inner Core Sphere (Glowing Emerald)
        const innerCoreGeo = new THREE.SphereGeometry(1.25, 20, 20);
        const innerCoreMat = new THREE.MeshStandardMaterial({
            color: 0x059669,
            emissive: 0x10b981,
            emissiveIntensity: 0.75,
            roughness: 0.25,
            metalness: 0.85,
            transparent: true,
            opacity: 0.8
        });
        const innerCore = new THREE.Mesh(innerCoreGeo, innerCoreMat);
        heroGroup.add(innerCore);

        // Planetary Orbit Rings
        const ringMat1 = new THREE.MeshBasicMaterial({
            color: 0x10b981,
            wireframe: true,
            transparent: true,
            opacity: 0.55
        });
        const ring1 = new THREE.Mesh(new THREE.TorusGeometry(2.5, 0.02, 6, 64), ringMat1);
        ring1.rotation.x = Math.PI * 0.35;
        ring1.rotation.y = Math.PI * 0.15;
        heroGroup.add(ring1);

        const ringMat2 = new THREE.MeshBasicMaterial({
            color: 0x3b82f6,
            wireframe: true,
            transparent: true,
            opacity: 0.5
        });
        const ring2 = new THREE.Mesh(new THREE.TorusGeometry(2.85, 0.02, 6, 64), ringMat2);
        ring2.rotation.x = -Math.PI * 0.25;
        ring2.rotation.z = Math.PI * 0.2;
        heroGroup.add(ring2);

        // B. Antigravity Floating FMCG Pallet Cubes & Parcel Boxes
        const floatingBoxes = [];
        const boxConfigs = [
            { size: 0.65, x: 2.2, y: 1.1, z: 0.8, color: 0x10b981, speed: 1.4, amp: 0.28, phase: 0.2 },
            { size: 0.55, x: -2.3, y: -0.8, z: 1.1, color: 0x38bdf8, speed: 1.8, amp: 0.22, phase: 1.5 },
            { size: 0.75, x: 1.9, y: -1.4, z: -0.6, color: 0xf59e0b, speed: 1.2, amp: 0.32, phase: 2.8 },
            { size: 0.50, x: -1.8, y: 1.5, z: -0.9, color: 0x8b5cf6, speed: 1.6, amp: 0.25, phase: 4.1 },
            { size: 0.60, x: 0.2, y: 2.4, z: 0.4, color: 0x10b981, speed: 1.5, amp: 0.30, phase: 5.2 },
            { size: 0.58, x: -0.5, y: -2.2, z: 0.9, color: 0x3b82f6, speed: 1.7, amp: 0.26, phase: 3.4 }
        ];

        boxConfigs.forEach((cfg) => {
            const bGeo = new THREE.BoxGeometry(cfg.size, cfg.size, cfg.size);
            const bMat = new THREE.MeshStandardMaterial({
                color: 0x09142e,
                emissive: cfg.color,
                emissiveIntensity: 0.45,
                metalness: 0.85,
                roughness: 0.2,
                transparent: true,
                opacity: 0.92
            });
            const box = new THREE.Mesh(bGeo, bMat);

            // Add Glowing Neon Edges
            const edges = new THREE.EdgesGeometry(bGeo);
            const edgeMat = new THREE.LineBasicMaterial({ color: cfg.color, linewidth: 2 });
            const edgeLine = new THREE.LineSegments(edges, edgeMat);
            box.add(edgeLine);

            box.position.set(cfg.x, cfg.y, cfg.z);
            box.userData = {
                y0: cfg.y,
                x0: cfg.x,
                z0: cfg.z,
                speed: cfg.speed,
                amp: cfg.amp,
                phase: cfg.phase,
                rotSpeedX: 0.01 + Math.random() * 0.015,
                rotSpeedY: 0.015 + Math.random() * 0.02
            };

            floatingBoxes.push(box);
            heroGroup.add(box);
        });

        // C. Orbiting Golden/Cyan Data Packets
        const particleCount = 36;
        const particleGeo = new THREE.BufferGeometry();
        const pPositions = new Float32Array(particleCount * 3);
        const pOrbits = [];

        for (let i = 0; i < particleCount; i++) {
            const rad = 2.1 + Math.random() * 1.2;
            const theta = Math.random() * Math.PI * 2;
            const phi = (Math.random() - 0.5) * Math.PI * 0.8;
            pOrbits.push({ rad, theta, phi, speed: 0.008 + Math.random() * 0.012 });

            pPositions[i * 3] = rad * Math.cos(theta) * Math.cos(phi);
            pPositions[i * 3 + 1] = rad * Math.sin(phi);
            pPositions[i * 3 + 2] = rad * Math.sin(theta) * Math.cos(phi);
        }

        particleGeo.setAttribute('position', new THREE.BufferAttribute(pPositions, 3));
        const particleMat = new THREE.PointsMaterial({
            color: 0x38bdf8,
            size: 0.14,
            transparent: true,
            opacity: 0.9,
            blending: THREE.AdditiveBlending
        });
        const orbitParticles = new THREE.Points(particleGeo, particleMat);
        heroGroup.add(orbitParticles);

        // Lighting
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
        scene.add(ambientLight);

        const dirLight = new THREE.DirectionalLight(0x38bdf8, 2.2);
        dirLight.position.set(5, 6, 5);
        scene.add(dirLight);

        const pointLight = new THREE.PointLight(0x10b981, 3.5, 12);
        pointLight.position.set(0, 0, 0);
        scene.add(pointLight);

        // Interactivity & Mouse Damping / Drag
        let isDragging = false;
        let prevMouseX = 0;
        let prevMouseY = 0;
        let targetRotY = 0.25;
        let targetRotX = 0.15;
        let hoverX = 0;
        let hoverY = 0;

        const onPointerDown = (e) => {
            isDragging = true;
            prevMouseX = e.clientX || (e.touches && e.touches[0].clientX);
            prevMouseY = e.clientY || (e.touches && e.touches[0].clientY);
        };

        const onPointerMove = (e) => {
            const clientX = e.clientX || (e.touches && e.touches[0].clientX) || 0;
            const clientY = e.clientY || (e.touches && e.touches[0].clientY) || 0;

            const rect = container.getBoundingClientRect();
            hoverX = ((clientX - rect.left) / rect.width - 0.5) * 2;
            hoverY = ((clientY - rect.top) / rect.height - 0.5) * 2;

            if (isDragging) {
                const deltaX = clientX - prevMouseX;
                const deltaY = clientY - prevMouseY;
                targetRotY += deltaX * 0.008;
                targetRotX += deltaY * 0.008;
                prevMouseX = clientX;
                prevMouseY = clientY;
            }
        };

        const onPointerUp = () => {
            isDragging = false;
        };

        container.addEventListener('mousedown', onPointerDown);
        window.addEventListener('mousemove', onPointerMove, { passive: true });
        window.addEventListener('mouseup', onPointerUp);

        container.addEventListener('touchstart', onPointerDown, { passive: true });
        window.addEventListener('touchmove', onPointerMove, { passive: true });
        window.addEventListener('touchend', onPointerUp);

        const onResize = () => {
            if (!container) return;
            width = container.clientWidth || 540;
            height = container.clientHeight || 440;
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
            renderer.setSize(width, height);
        };
        window.addEventListener('resize', onResize);

        let animId = null;
        let clock = new THREE.Clock();

        function animate() {
            const time = clock.getElapsedTime();

            // Auto subtle rotation + mouse drag damping
            if (!isDragging) {
                targetRotY += 0.0025;
            }

            const activeTargetX = targetRotX + hoverY * 0.35;
            const activeTargetY = targetRotY + hoverX * 0.35;

            heroGroup.rotation.y += (activeTargetY - heroGroup.rotation.y) * 0.06;
            heroGroup.rotation.x += (activeTargetX - heroGroup.rotation.x) * 0.06;

            // Globe Pulsation & Internal rotations
            outerGlobe.rotation.y = time * 0.15;
            outerGlobe.rotation.z = time * 0.08;
            ring1.rotation.z = time * 0.25;
            ring2.rotation.y = -time * 0.2;

            const scalePulse = 1 + Math.sin(time * 2.2) * 0.035;
            innerCore.scale.set(scalePulse, scalePulse, scalePulse);

            // Antigravity Box Physics (Harmonic Floating + Tumble)
            floatingBoxes.forEach((box) => {
                const u = box.userData;
                box.position.y = u.y0 + Math.sin(time * u.speed + u.phase) * u.amp;
                box.position.x = u.x0 + Math.cos(time * (u.speed * 0.6) + u.phase) * (u.amp * 0.4);
                box.rotation.x += u.rotSpeedX;
                box.rotation.y += u.rotSpeedY;
            });

            // Orbiting Data Packets
            const pArr = particleGeo.attributes.position.array;
            for (let i = 0; i < particleCount; i++) {
                const orb = pOrbits[i];
                orb.theta += orb.speed;
                pArr[i * 3] = orb.rad * Math.cos(orb.theta) * Math.cos(orb.phi);
                pArr[i * 3 + 1] = orb.rad * Math.sin(orb.phi) + Math.sin(time * 1.5 + i) * 0.15;
                pArr[i * 3 + 2] = orb.rad * Math.sin(orb.theta) * Math.cos(orb.phi);
            }
            particleGeo.attributes.position.needsUpdate = true;

            renderer.render(scene, camera);
            animId = requestAnimationFrame(animate);
        }

        animate();

        return () => {
            container.removeEventListener('mousedown', onPointerDown);
            window.removeEventListener('mousemove', onPointerMove);
            window.removeEventListener('mouseup', onPointerUp);
            container.removeEventListener('touchstart', onPointerDown);
            window.removeEventListener('touchmove', onPointerMove);
            window.removeEventListener('touchend', onPointerUp);
            window.removeEventListener('resize', onResize);
            if (animId) cancelAnimationFrame(animId);

            outerGlobeGeo.dispose();
            outerGlobeMat.dispose();
            innerCoreGeo.dispose();
            innerCoreMat.dispose();
            ringMat1.dispose();
            ringMat2.dispose();
            floatingBoxes.forEach(b => {
                b.geometry.dispose();
                b.material.dispose();
            });
            particleGeo.dispose();
            particleMat.dispose();
            renderer.dispose();
        };
    }

    /* =========================================================================
       3. SUPPLY NETWORK 3D ISOMETRIC NODES & AUTOMATED DATA FLOW (FIFO)
       ========================================================================= */
    function initSupply3DScene() {
        const canvas = document.getElementById('supply-3d-scene');
        const container = document.getElementById('supply-3d-container') || (canvas ? canvas.parentElement : null);
        if (!canvas || !container) return null;

        let width = container.clientWidth || 450;
        let height = container.clientHeight || 280;

        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(40, width / height, 0.1, 100);
        camera.position.set(0, 2.8, 8.2);
        camera.lookAt(0, 0, 0);

        const renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            alpha: true,
            antialias: true,
            powerPreference: 'high-performance'
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));

        const supplyGroup = new THREE.Group();
        scene.add(supplyGroup);

        // Node A: Kho Hàng Sỉ (Left) - Stood Pallet Warehouse Nodes
        const nodeAGroup = new THREE.Group();
        nodeAGroup.position.set(-2.5, -0.3, 0);

        const boxGeoA = new THREE.BoxGeometry(0.7, 0.45, 0.7);
        const boxMatA = new THREE.MeshStandardMaterial({
            color: 0x0e2452,
            emissive: 0x3b82f6,
            emissiveIntensity: 0.5,
            metalness: 0.8,
            roughness: 0.2
        });
        const box1A = new THREE.Mesh(boxGeoA, boxMatA);
        const box2A = new THREE.Mesh(boxGeoA, boxMatA);
        box2A.position.set(0, 0.45, 0);
        box2A.scale.set(0.85, 0.85, 0.85);

        const edgeA1 = new THREE.LineSegments(new THREE.EdgesGeometry(boxGeoA), new THREE.LineBasicMaterial({ color: 0x60a5fa }));
        box1A.add(edgeA1);
        const edgeA2 = new THREE.LineSegments(new THREE.EdgesGeometry(boxGeoA), new THREE.LineBasicMaterial({ color: 0x93c5fd }));
        box2A.add(edgeA2);

        nodeAGroup.add(box1A);
        nodeAGroup.add(box2A);

        // Base Hologram Ring Node A
        const baseRingA = new THREE.Mesh(new THREE.TorusGeometry(0.75, 0.02, 4, 32), new THREE.MeshBasicMaterial({ color: 0x3b82f6 }));
        baseRingA.rotation.x = Math.PI / 2;
        baseRingA.position.y = -0.25;
        nodeAGroup.add(baseRingA);
        supplyGroup.add(nodeAGroup);

        // Node B: Cổng Đại Lý VietQR (Center/Top) - Hexagonal Terminal Node
        const nodeBGroup = new THREE.Group();
        nodeBGroup.position.set(0, 0.9, 0.3);

        const cylGeoB = new THREE.CylinderGeometry(0.65, 0.75, 0.35, 6);
        const cylMatB = new THREE.MeshStandardMaterial({
            color: 0x064e3b,
            emissive: 0x10b981,
            emissiveIntensity: 0.65,
            metalness: 0.8,
            roughness: 0.2
        });
        const cylB = new THREE.Mesh(cylGeoB, cylMatB);
        const edgeB = new THREE.LineSegments(new THREE.EdgesGeometry(cylGeoB), new THREE.LineBasicMaterial({ color: 0x34d399, linewidth: 2 }));
        cylB.add(edgeB);
        nodeBGroup.add(cylB);

        // QR Hologram Scan Cone
        const coneGeoB = new THREE.ConeGeometry(0.55, 0.75, 4, 1, true);
        const coneMatB = new THREE.MeshBasicMaterial({ color: 0x10b981, wireframe: true, transparent: true, opacity: 0.4 });
        const scanCone = new THREE.Mesh(coneGeoB, coneMatB);
        scanCone.position.y = 0.55;
        scanCone.rotation.x = Math.PI;
        nodeBGroup.add(scanCone);
        supplyGroup.add(nodeBGroup);

        // Node C: Sổ Cái Kế Toán FIFO (Right) - Stacked Database Disks
        const nodeCGroup = new THREE.Group();
        nodeCGroup.position.set(2.5, -0.3, 0);

        const diskGeoC = new THREE.CylinderGeometry(0.55, 0.55, 0.2, 24);
        const diskMatC = new THREE.MeshStandardMaterial({
            color: 0x78350f,
            emissive: 0xf59e0b,
            emissiveIntensity: 0.55,
            metalness: 0.8,
            roughness: 0.2
        });

        for (let i = 0; i < 3; i++) {
            const disk = new THREE.Mesh(diskGeoC, diskMatC);
            disk.position.y = i * 0.26;
            const diskEdge = new THREE.LineSegments(new THREE.EdgesGeometry(diskGeoC), new THREE.LineBasicMaterial({ color: 0xfbbf24 }));
            disk.add(diskEdge);
            nodeCGroup.add(disk);
        }

        const baseRingC = new THREE.Mesh(new THREE.TorusGeometry(0.75, 0.02, 4, 32), new THREE.MeshBasicMaterial({ color: 0xf59e0b }));
        baseRingC.rotation.x = Math.PI / 2;
        baseRingC.position.y = -0.25;
        nodeCGroup.add(baseRingC);
        supplyGroup.add(nodeCGroup);

        // Curved Data Flow Splines A -> B and B -> C
        const curveAB = new THREE.QuadraticBezierCurve3(
            new THREE.Vector3(-2.5, 0.3, 0),
            new THREE.Vector3(-1.25, 1.4, 0.2),
            new THREE.Vector3(0, 0.9, 0.3)
        );

        const curveBC = new THREE.QuadraticBezierCurve3(
            new THREE.Vector3(0, 0.9, 0.3),
            new THREE.Vector3(1.25, 1.4, 0.2),
            new THREE.Vector3(2.5, 0.3, 0)
        );

        // Visual Tubing Lines
        const tubeGeoAB = new THREE.BufferGeometry().setFromPoints(curveAB.getPoints(30));
        const tubeGeoBC = new THREE.BufferGeometry().setFromPoints(curveBC.getPoints(30));
        const lineMatAB = new THREE.LineBasicMaterial({ color: 0x38bdf8, transparent: true, opacity: 0.45 });
        const lineMatBC = new THREE.LineBasicMaterial({ color: 0x10b981, transparent: true, opacity: 0.45 });
        supplyGroup.add(new THREE.Line(tubeGeoAB, lineMatAB));
        supplyGroup.add(new THREE.Line(tubeGeoBC, lineMatBC));

        // Pulsing Traveling Energy Packets
        const packetCount = 18;
        const packetGeo = new THREE.BufferGeometry();
        const packetPos = new Float32Array(packetCount * 3);
        packetGeo.setAttribute('position', new THREE.BufferAttribute(packetPos, 3));
        const packetMat = new THREE.PointsMaterial({
            color: 0x67e8f9,
            size: 0.16,
            transparent: true,
            opacity: 0.95,
            blending: THREE.AdditiveBlending
        });
        const packetPoints = new THREE.Points(packetGeo, packetMat);
        supplyGroup.add(packetPoints);

        // Lighting
        const ambLight = new THREE.AmbientLight(0xffffff, 0.85);
        scene.add(ambLight);

        const lightA = new THREE.PointLight(0x3b82f6, 2.5, 6);
        lightA.position.set(-2.5, 1.5, 2);
        scene.add(lightA);

        const lightB = new THREE.PointLight(0x10b981, 3.2, 6);
        lightB.position.set(0, 2.2, 2);
        scene.add(lightB);

        const lightC = new THREE.PointLight(0xf59e0b, 2.5, 6);
        lightC.position.set(2.5, 1.5, 2);
        scene.add(lightC);

        // Mouse Parallax
        let targetTiltX = 0;
        let targetTiltY = 0;

        const onMouseMove = (e) => {
            const rect = container.getBoundingClientRect();
            targetTiltY = ((e.clientX - rect.left) / rect.width - 0.5) * 0.4;
            targetTiltX = ((e.clientY - rect.top) / rect.height - 0.5) * 0.3;
        };
        container.addEventListener('mousemove', onMouseMove, { passive: true });

        const onResize = () => {
            if (!container) return;
            width = container.clientWidth || 450;
            height = container.clientHeight || 280;
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
            renderer.setSize(width, height);
        };
        window.addEventListener('resize', onResize);

        let animId = null;
        let clock = new THREE.Clock();

        function animate() {
            const time = clock.getElapsedTime();

            // Isometric tilt damping
            supplyGroup.rotation.y += (targetTiltY - supplyGroup.rotation.y) * 0.05;
            supplyGroup.rotation.x += (targetTiltX - supplyGroup.rotation.x) * 0.05;

            // Node rotations & oscillations
            nodeAGroup.position.y = -0.3 + Math.sin(time * 1.5) * 0.05;
            nodeBGroup.position.y = 0.9 + Math.sin(time * 1.8 + 1) * 0.06;
            nodeCGroup.position.y = -0.3 + Math.sin(time * 1.6 + 2) * 0.05;

            cylB.rotation.y = time * 0.8;
            scanCone.rotation.y = -time * 1.2;

            // Travel energy packets along curves
            const pArr = packetGeo.attributes.position.array;
            for (let i = 0; i < packetCount; i++) {
                const isFirstHalf = i < packetCount / 2;
                const offset = (time * 0.45 + (i % (packetCount / 2)) / (packetCount / 2)) % 1;

                const pt = isFirstHalf ? curveAB.getPoint(offset) : curveBC.getPoint(offset);
                pArr[i * 3] = pt.x;
                pArr[i * 3 + 1] = pt.y + (Math.sin(time * 4 + i) * 0.03);
                pArr[i * 3 + 2] = pt.z;
            }
            packetGeo.attributes.position.needsUpdate = true;

            renderer.render(scene, camera);
            animId = requestAnimationFrame(animate);
        }

        animate();

        return () => {
            container.removeEventListener('mousemove', onMouseMove);
            window.removeEventListener('resize', onResize);
            if (animId) cancelAnimationFrame(animId);

            boxGeoA.dispose();
            boxMatA.dispose();
            cylGeoB.dispose();
            cylMatB.dispose();
            coneGeoB.dispose();
            coneMatB.dispose();
            diskGeoC.dispose();
            diskMatC.dispose();
            tubeGeoAB.dispose();
            tubeGeoBC.dispose();
            packetGeo.dispose();
            packetMat.dispose();
            renderer.dispose();
        };
    }

    /* =========================================================================
       4. PRODUCT CARDS 3D PERSPECTIVE TILT & DYNAMIC GLARE
       ========================================================================= */
    function initProduct3DInteractions() {
        const cards = document.querySelectorAll('.product-card');
        if (!cards.length) return;

        cards.forEach((card) => {
            // Append glare reflection overlay if not existing
            let glare = card.querySelector('.product-card-glare');
            if (!glare) {
                glare = document.createElement('div');
                glare.className = 'product-card-glare';
                card.appendChild(glare);
            }

            // Ensure dynamic ambient drop shadow exists under product stage
            const stage = card.querySelector('.product-stage');
            if (stage && !stage.querySelector('.product-ambient-shadow')) {
                const shadow = document.createElement('div');
                shadow.className = 'product-ambient-shadow';
                stage.appendChild(shadow);
            }

            const onMouseMove = (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;

                const rotX = ((y / rect.height) - 0.5) * -16;
                const rotY = ((x / rect.width) - 0.5) * 16;

                card.style.transform = `perspective(1000px) rotateX(${rotX.toFixed(2)}deg) rotateY(${rotY.toFixed(2)}deg) translateY(-8px)`;
                card.style.setProperty('--glare-x', `${(x / rect.width * 100).toFixed(1)}%`);
                card.style.setProperty('--glare-y', `${(y / rect.height * 100).toFixed(1)}%`);
            };

            const onMouseLeave = () => {
                card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(0px)';
                card.style.transition = 'transform 0.5s cubic-bezier(0.2, 0.8, 0.2, 1)';
            };

            const onMouseEnter = () => {
                card.style.transition = 'none';
            };

            card.addEventListener('mousemove', onMouseMove);
            card.addEventListener('mouseleave', onMouseLeave);
            card.addEventListener('mouseenter', onMouseEnter);
        });
    }

    /* =========================================================================
       5. HERO HUD BADGES PARALLAX
       ========================================================================= */
    function initHeroParallaxHUD() {
        const container = document.getElementById('hero-3d-container') || document.querySelector('.hero-visual-frame');
        if (!container) return;

        const pill1 = container.querySelector('.pill-1');
        const pill2 = container.querySelector('.pill-2');
        const pill3 = container.querySelector('.pill-3');

        if (!pill1 && !pill2 && !pill3) return;

        const onMouseMove = (e) => {
            const rect = container.getBoundingClientRect();
            const dx = ((e.clientX - rect.left) / rect.width - 0.5) * 2;
            const dy = ((e.clientY - rect.top) / rect.height - 0.5) * 2;

            if (pill1) pill1.style.transform = `translate3d(${dx * -16}px, ${dy * -14}px, 0)`;
            if (pill2) pill2.style.transform = `translate3d(${dx * 18}px, ${dy * 16}px, 0)`;
            if (pill3) pill3.style.transform = `translate3d(${dx * -22}px, ${dy * 20}px, 0)`;
        };

        const onMouseLeave = () => {
            if (pill1) pill1.style.transform = 'translate3d(0, 0, 0)';
            if (pill2) pill2.style.transform = 'translate3d(0, 0, 0)';
            if (pill3) pill3.style.transform = 'translate3d(0, 0, 0)';
        };

        container.addEventListener('mousemove', onMouseMove, { passive: true });
        container.addEventListener('mouseleave', onMouseLeave);
    }

    /* =========================================================================
       6. 2D CANVAS FALLBACK IF WEBGL IS DISABLED
       ========================================================================= */
    function init2DCanvasFallback() {
        const canvas = document.getElementById('dong-ambient-canvas');
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        let width = canvas.width = window.innerWidth;
        let height = canvas.height = window.innerHeight;

        const pCount = 50;
        const pArr = [];
        for (let i = 0; i < pCount; i++) {
            pArr.push({
                x: Math.random() * width,
                y: Math.random() * height,
                vx: (Math.random() - 0.5) * 0.4,
                vy: (Math.random() - 0.5) * 0.4,
                r: Math.random() * 2 + 1
            });
        }

        let id = null;
        function draw() {
            ctx.clearRect(0, 0, width, height);
            ctx.fillStyle = 'rgba(56, 189, 248, 0.4)';
            pArr.forEach(p => {
                p.x += p.vx;
                p.y += p.vy;
                if (p.x < 0) p.x = width;
                if (p.x > width) p.x = 0;
                if (p.y < 0) p.y = height;
                if (p.y > height) p.y = 0;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fill();
            });
            id = requestAnimationFrame(draw);
        }
        draw();

        threeCleanups.push(() => {
            if (id) cancelAnimationFrame(id);
        });
    }

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

    // Cleanup when Blazor disposes the component
    window.destroyDongAnimations = function () {
        if (typeof ScrollTrigger !== 'undefined') {
            ScrollTrigger.getAll().forEach(t => t.kill());
        }
        if (scrollHandler) {
            window.removeEventListener('scroll', scrollHandler);
            scrollHandler = null;
        }
        threeCleanups.forEach(fn => {
            try { fn(); } catch (e) {}
        });
        threeCleanups = [];
    };
})();
