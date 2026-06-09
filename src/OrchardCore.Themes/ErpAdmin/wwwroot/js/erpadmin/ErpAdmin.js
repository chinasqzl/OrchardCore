(function () {
    'use strict';

    // --- Brand Color Injection ---

    function hexToRgb(hex) {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        if (!result) return null;
        return {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        };
    }

    function darken(hex, amount) {
        var rgb = hexToRgb(hex);
        if (!rgb) return hex;
        var r = Math.max(0, Math.round(rgb.r * (1 - amount)));
        var g = Math.max(0, Math.round(rgb.g * (1 - amount)));
        var b = Math.max(0, Math.round(rgb.b * (1 - amount)));
        return '#' + [r, g, b].map(function (c) { return c.toString(16).padStart(2, '0'); }).join('');
    }

    function lighten(hex, amount) {
        var rgb = hexToRgb(hex);
        if (!rgb) return hex;
        var r = Math.min(255, Math.round(rgb.r + (255 - rgb.r) * amount));
        var g = Math.min(255, Math.round(rgb.g + (255 - rgb.g) * amount));
        var b = Math.min(255, Math.round(rgb.b + (255 - rgb.b) * amount));
        return '#' + [r, g, b].map(function (c) { return c.toString(16).padStart(2, '0'); }).join('');
    }

    function getBrandColor() {
        var html = document.documentElement;

        // Try data-erp-brand attribute on <html>
        var brandAttr = html.getAttribute('data-erp-brand');
        if (brandAttr) return brandAttr;

        // Try meta tag
        var metaTag = document.querySelector('meta[name="erp-brand-color"]');
        if (metaTag && metaTag.getAttribute('content')) return metaTag.getAttribute('content');

        return null;
    }

    function getAccentColor() {
        var html = document.documentElement;

        var accentAttr = html.getAttribute('data-erp-accent');
        if (accentAttr) return accentAttr;

        var metaTag = document.querySelector('meta[name="erp-accent-color"]');
        if (metaTag && metaTag.getAttribute('content')) return metaTag.getAttribute('content');

        return null;
    }

    function applyBrandColors() {
        var primary = getBrandColor();
        if (!primary) return;

        var root = document.documentElement;
        var primaryRgb = hexToRgb(primary);

        root.style.setProperty('--erp-brand-primary', primary);
        if (primaryRgb) {
            root.style.setProperty('--erp-brand-primary-rgb', primaryRgb.r + ', ' + primaryRgb.g + ', ' + primaryRgb.b);
        }
        root.style.setProperty('--erp-brand-primary-dark', darken(primary, 0.15));
        root.style.setProperty('--erp-brand-primary-light', lighten(primary, 0.2));

        var accent = getAccentColor();
        if (accent) {
            root.style.setProperty('--erp-brand-accent', accent);
            var accentRgb = hexToRgb(accent);
            if (accentRgb) {
                root.style.setProperty('--erp-brand-accent-rgb', accentRgb.r + ', ' + accentRgb.g + ', ' + accentRgb.b);
            }
        }
    }

    // --- Sidebar Enhancement ---

    function initSidebarHover() {
        var sidebar = document.getElementById('ta-left-sidebar');
        if (!sidebar) return;

        var items = sidebar.querySelectorAll('.nav-item, .sidebar-menu-item');
        items.forEach(function (item) {
            item.addEventListener('mouseenter', function () {
                item.style.borderLeft = '3px solid var(--erp-brand-primary, #1e40af)';
                item.style.boxShadow = '-2px 0 8px rgba(var(--erp-brand-primary-rgb, 30, 64, 175), 0.3)';
                item.style.transition = 'border-left 0.2s ease, box-shadow 0.2s ease';
            });
            item.addEventListener('mouseleave', function () {
                item.style.borderLeft = '';
                item.style.boxShadow = '';
            });
        });
    }

    // --- Content Area Card Auto-Detection ---

    function wrapWithCard(elements) {
        elements.forEach(function (el) {
            if (el.parentElement && el.parentElement.classList.contains('erp-card')) return;

            var wrapper = document.createElement('div');
            wrapper.className = 'erp-card';
            el.parentNode.insertBefore(wrapper, el);
            wrapper.appendChild(el);
        });
    }

    function enhanceContentActions() {
        var content = document.querySelector('.ta-content');
        if (!content) return;

        var btnGroups = content.querySelectorAll('.btn-group');
        var actionBars = content.querySelectorAll('.action-bar');

        wrapWithCard(btnGroups);
        wrapWithCard(actionBars);
    }

    // --- Table Enhancement ---

    function enhanceTables() {
        var content = document.querySelector('.ta-content');
        if (!content) return;

        var tables = content.querySelectorAll('table');
        tables.forEach(function (table) {
            if (table.classList.contains('erp-table')) return;
            table.classList.add('erp-table');

            if (table.parentElement && table.parentElement.classList.contains('erp-table-wrapper')) return;

            var wrapper = document.createElement('div');
            wrapper.className = 'erp-table-wrapper';
            wrapper.style.overflowX = 'auto';

            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        });
    }

    // --- Initialization ---

    function init() {
        applyBrandColors();
        initSidebarHover();
        enhanceContentActions();
        enhanceTables();

        document.body.classList.add('erp-admin-loaded');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
