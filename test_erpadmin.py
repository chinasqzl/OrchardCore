from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page()
    
    errors = []
    page.on("pageerror", lambda err: errors.append(f"[PAGE_ERROR] {err}"))
    page.on("console", lambda msg: errors.append(f"[{msg.type}] {msg.text}") if msg.type == "error" else None)
    
    # Test 1: Homepage (Setup page)
    print("=== Test 1: Homepage ===")
    page.goto('http://localhost:5000/', timeout=30000)
    page.wait_for_load_state('networkidle', timeout=15000)
    page.screenshot(path='/workspace/test_home.png', full_page=True)
    title = page.title()
    print(f"Title: {title}")
    print(f"URL: {page.url}")
    status_ok = "Error" not in title and "error" not in page.inner_text("body").lower()[:200]
    print(f"Page OK: {status_ok}")
    
    # Test 2: Login page
    print("\n=== Test 2: Login Page ===")
    page.goto('http://localhost:5000/Login', timeout=30000)
    page.wait_for_load_state('networkidle', timeout=15000)
    page.screenshot(path='/workspace/test_login.png', full_page=True)
    title = page.title()
    print(f"Title: {title}")
    print(f"URL: {page.url}")
    body_text = page.inner_text("body").lower()[:300]
    has_login_form = "login" in body_text or "password" in body_text or "sign" in body_text
    print(f"Login form detected: {has_login_form}")
    
    # Test 3: Admin page (should redirect to login)
    print("\n=== Test 3: Admin Page ===")
    page.goto('http://localhost:5000/Admin', timeout=30000)
    page.wait_for_load_state('networkidle', timeout=15000)
    page.screenshot(path='/workspace/test_admin.png', full_page=True)
    print(f"URL: {page.url}")
    body_text = page.inner_text("body").lower()[:300]
    print(f"Body preview: {body_text[:200]}")
    
    # Print errors
    if errors:
        print("\n=== ERRORS ===")
        for e in errors:
            print(e)
    else:
        print("\n=== No browser errors ===")
    
    browser.close()
    print("\nAll tests completed!")
