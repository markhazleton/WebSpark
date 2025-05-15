window.addEventListener('DOMContentLoaded', event => {

    // Enable tooltips globally
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Enable popovers globally
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Activate Bootstrap scrollspy for the sticky nav component
    const navStick = document.body.querySelector('#navStick');
    if (navStick) {
        new bootstrap.ScrollSpy(document.body, {
            target: '#navStick',
            offset: 82,
        });
    }

    // Collapse Navbar
    // Add styling fallback for when a transparent background .navbar-marketing is scrolled
    var navbarCollapse = function() {
        const navbarMarketingTransparentFixed = document.body.querySelector('.navbar-marketing.bg-transparent.fixed-top');
        if (!navbarMarketingTransparentFixed) {
            return;
        }
        if (window.scrollY === 0) {
            navbarMarketingTransparentFixed.classList.remove('navbar-scrolled')
        } else {
            navbarMarketingTransparentFixed.classList.add('navbar-scrolled')
        }
    };
    // Collapse now if page is not at top
    navbarCollapse();
    // Collapse the navbar when page is scrolled
    document.addEventListener('scroll', navbarCollapse);

    // Theme switcher functionality
    // Remove localStorage usage for theme persistence
    const initTheme = () => {
        // The server will set the theme on page load via BootswatchThemeHelper
        // Just update the toggle UI to match the current theme
        const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
        updateThemeToggle(currentTheme);
    };

    const updateThemeToggle = (theme) => {
        const themeToggles = document.querySelectorAll('.theme-toggle');
        themeToggles.forEach(toggle => {
            const lightIcon = toggle.querySelector('.theme-toggle-light-icon');
            const darkIcon = toggle.querySelector('.theme-toggle-dark-icon');
            
            if (theme === 'dark') {
                darkIcon.classList.add('d-none');
                lightIcon.classList.remove('d-none');
            } else {
                lightIcon.classList.add('d-none');
                darkIcon.classList.remove('d-none');
            }
        });
    };

    // Initialize theme
    initTheme();

    // Add click event listeners to theme toggles
    const themeToggles = document.querySelectorAll('.theme-toggle');
    themeToggles.forEach(toggle => {
        toggle.addEventListener('click', () => {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', newTheme);
            updateThemeToggle(newTheme);
            // Persist theme to server session
            fetch('/Home/SetTheme', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: `theme=${encodeURIComponent(newTheme)}`
            });
        });
    });

    // Helper to get anti-forgery token from the page (if present)
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }
});
