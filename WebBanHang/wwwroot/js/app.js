/* ==========================================================================
   APP GLOBAL JAVASCRIPT - SIDEBAR CONTROLLER, SHORTCUTS, THEME & UTILITIES
   ========================================================================== */

(function () {
    // -------------------------------------------------------------------------
    // 1. Enterprise ENTERPRISE SIDEBAR CONTROLLER (AUTO-HIDE, OVERLAY, PIN, OFFCANVAS)
    // -------------------------------------------------------------------------
    const STORAGE_KEY_PINNED = 'app_sidebar_pinned';
    let hideTimer = null;
    let isPinned = false;
    let isInitialized = false;
    let lastToggleTime = 0;

    function getElements() {
        return {
            sidebar: document.getElementById('appSidebar'),
            trigger: document.getElementById('appSidebarTrigger'),
            workspace: document.getElementById('appWorkspace'),
            canvas: document.getElementById('appContentCanvas'),
            backdrop: document.getElementById('appSidebarBackdrop'),
            pinBtn: document.getElementById('appSidebarPinBtn'),
            mobileCloseBtn: document.getElementById('appSidebarMobileCloseBtn'),
            toggleBtn: document.getElementById('appSidebarToggleBtn')
        };
    }

    function isDesktop() {
        return window.innerWidth >= 992;
    }

    function initPinnedState() {
        try {
            const stored = localStorage.getItem(STORAGE_KEY_PINNED);
            // Default is unpinned (auto-hide). Only pinned if explicitly stored as 'true'.
            isPinned = (stored === 'true');
        } catch (e) {
            isPinned = false;
        }
        applyPinnedState();
    }

    function applyPinnedState() {
        const { sidebar, workspace, canvas, pinBtn } = getElements();
        if (!sidebar || !workspace) return;

        if (isDesktop() && isPinned) {
            workspace.classList.add('is-sidebar-pinned');
            sidebar.classList.add('is-pinned');
            sidebar.classList.remove('is-hover-open');
            sidebar.removeAttribute('inert');
            sidebar.setAttribute('aria-hidden', 'false');
            if (canvas) canvas.classList.add('is-pinned');
            if (pinBtn) {
                pinBtn.classList.add('active');
                pinBtn.setAttribute('title', 'Bỏ ghim menu (Ctrl+B)');
                const icon = pinBtn.querySelector('i');
                if (icon) {
                    icon.className = 'bi bi-pin-fill text-success fs-6';
                }
            }
        } else {
            workspace.classList.remove('is-sidebar-pinned');
            sidebar.classList.remove('is-pinned');
            if (canvas) canvas.classList.remove('is-pinned');
            if (pinBtn) {
                pinBtn.classList.remove('active');
                pinBtn.setAttribute('title', 'Ghim menu (Ctrl+B)');
                const icon = pinBtn.querySelector('i');
                if (icon) {
                    icon.className = 'bi bi-pin fs-6';
                }
            }
            if (!sidebar.classList.contains('is-hover-open') && !sidebar.classList.contains('is-mobile-open')) {
                sidebar.setAttribute('aria-hidden', 'true');
            }
        }
    }

    function openHover() {
        if (!isDesktop() || isPinned) return;
        clearHideTimer();
        const { sidebar, workspace } = getElements();
        if (!sidebar) return;

        sidebar.classList.add('is-hover-open');
        sidebar.removeAttribute('inert');
        sidebar.setAttribute('aria-hidden', 'false');
        if (workspace) workspace.classList.add('sidebar-hover-open');
    }

    function scheduleHide() {
        if (!isDesktop() || isPinned) return;
        clearHideTimer();
        hideTimer = setTimeout(() => {
            const { sidebar, workspace } = getElements();
            if (!sidebar) return;

            // Do NOT hide if mouse returned to sidebar or trigger
            const trigger = document.getElementById('appSidebarTrigger');
            if (sidebar.matches(':hover') || (trigger && trigger.matches(':hover'))) {
                return;
            }

            // Do NOT hide if an element has focus inside sidebar or an internal dropdown is open
            const isFocusedInside = sidebar.contains(document.activeElement);
            const isDropdownOpen = !!sidebar.querySelector('.dropdown-menu.show, [aria-expanded="true"]');
            if (isFocusedInside || isDropdownOpen) {
                return;
            }

            sidebar.classList.remove('is-hover-open');
            sidebar.setAttribute('aria-hidden', 'true');
            if (workspace) workspace.classList.remove('sidebar-hover-open');
        }, 300);
    }

    function clearHideTimer() {
        if (hideTimer) {
            clearTimeout(hideTimer);
            hideTimer = null;
        }
    }

    function togglePin() {
        if (!isDesktop()) {
            toggleMobile();
            return;
        }
        isPinned = !isPinned;
        try {
            localStorage.setItem(STORAGE_KEY_PINNED, isPinned ? 'true' : 'false');
        } catch (e) { }
        applyPinnedState();
    }

    function openMobile() {
        const { sidebar, backdrop } = getElements();
        if (!sidebar) return;
        sidebar.classList.add('is-mobile-open');
        sidebar.removeAttribute('inert');
        sidebar.setAttribute('aria-hidden', 'false');
        if (backdrop) backdrop.classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    function closeMobile() {
        const { sidebar, backdrop } = getElements();
        if (!sidebar) return;
        sidebar.classList.remove('is-mobile-open');
        sidebar.setAttribute('aria-hidden', 'true');
        if (backdrop) backdrop.classList.remove('show');
        document.body.style.overflow = '';
    }

    function toggleMobile() {
        const { sidebar } = getElements();
        if (!sidebar) return;
        if (sidebar.classList.contains('is-mobile-open')) {
            closeMobile();
        } else {
            openMobile();
        }
    }

    function toggleFromTopbar() {
        const now = Date.now();
        if (now - lastToggleTime < 250) {
            return; // Prevent double execution from both DOM click and Blazor Interop!
        }
        lastToggleTime = now;
        if (isDesktop()) {
            togglePin();
        } else {
            toggleMobile();
        }
    }

    function closeFloatingOverlay() {
        const { sidebar, workspace } = getElements();
        if (!sidebar) return false;

        if (sidebar.classList.contains('is-mobile-open')) {
            closeMobile();
            return true;
        }
        if (!isPinned && sidebar.classList.contains('is-hover-open')) {
            sidebar.classList.remove('is-hover-open');
            sidebar.setAttribute('aria-hidden', 'true');
            if (workspace) workspace.classList.remove('sidebar-hover-open');
            return true;
        }
        return false;
    }

    function bindEvents() {
        const { sidebar, trigger } = getElements();
        if (!sidebar) return;

        // Trigger zone hover
        if (trigger) {
            trigger.onmouseenter = openHover;
            trigger.onmouseleave = scheduleHide;
        }

        // Sidebar hover
        sidebar.onmouseenter = openHover;
        sidebar.onmouseleave = scheduleHide;

        // Focusout listener to check when focus leaves sidebar
        sidebar.onfocusout = function () {
            if (!isDesktop() || isPinned) return;
            setTimeout(() => {
                const els = getElements();
                if (!els.sidebar) return;
                if (!els.sidebar.matches(':hover') && !els.sidebar.contains(document.activeElement)) {
                    scheduleHide();
                }
            }, 60);
        };
    }

    function init() {
        const els = getElements();
        if (!els.sidebar) return;

        initPinnedState();
        bindEvents();
        isInitialized = true;
    }

    // -------------------------------------------------------------------------
    // GLOBAL DELEGATED EVENTS (GUARANTEES 100% RELIABILITY ACROSS NAVIGATION)
    // -------------------------------------------------------------------------
    document.addEventListener('click', function (e) {
        // 1. Topbar Hamburger Button
        const toggleBtn = e.target.closest('#appSidebarToggleBtn');
        if (toggleBtn) {
            e.preventDefault();
            toggleFromTopbar();
            return;
        }

        // 2. Pin Button inside sidebar header
        const pinBtn = e.target.closest('#appSidebarPinBtn');
        if (pinBtn) {
            e.preventDefault();
            e.stopPropagation();
            togglePin();
            return;
        }

        // 3. Mobile Close Button
        const closeBtn = e.target.closest('#appSidebarMobileCloseBtn');
        if (closeBtn) {
            e.preventDefault();
            closeMobile();
            return;
        }

        // 4. Mobile Backdrop
        const backdrop = e.target.closest('#appSidebarBackdrop');
        if (backdrop) {
            closeMobile();
            return;
        }

        // 5. Trigger zone click
        const trigger = e.target.closest('#appSidebarTrigger');
        if (trigger && isDesktop() && !isPinned) {
            openHover();
            return;
        }

        // 6. Navigation links click
        const navLink = e.target.closest('.app-nav-link');
        if (navLink) {
            if (!isDesktop()) {
                closeMobile();
            } else if (!isPinned) {
                closeFloatingOverlay();
            }
        }
    });

    // Global Edge Mouse Tracking with requestAnimationFrame throttling
    let mouseMoveTicking = false;
    window.addEventListener('mousemove', function (e) {
        if (!isDesktop() || isPinned || mouseMoveTicking) return;
        const clientX = e.clientX;
        const clientY = e.clientY;
        mouseMoveTicking = true;
        requestAnimationFrame(function () {
            mouseMoveTicking = false;
            const els = getElements();
            if (!els.sidebar) return;

            // When mouse cursor reaches the left edge strip (<= 18px from screen edge, below topbar >= 48px)
            if (clientX <= 18 && clientY >= 48) {
                openHover();
                return;
            }

            // When sidebar is currently floating open in hover mode:
            if (els.sidebar.classList.contains('is-hover-open')) {
                // Keep open while cursor is within sidebar (width 240px + 15px buffer)
                if (clientX <= 255 && clientY >= 48) {
                    clearHideTimer();
                } else {
                    scheduleHide();
                }
            }
        });
    }, { passive: true });

    // Window resize handler: handles crossing desktop/mobile breakpoint
    window.addEventListener('resize', function () {
        if (isDesktop()) {
            closeMobile();
            applyPinnedState();
        } else {
            const { sidebar, workspace, canvas } = getElements();
            if (sidebar) sidebar.classList.remove('is-pinned', 'is-hover-open');
            if (workspace) workspace.classList.remove('is-sidebar-pinned', 'sidebar-hover-open');
            if (canvas) canvas.classList.remove('is-pinned');
        }
    });

    // Expose controller to window
    window.appSidebar = {
        init: init,
        togglePin: togglePin,
        toggleMobile: toggleMobile,
        toggleFromTopbar: toggleFromTopbar,
        openHover: openHover,
        scheduleHide: scheduleHide,
        closeFloatingOverlay: closeFloatingOverlay,
        syncState: applyPinnedState
    };

    // Auto-init when ready or via MutationObserver
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }



    // Blazor enhanced navigation listener
    if (window.Blazor && window.Blazor.addEventListener) {
        window.Blazor.addEventListener('enhancedload', init);
    }
    window.addEventListener('popstate', init);

    // -------------------------------------------------------------------------
    // 2. GLOBAL KEYBOARD SHORTCUTS
    // -------------------------------------------------------------------------
    document.addEventListener('keydown', function (e) {
        // Ctrl+B or Cmd+B: Toggle pin / sidebar
        if ((e.ctrlKey || e.metaKey) && (e.key === 'b' || e.key === 'B')) {
            e.preventDefault();
            toggleFromTopbar();
            return;
        }

        // Ctrl+K or Cmd+K: Focus Global Search Input
        if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
            const searchInput = document.getElementById('globalSearchInput');
            if (searchInput) {
                e.preventDefault();
                searchInput.focus();
                searchInput.select();
                return;
            }
        }
        
        // Escape: Close floating overlay sidebar, modals, or blur search
        if (e.key === 'Escape') {
            const activeInput = document.activeElement;
            if (activeInput && activeInput.id === 'globalSearchInput') {
                activeInput.blur();
                return;
            }

            const closed = closeFloatingOverlay();
            if (closed) {
                e.preventDefault();
            }
        }
    });

    // -------------------------------------------------------------------------
    // 3. THEME CONTROLLER & UTILITIES (PERMANENT LIGHT THEME)
    // -------------------------------------------------------------------------
    window.setAppTheme = function (theme) {
        try {
            localStorage.removeItem('theme_preference');
            document.documentElement.setAttribute('data-theme', 'light');
            document.documentElement.setAttribute('data-bs-theme', 'light');
        } catch (e) { }
    };

    window.getAppTheme = function () {
        return 'light';
    };

    try {
        localStorage.removeItem('theme_preference');
        document.documentElement.setAttribute('data-theme', 'light');
        document.documentElement.setAttribute('data-bs-theme', 'light');
    } catch (e) { }

    window.copyToClipboard = async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy: ', err);
            return false;
        }
    };

    // -------------------------------------------------------------------------
    // 4. PREVENT BROWSER BFCACHE FROM SHOWING STALE AUTHENTICATED PAGES
    // -------------------------------------------------------------------------
    window.addEventListener('pageshow', function (event) {
        if (event.persisted) {
            window.location.reload();
        }
    });
})();
