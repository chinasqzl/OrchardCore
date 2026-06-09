from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page()

    # Step 1: Login
    page.goto('http://localhost:5000/Login')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='/tmp/01_login_page.png', full_page=True)

    # Fill login form
    page.fill('input[name="UserName"], input[id="UserName"]', 'admin')
    page.fill('input[name="Password"], input[id="Password"]', 'China521%')
    page.click('input[type="submit"], button[type="submit"]')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='/tmp/02_after_login.png', full_page=True)

    print("Current URL after login:", page.url)

    # Step 2: Go to Features to enable Comments module
    page.goto('http://localhost:5000/Admin/Features')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='/tmp/03_features_page.png', full_page=True)

    # Search for Comments module
    search_input = page.locator('input[name="search"], input[id="search"], input[type="search"]')
    if search_input.count() > 0:
        search_input.first.fill('Comments')
        page.wait_for_timeout(1000)
        page.screenshot(path='/tmp/04_features_search.png', full_page=True)

    # Find and click the Enable button for Comments
    page_content = page.content()
    if 'OrchardCore.Comments' in page_content or 'Comments' in page_content:
        enable_btn = page.locator('form:has-text("Comments") button:has-text("Enable"), form:has-text("Comments") input[value="Enable"]')
        if enable_btn.count() > 0:
            enable_btn.first.click()
            page.wait_for_load_state('networkidle')
            page.screenshot(path='/tmp/05_comments_enabled.png', full_page=True)
            print("Comments module enabled!")
        else:
            print("Comments module may already be enabled or Enable button not found")
            disable_btn = page.locator('form:has-text("Comments") button:has-text("Disable"), form:has-text("Comments") input[value="Disable"]')
            if disable_btn.count() > 0:
                print("Comments module is already enabled (Disable button found)")
            else:
                print("Could not find Enable/Disable button for Comments")
    else:
        print("Comments module not found on Features page")

    # Step 3: Go to Content Types to add CommentablePart to Article
    page.goto('http://localhost:5000/Admin/ContentTypes')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='/tmp/06_content_types.png', full_page=True)

    # Check if Article type exists
    if page.locator('a:has-text("Article")').count() > 0:
        page.click('a:has-text("Article")')
        page.wait_for_load_state('networkidle')
        page.screenshot(path='/tmp/07_article_edit.png', full_page=True)

        # Add CommentablePart
        add_part_btn = page.locator('button:has-text("Add Parts"), a:has-text("Add Parts")')
        if add_part_btn.count() > 0:
            add_part_btn.first.click()
            page.wait_for_load_state('networkidle')
            page.screenshot(path='/tmp/08_add_parts.png', full_page=True)

            commentable_cb = page.locator('input[type="checkbox"][name*="Commentable"], label:has-text("CommentablePart") + input, input[value="CommentablePart"]')
            if commentable_cb.count() > 0:
                commentable_cb.first.check()
                page.screenshot(path='/tmp/09_commentable_checked.png', full_page=True)

                save_btn = page.locator('button:has-text("Save"), button:has-text("Add"), input[value="Save"], input[value="Add"]')
                if save_btn.count() > 0:
                    save_btn.first.click()
                    page.wait_for_load_state('networkidle')
                    page.screenshot(path='/tmp/10_commentable_added.png', full_page=True)
                    print("CommentablePart added to Article!")
            else:
                print("CommentablePart checkbox not found")
        else:
            print("Add Parts button not found")
    else:
        print("Article content type not found, listing available types:")
        links = page.locator('table a, .list-group a').all_text_contents()
        print("Available types:", links)

    browser.close()
