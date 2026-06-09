import $ from 'jquery';

$(document).ready(() => {
    initSidebar();
    initUserMenu();
    initThemeToggle();
});

function initSidebar() {
    const sidebarToggle = $('.erp-sidebar-toggle');
    const sidebar = $('.erp-sidebar');
    const content = $('.erp-admin-content');
    
    sidebarToggle.on('click', () => {
        sidebar.toggleClass('erp-sidebar-collapsed');
        content.toggleClass('erp-content-expanded');
    });
}

function initUserMenu() {
    const userMenu = $('.user-menu');
    const dropdownMenu = userMenu.find('.dropdown-menu');
    
    userMenu.on('click', '.dropdown-toggle', (e) => {
        e.stopPropagation();
        dropdownMenu.toggleClass('show');
    });
    
    $(document).on('click', (e) => {
        if (!userMenu.is(e.target) && userMenu.has(e.target).length === 0) {
            dropdownMenu.removeClass('show');
        }
    });
}

function initThemeToggle() {
    const themeToggle = $('.erp-theme-toggle');
    
    themeToggle.on('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('erp-theme', newTheme);
    });
    
    const savedTheme = localStorage.getItem('erp-theme');
    if (savedTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme);
    }
}

export function initErpAdmin() {
    initSidebar();
    initUserMenu();
    initThemeToggle();
}