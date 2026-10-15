/* ==========================================================================
   APP GLOBAL JAVASCRIPT - SHORTCUTS, THEME & UTILITIES
   ========================================================================== */

(function () {
    // 1. GLOBAL KEYBOARD SHORTCUTS
    document.addEventListener('keydown', function (e) {
        // Ctrl+K or Cmd+K: Focus Global Search Input
        if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
            const searchInput = document.getElementById('globalSearchInput');
            if (searchInput) {
                e.preventDefault();
                searchInput.focus();
                searchInput.select();
            }
        }
        
        // Escape: Close active modals or blur search
        if (e.key === 'Escape') {
            const activeInput = document.activeElement;
            if (activeInput && activeInput.id === 'globalSearchInput') {
                activeInput.blur();
            }
        }
    });

    // 2. THEME CONTROLLER
    window.setAppTheme = function (theme) {
        if (theme === 'dark') {
            document.documentElement.setAttribute('data-bs-theme', 'dark');
            localStorage.setItem('theme_preference', 'dark');
        } else {
            document.documentElement.setAttribute('data-bs-theme', 'light');
            localStorage.setItem('theme_preference', 'light');
        }
    };

    window.getAppTheme = function () {
        return localStorage.getItem('theme_preference') || 'light';
    };

    // Apply stored theme on script load
    try {
        const storedTheme = localStorage.getItem('theme_preference');
        if (storedTheme === 'dark') {
            document.documentElement.setAttribute('data-bs-theme', 'dark');
        }
    } catch (e) { }

    // 3. CLIPBOARD HELPER
    window.copyToClipboard = async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy: ', err);
            return false;
        }
    };
})();
