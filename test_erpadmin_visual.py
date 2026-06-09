from playwright.sync_api import sync_playwright
import time

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1440, "height": 900})

    # 1. Login page screenshot
    page.goto('http://localhost:5000/Login')
    page.wait_for_load_state('networkidle')
    time.sleep(2)
    page.screenshot(path='/workspace/test_login.png', full_page=True)
    print("Login page screenshot saved")

    # 2. Login
    page.goto('http://localhost:5000/Login')
    page.wait_for_load_state('networkidle')
    time.sleep(1)

    username_input = page.locator('input[type="text"], input[name="UserName"], input[id="UserName"]')
    password_input = page.locator('input[type="password"], input[name="Password"], input[id="Password"]')

    if username_input.count() > 0 and password_input.count() > 0:
        username_input.first.fill('admin')
        password_input.first.fill('China521%')

        submit_btn = page.locator('button[type="submit"], input[type="submit"]')
        if submit_btn.count() > 0:
            submit_btn.first.click()
            page.wait_for_load_state('networkidle')
            time.sleep(3)
            print("Login attempted")
    else:
        print("No login form found")

    # 3. Admin dashboard screenshot
    page.goto('http://localhost:5000/Admin')
    page.wait_for_load_state('networkidle')
    time.sleep(3)
    page.screenshot(path='/workspace/test_admin.png', full_page=True)
    print("Admin page screenshot saved")

    # 4. Check visual elements
    body_class = page.evaluate('document.body.className')
    print(f"Body class: {body_class}")

    has_erp_admin = page.evaluate('document.body.classList.contains("erp-admin")')
    print(f"Has erp-admin class: {has_erp_admin}")

    topbar_bg = page.evaluate('''
        const topbar = document.querySelector(".ta-navbar-top");
        if (topbar) return window.getComputedStyle(topbar).backgroundColor;
        return "not found";
    ''')
    print(f"Topbar background: {topbar_bg}")

    sidebar_bg = page.evaluate('''
        const sidebar = document.getElementById("ta-left-sidebar");
        if (sidebar) return window.getComputedStyle(sidebar).backgroundColor;
        return "not found";
    ''')
    print(f"Sidebar background: {sidebar_bg}")

    content_bg = page.evaluate('''
        const content = document.querySelector(".ta-content");
        if (content) return window.getComputedStyle(content).backgroundColor;
        return "not found";
    ''')
    print(f"Content background: {content_bg}")

    erp_css = page.evaluate('''
        const links = document.querySelectorAll('link[rel="stylesheet"]');
        for (const link of links) {
            if (link.href.includes("ErpAdmin")) return link.href;
        }
        return "not-found";
    ''')
    print(f"ErpAdmin CSS: {erp_css}")

    accent = page.evaluate('document.querySelector(".erp-topbar-accent") ? "exists" : "not-found"')
    print(f"Topbar accent: {accent}")

    header = page.evaluate('document.querySelector(".erp-sidebar-header") ? "exists" : "not-found"')
    print(f"Sidebar header: {header}")

    # 5. Contents page
    page.goto('http://localhost:5000/Admin/Contents')
    page.wait_for_load_state('networkidle')
    time.sleep(3)
    page.screenshot(path='/workspace/test_contents.png', full_page=True)
    print("Contents page screenshot saved")

    browser.close()
    print("Done!")
