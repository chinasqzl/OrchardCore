# ErpAdmin 主题实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 创建 ErpAdmin 主题，继承 TheAdmin，为 SaaS 化 ERP 场景提供色块分区布局、ERP 设计系统和租户品牌定制能力。

**Architecture:** ErpAdmin 继承 TheAdmin 主题，通过视图覆盖机制替换布局和导航形状模板，新增 DetailAdmin 内容显示模板、MetadataContentsHandler 路由覆盖、TenantBranding 租户品牌驱动。SCSS 使用 CSS 自定义属性实现亮/暗主题切换，Parcel 打包资源。

**Tech Stack:** OrchardCore CMS, C# / Razor, SCSS, TypeScript, Parcel, Bootstrap 5, Bootstrap Icons

---

## 文件结构总览

```
src/OrchardCore.Themes/ErpAdmin/
├── Manifest.cs
├── Startup.cs
├── Assets.json
├── ResourceManagementOptionsConfiguration.cs
├── placement.json
├── ErpAdmin.csproj
├── Assets/
│   ├── package.json
│   ├── scss/
│   │   ├── ErpAdmin.scss
│   │   ├── erp-admin-layout.scss
│   │   ├── login.scss
│   │   ├── _variables.scss
│   │   ├── components/
│   │   │   ├── _data-table.scss
│   │   │   ├── _stat-card.scss
│   │   │   ├── _status-badge.scss
│   │   │   ├── _action-bar.scss
│   │   │   ├── _filter-panel.scss
│   │   │   ├── _detail-panel.scss
│   │   │   └── _empty-state.scss
│   │   └── themes/
│   │       ├── light/
│   │       │   └── _index.scss
│   │       └── dark/
│   │           └── _index.scss
│   └── js/
│       └── ErpAdmin/
│           └── ErpAdmin.ts
├── Handlers/
│   └── MetadataContentsHandler.cs
├── Drivers/
│   └── TenantBrandingNavbarDisplayDriver.cs
├── Models/
│   └── TenantBranding.cs
├── Views/
│   ├── _ViewImports.cshtml
│   ├── Layout.cshtml
│   ├── Layout-Login.cshtml
│   ├── Layout-TwoFactor.cshtml
│   ├── Navigation-admin.cshtml
│   ├── NavigationItem-admin.cshtml
│   ├── NavigationItemLink-admin.cshtml
│   ├── NavigationItemText.cshtml
│   ├── AdminBranding.cshtml
│   ├── Navbar.DetailAdmin.cshtml
│   ├── NavbarUserMenu.cshtml
│   ├── UserMenu.cshtml
│   ├── UserMenuItems-SignedUser.cshtml
│   ├── UserMenuItems-SignOut.cshtml
│   ├── Content.DetailAdmin.cshtml
│   ├── ContentPart.DetailAdmin.cshtml
│   ├── Content.SummaryAdmin.cshtml
│   ├── ContentsAdminList.cshtml
│   ├── Stereotype.DetailAdmin.cshtml
│   ├── Account/
│   │   └── Login.cshtml
│   ├── LoginFormCredentials.cshtml
│   ├── LoginFormForgotPassword.cshtml
│   ├── LoginFormRegisterUser.cshtml
│   ├── Pager.cshtml
│   ├── Pager_CurrentPage.cshtml
│   ├── Pager_First.cshtml
│   ├── Pager_Gap.cshtml
│   ├── Pager_Last.cshtml
│   ├── Pager_Link.cshtml
│   ├── Pager_Next.cshtml
│   └── Pager_Previous.cshtml
└── Recipes/
    └── erpadmin.recipe.json
```

---

### Task 1: 项目脚手架

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Manifest.cs`
- Create: `src/OrchardCore.Themes/ErpAdmin/ErpAdmin.csproj`
- Create: `src/OrchardCore.Themes/ErpAdmin/Startup.cs`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets.json`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/_ViewImports.cshtml`

- [ ] **Step 1: 创建 Manifest.cs**

```csharp
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "ErpAdmin",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "ERP Admin theme with sidebar layout, color-block design system, and tenant branding.",
    BaseTheme = "TheAdmin",
    Dependencies =
    [
        "OrchardCore.Themes",
    ],
    Tags =
    [
        ManifestConstants.AdminTag,
    ]
)]
```

- [ ] **Step 2: 创建 ErpAdmin.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>

    <!-- NuGet properties-->
    <Title>ErpAdmin theme</Title>
    <Description>$(OCCMSDescription)

    ERP Admin theme of OrchardCore CMS with sidebar layout and tenant branding</Description>
    <PackageTags>$(PackageTags) OrchardCoreCMS Theme</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore.Modules\OrchardCore.AdminMenu\OrchardCore.AdminMenu.csproj" />
    <ProjectReference Include="..\..\OrchardCore.Modules\OrchardCore.Contents\OrchardCore.Contents.csproj" />
    <ProjectReference Include="..\..\OrchardCore.Modules\OrchardCore.Lists\OrchardCore.Lists.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.AdminMenu.Abstractions\OrchardCore.AdminMenu.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Theme.Targets\OrchardCore.Theme.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.DisplayManagement\OrchardCore.DisplayManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ResourceManagement\OrchardCore.ResourceManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore.Modules\OrchardCore.Admin\OrchardCore.Admin.csproj" />
    <ProjectReference Include="..\..\OrchardCore.Modules\OrchardCore.Themes\OrchardCore.Themes.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Users.Core\OrchardCore.Users.Core.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: 创建 Startup.cs**

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Themes.ErpAdmin.Drivers;
using OrchardCore.Themes.ErpAdmin.Handlers;

namespace OrchardCore.Themes.ErpAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, TenantBrandingNavbarDisplayDriver>();
        services.AddScoped<IContentHandler, MetadataContentsHandler>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
```

- [ ] **Step 4: 创建 Assets.json**

```json
[
  {
    "action": "sass",
    "generateRTL": true,
    "name": "theme-erpadmin",
    "source": "Assets/scss/ErpAdmin.scss",
    "dest": "wwwroot/css/",
    "tags": ["theme", "css"]
  },
  {
    "action": "sass",
    "generateRTL": true,
    "name": "theme-erpadmin-layout",
    "source": "Assets/scss/erp-admin-layout.scss",
    "dest": "wwwroot/css/",
    "tags": ["theme", "css"]
  },
  {
    "action": "sass",
    "generateRTL": true,
    "name": "theme-erpadmin-login",
    "source": "Assets/scss/login.scss",
    "dest": "wwwroot/css/",
    "tags": ["theme", "css"]
  },
  {
    "action": "parcel",
    "name": "ErpAdmin",
    "source": "Assets/js/ErpAdmin/ErpAdmin.ts",
    "dest": "wwwroot/js/erpadmin/",
    "tags": ["theme", "css"]
  }
]
```

- [ ] **Step 5: 创建 _ViewImports.cshtml**

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>

@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement

@using Microsoft.Extensions.Options
@using OrchardCore.Admin
@using OrchardCore.Admin.Models
@using OrchardCore.Entities
@using OrchardCore.Environment.Shell
@using OrchardCore.Themes.Services
@using OrchardCore.Users.Models
```

- [ ] **Step 6: 创建 Assets/package.json**

```json
{
  "name": "@orchardcore/erpadmin",
  "version": "1.0.0",
  "dependencies": {
    "@popperjs/core": "2.11.8",
    "@types/jquery": "3.5.32",
    "@types/js-cookie": "^3.0.6",
    "bootstrap": "5.3.8",
    "bootstrap-icons": "^1.11.3",
    "jquery": "3.7.1",
    "js-cookie": "^3.0.5"
  }
}
```

- [ ] **Step 7: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Manifest.cs src/OrchardCore.Themes/ErpAdmin/ErpAdmin.csproj src/OrchardCore.Themes/ErpAdmin/Startup.cs src/OrchardCore.Themes/ErpAdmin/Assets.json src/OrchardCore.Themes/ErpAdmin/Views/_ViewImports.cshtml src/OrchardCore.Themes/ErpAdmin/Assets/package.json
git commit -m "feat(erpadmin): scaffold project structure with manifest, csproj, startup, and assets config"
```

---

### Task 2: 资源管理

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/ResourceManagementOptionsConfiguration.cs`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/ErpAdmin.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/_variables.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/js/ErpAdmin/ErpAdmin.ts`

- [ ] **Step 1: 创建 ResourceManagementOptionsConfiguration.cs**

```csharp
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.ErpAdmin;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("erp-admin")
            .SetDependencies("bootstrap", "admin-main", "theme-manager", "jquery")
            .SetUrl("~/ErpAdmin/js/erpadmin/ErpAdmin.min.js", "~/ErpAdmin/js/erpadmin/ErpAdmin.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("erp-admin")
            .SetUrl("~/ErpAdmin/css/ErpAdmin.min.css", "~/ErpAdmin/css/ErpAdmin.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
```

- [ ] **Step 2: 创建 _variables.scss**

```scss
/*
    IMPORTANT: Never import Bootstrap directly into the theme.
    ErpAdmin.css will depend on Bootstrap, but we never want to compile it.
*/
@import '../../../../../node_modules/bootstrap/scss/_functions';
@import '../../../../../node_modules/bootstrap/scss/_variables';
@import '../../../../../node_modules/bootstrap/scss/mixins/_breakpoints';

// Custom variables.
$erp-topbar-height: 56px !default;
$erp-sidebar-width: 260px !default;
$erp-sidebar-collapsed-width: 72px !default;
$erp-radius: 10px !default;
$erp-radius-sm: 6px !default;
$erp-transition: .2s cubic-bezier(.4, 0, .2, 1) !default;

:root {
    --erp-primary: #1a56db;
    --erp-primary-light: #e8edfb;
    --erp-primary-dark: #1240a8;
    --erp-accent: #0ea5e9;
    --erp-sidebar-bg: #0f172a;
    --erp-sidebar-text: #94a3b8;
    --erp-sidebar-active: #1e293b;
    --erp-sidebar-width: #{$erp-sidebar-width};
    --erp-sidebar-collapsed-width: #{$erp-sidebar-collapsed-width};
    --erp-topbar-height: #{$erp-topbar-height};
    --erp-content-bg: #f8fafc;
    --erp-card-bg: #ffffff;
    --erp-border: #e2e8f0;
    --erp-text: #1e293b;
    --erp-text-muted: #64748b;
    --erp-success: #10b981;
    --erp-warning: #f59e0b;
    --erp-danger: #ef4444;
    --erp-info: #0ea5e9;
    --erp-radius: #{$erp-radius};
    --erp-radius-sm: #{$erp-radius-sm};
    --erp-shadow: 0 1px 3px rgba(0, 0, 0, .06), 0 1px 2px rgba(0, 0, 0, .04);
    --erp-shadow-md: 0 4px 6px rgba(0, 0, 0, .05), 0 2px 4px rgba(0, 0, 0, .04);
    --erp-shadow-lg: 0 10px 25px rgba(0, 0, 0, .08);
    --erp-transition: #{$erp-transition};
    --erp-row-alt: #f8fafc;
    --oc-top-nav-height: #{$erp-topbar-height};
    --oc-start-navigation-width: #{$erp-sidebar-width};
    --oc-start-navigation-width-when-compact: #{$erp-sidebar-collapsed-width};
}
```

- [ ] **Step 3: 创建 ErpAdmin.scss 主入口**

```scss
/* Import variables first */
@import 'variables';

/* Import ERP components */
@import 'components/data-table';
@import 'components/stat-card';
@import 'components/status-badge';
@import 'components/action-bar';
@import 'components/filter-panel';
@import 'components/detail-panel';
@import 'components/empty-state';

/* Import themes */
@import 'themes/light/index';
@import 'themes/dark/index';

/* Base typography overrides */
body {
    font-family: 'DM Sans', -apple-system, BlinkMacSystemFont, sans-serif;
    background: var(--erp-content-bg);
    color: var(--erp-text);
    overflow-x: hidden;
}

h1, h2, h3, h4, h5, h6, .font-display {
    font-family: 'Space Grotesk', 'DM Sans', sans-serif;
}

/* Button styles */
.btn-erp-primary {
    background: var(--erp-primary);
    color: #fff;
    border: none;
    padding: 8px 18px;
    border-radius: var(--erp-radius-sm);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all var(--erp-transition);
    display: inline-flex;
    align-items: center;
    gap: 6px;

    &:hover {
        background: var(--erp-primary-dark);
        box-shadow: var(--erp-shadow-md);
    }
}

.btn-erp-outline {
    background: transparent;
    color: var(--erp-text);
    border: 1px solid var(--erp-border);
    padding: 8px 18px;
    border-radius: var(--erp-radius-sm);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all var(--erp-transition);
    display: inline-flex;
    align-items: center;
    gap: 6px;

    &:hover {
        border-color: var(--erp-text-muted);
        background: var(--erp-content-bg);
    }
}

.btn-erp-ghost {
    background: transparent;
    color: var(--erp-text-muted);
    border: none;
    padding: 8px 14px;
    border-radius: var(--erp-radius-sm);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all var(--erp-transition);
    display: inline-flex;
    align-items: center;
    gap: 6px;

    &:hover {
        color: var(--erp-text);
        background: var(--erp-content-bg);
    }
}

/* Page header */
.page-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    margin-bottom: 24px;
    gap: 16px;
    flex-wrap: wrap;
}

.page-header-left h1 {
    font-size: 22px;
    font-weight: 700;
    margin: 0;
    line-height: 1.3;
}

.page-header-left .breadcrumb {
    font-size: 12px;
    margin-top: 4px;
    padding: 0;
    background: none;

    a { color: var(--erp-text-muted); text-decoration: none; }
    a:hover { color: var(--erp-primary); }
    .active { color: var(--erp-text); }
}

.page-header-actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
}

/* Content area */
.erp-content {
    flex: 1;
    padding: 24px;
}

/* View switcher */
.view-switcher {
    display: flex;
    gap: 4px;
    background: var(--erp-content-bg);
    padding: 3px;
    border-radius: 8px;
    border: 1px solid var(--erp-border);

    .view-btn {
        padding: 6px 14px;
        border: none;
        border-radius: 6px;
        font-size: 12px;
        font-weight: 500;
        cursor: pointer;
        background: transparent;
        color: var(--erp-text-muted);
        transition: all var(--erp-transition);

        &.active {
            background: var(--erp-card-bg);
            color: var(--erp-text);
            box-shadow: var(--erp-shadow);
        }
    }
}

/* Timeline */
.timeline-item {
    display: flex;
    gap: 12px;
    padding: 10px 0;
    position: relative;

    &:not(:last-child)::after {
        content: '';
        position: absolute;
        left: 15px;
        top: 38px;
        bottom: -2px;
        width: 2px;
        background: var(--erp-content-bg);
        border-radius: 1px;
    }
}

.timeline-dot {
    width: 30px;
    height: 30px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 13px;
    flex-shrink: 0;

    &.blue { background: #dbeafe; color: #2563eb; }
    &.green { background: #d1fae5; color: #059669; }
    &.amber { background: #fef3c7; color: #d97706; }
}

.timeline-content {
    flex: 1;

    .tl-text { font-size: 13px; }
    .tl-time { font-size: 11px; color: var(--erp-text-muted); margin-top: 2px; }
}

/* Action dots */
.action-dots {
    width: 28px;
    height: 28px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 6px;
    border: none;
    background: transparent;
    color: var(--erp-text-muted);
    cursor: pointer;
    transition: all var(--erp-transition);
    font-size: 16px;

    &:hover {
        background: var(--erp-content-bg);
        color: var(--erp-text);
    }
}

/* Responsive */
@include media-breakpoint-down(sm) {
    .erp-content { padding: 16px; }
    .page-header { flex-direction: column; }
}
```

- [ ] **Step 4: 创建 ErpAdmin.ts**

```typescript
///<reference path="@types/bootstrap/index.d.ts" />

function confirmDialog({ callback, ...options }: { callback: (response: boolean) => void; [key: string]: any }) {
    const defaultOptions = $("#confirmRemoveModalMetadata").data();
    const { title, message, okText, cancelText, okClass, cancelClass } = $.extend({}, defaultOptions, options);

    $(
        '<div id="confirmRemoveModal" class="modal" tabindex="-1" role="dialog">\
        <div class="modal-dialog modal-dialog-centered" role="document">\
            <div class="modal-content">\
                <div class="modal-header">\
                    <h5 class="modal-title">' +
        title +
        '</h5>\
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>\
                </div>\
                <div class="modal-body">\
                    <p>' +
        message +
        '</p>\
                </div>\
                <div class="modal-footer">\
                    <button id="modalOkButton" type="button" class="btn ' +
        okClass +
        '">' +
        okText +
        '</button>\
                    <button id="modalCancelButton" type="button" class="btn ' +
        cancelClass +
        '" data-bs-dismiss="modal">' +
        cancelText +
        "</button>\
                </div>\
            </div>\
        </div>\
    </div>",
    ).appendTo("body");

    const modalElement = document.getElementById("confirmRemoveModal");

    if (modalElement) {
        const confirmModal = new bootstrap.Modal(modalElement, {
            backdrop: "static",
            keyboard: false,
        });

        confirmModal.show();

        document.getElementById("confirmRemoveModal")?.addEventListener("hidden.bs.modal", function () {
            document.getElementById("confirmRemoveModal")?.remove();
            confirmModal.dispose();
        });

        $("#modalOkButton").click(function () {
            callback(true);
            confirmModal.hide();
        });

        $("#modalCancelButton").click(function () {
            callback(false);
            confirmModal.hide();
        });
    }
}

(function () {
    // Prevents page flickering while downloading css
    document.addEventListener("DOMContentLoaded", () => {
        document.body.classList.remove("preload");
    });
})();

$(function () {
    $("body").on("click", '[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]', function () {
        const _this = $(this);
        if (_this.filter('a[itemprop~="UnsafeUrl"]').length == 1) {
            console.warn("Please use data-url-af instead of itemprop attribute for confirm modals.");
        }
        if (_this.filter('[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]').length == 1) {
            return false;
        }
        confirmDialog({
            ..._this.data(),
            callback: function (resp: any) {
                if (resp) {
                    const url = _this.attr("href");
                    if (url == undefined) {
                        let form = _this.parents("form");
                        form.append($('<input type="hidden" name="' + _this.attr("name") + '" value="' + _this.attr("value") + '" />'));
                        form.submit();
                    } else {
                        window.location.href = url;
                    }
                }
            },
        });
        return false;
    });
});

$(function () {
    const magicToken = $("input[name=__RequestVerificationToken]").first();
    if (magicToken) {
        $("body").on("click", 'a[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]', function () {
            const _this = $(this);
            const hrefParts = _this.attr("href")?.split("?");

            if (hrefParts == undefined) {
                return false;
            }

            let form = $('<form action="' + hrefParts[0] + '" method="POST" />');
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                const queryParts = hrefParts[1].split("&");
                for (let i = 0; i < queryParts.length; i++) {
                    const queryPartKVP = queryParts[i].split("=");
                    form.append($('<input type="hidden" name="' + decodeURIComponent(queryPartKVP[0]) + '" value="' + decodeURIComponent(queryPartKVP[1]) + '" />'));
                }
            }

            form.css({ position: "absolute", left: "-9999em" });
            $("body").append(form);

            const unsafeUrlPrompt = _this.data("unsafe-url");

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp: any) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });
                return false;
            }

            if (_this.filter('[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]').length == 1) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp: any) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });
                return false;
            }

            form.submit();
            return false;
        });
    }
});

(function () {
    // Tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
})();

// Prevent multi submissions on forms
$("body").on("submit", "form.no-multisubmit", function (e) {
    const submittingClass = "submitting";
    const form = $(this);

    if (form.hasClass(submittingClass)) {
        e.preventDefault();
        return;
    }

    form.addClass(submittingClass);
    setTimeout(function () {
        form.removeClass(submittingClass);
    }, 5000);
});

// ===== ErpAdmin-specific interactions =====

// Sidebar toggle
function toggleSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    if (!sidebar) return;
    sidebar.classList.toggle("collapsed");
    const btn = sidebar.querySelector(".sidebar-toggle-btn i");
    if (btn) {
        btn.classList.toggle("bi-chevron-double-left");
        btn.classList.toggle("bi-chevron-double-right");
    }
    // Persist preference
    const isCollapsed = sidebar.classList.contains("collapsed");
    try {
        const prefs = JSON.parse(localStorage.getItem("erpadmin_prefs") || "{}");
        prefs.sidebarCollapsed = isCollapsed;
        localStorage.setItem("erpadmin_prefs", JSON.stringify(prefs));
    } catch (e) { /* ignore */ }
}

// Mobile sidebar
function openMobileSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    const overlay = document.getElementById("erp-sidebar-overlay");
    if (sidebar) sidebar.classList.add("mobile-open");
    if (overlay) overlay.classList.add("show");
}

function closeMobileSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    const overlay = document.getElementById("erp-sidebar-overlay");
    if (sidebar) sidebar.classList.remove("mobile-open");
    if (overlay) overlay.classList.remove("show");
}

// Theme toggle
function toggleErpTheme() {
    const html = document.documentElement;
    const current = html.getAttribute("data-bs-theme");
    const next = current === "dark" ? "light" : "dark";
    html.setAttribute("data-bs-theme", next);
    const icon = document.querySelector(".erp-theme-toggle-btn i");
    if (icon) {
        icon.classList.toggle("bi-moon-stars", next === "dark");
        icon.classList.toggle("bi-sun", next === "light");
    }
}

// User dropdown
function toggleUserDropdown() {
    const dropdown = document.getElementById("erp-user-dropdown");
    if (dropdown) dropdown.classList.toggle("open");
}

// Filter panel
function toggleFilterPanel() {
    const panel = document.getElementById("erp-filter-panel");
    const overlay = document.getElementById("erp-filter-overlay");
    if (panel) panel.classList.toggle("open");
    if (overlay) overlay.classList.toggle("open");
}

// Close user dropdown on outside click
document.addEventListener("click", function (e) {
    const userMenu = (e.target as HTMLElement).closest(".topbar-user");
    if (!userMenu) {
        const dropdown = document.getElementById("erp-user-dropdown");
        if (dropdown) dropdown.classList.remove("open");
    }
});

// Restore sidebar state on load
document.addEventListener("DOMContentLoaded", function () {
    try {
        const prefs = JSON.parse(localStorage.getItem("erpadmin_prefs") || "{}");
        if (prefs.sidebarCollapsed) {
            const sidebar = document.getElementById("erp-sidebar");
            if (sidebar) {
                sidebar.classList.add("collapsed");
                const btn = sidebar.querySelector(".sidebar-toggle-btn i");
                if (btn) {
                    btn.classList.remove("bi-chevron-double-left");
                    btn.classList.add("bi-chevron-double-right");
                }
            }
        }
    } catch (e) { /* ignore */ }
});

// Expose globally
(window as any).toggleSidebar = toggleSidebar;
(window as any).openMobileSidebar = openMobileSidebar;
(window as any).closeMobileSidebar = closeMobileSidebar;
(window as any).toggleErpTheme = toggleErpTheme;
(window as any).toggleUserDropdown = toggleUserDropdown;
(window as any).toggleFilterPanel = toggleFilterPanel;

declare global {
    interface Window {
        confirmDialog: typeof confirmDialog;
        toggleSidebar: typeof toggleSidebar;
        openMobileSidebar: typeof openMobileSidebar;
        closeMobileSidebar: typeof closeMobileSidebar;
        toggleErpTheme: typeof toggleErpTheme;
        toggleUserDropdown: typeof toggleUserDropdown;
        toggleFilterPanel: typeof toggleFilterPanel;
    }
}

window.confirmDialog = confirmDialog;

export { confirmDialog, toggleSidebar, openMobileSidebar, closeMobileSidebar, toggleErpTheme, toggleUserDropdown, toggleFilterPanel };
```

- [ ] **Step 5: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/ResourceManagementOptionsConfiguration.cs src/OrchardCore.Themes/ErpAdmin/Assets/scss/ErpAdmin.scss src/OrchardCore.Themes/ErpAdmin/Assets/scss/_variables.scss src/OrchardCore.Themes/ErpAdmin/Assets/js/ErpAdmin/ErpAdmin.ts
git commit -m "feat(erpadmin): add resource management, SCSS variables, and TypeScript entry point"
```

---

### Task 3: 布局覆盖

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Layout.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Layout-Login.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Layout-TwoFactor.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/erp-admin-layout.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/login.scss`

- [ ] **Step 1: 创建 erp-admin-layout.scss**

```scss
@import 'variables';

* { margin: 0; padding: 0; box-sizing: border-box; }

body, html {
    height: 100%;
}

.erp-wrapper {
    display: flex;
    min-height: 100vh;
}

/* ===== SIDEBAR ===== */
.erp-sidebar {
    width: var(--erp-sidebar-width);
    background: var(--erp-sidebar-bg);
    color: var(--erp-sidebar-text);
    display: flex;
    flex-direction: column;
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    z-index: 1040;
    transition: width var(--erp-transition), transform var(--erp-transition);
    overflow: hidden;

    &.collapsed {
        width: var(--erp-sidebar-collapsed-width);
    }
}

.erp-sidebar-brand {
    height: var(--erp-topbar-height);
    display: flex;
    align-items: center;
    padding: 0 20px;
    border-bottom: none;
    margin-bottom: 4px;
    flex-shrink: 0;

    .brand-logo {
        width: 32px;
        height: 32px;
        background: var(--erp-primary);
        border-radius: 8px;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-weight: 700;
        font-size: 14px;
        flex-shrink: 0;
    }

    .brand-text {
        margin-left: 12px;
        font-family: 'Space Grotesk', sans-serif;
        font-weight: 600;
        font-size: 16px;
        color: #f1f5f9;
        white-space: nowrap;
        opacity: 1;
        transition: opacity var(--erp-transition);
    }
}

.erp-sidebar.collapsed .brand-text {
    opacity: 0;
    pointer-events: none;
}

.erp-sidebar-nav {
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
    padding: 12px 0;

    &::-webkit-scrollbar { width: 3px; }
    &::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, .15); border-radius: 3px; }
}

.nav-section-title {
    padding: 16px 20px 6px;
    font-size: 10px;
    font-weight: 600;
    letter-spacing: 1.2px;
    text-transform: uppercase;
    color: rgba(148, 163, 184, .5);
    white-space: nowrap;
    overflow: hidden;
}

.erp-sidebar.collapsed .nav-section-title {
    text-align: center;
    padding: 16px 8px 6px;
}

.nav-item-erp {
    position: relative;
}

.nav-link-erp {
    display: flex;
    align-items: center;
    padding: 9px 20px;
    color: var(--erp-sidebar-text);
    text-decoration: none;
    font-size: 13.5px;
    font-weight: 400;
    transition: all var(--erp-transition);
    cursor: pointer;
    border: none;
    background: none;
    width: 100%;
    text-align: left;
    white-space: nowrap;

    &:hover {
        color: #e2e8f0;
        background: rgba(255, 255, 255, .04);
    }

    &.active {
        color: #fff;
        background: var(--erp-primary);
        font-weight: 500;
    }

    .nav-icon {
        width: 20px;
        height: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 17px;
        flex-shrink: 0;
    }

    .nav-label {
        margin-left: 12px;
        opacity: 1;
        transition: opacity var(--erp-transition);
    }

    .nav-badge {
        margin-left: auto;
        background: var(--erp-danger);
        color: #fff;
        font-size: 10px;
        font-weight: 600;
        padding: 2px 7px;
        border-radius: 10px;
        line-height: 1.4;
    }

    .nav-expand {
        margin-left: auto;
        font-size: 12px;
        transition: transform var(--erp-transition);
    }

    &[aria-expanded="true"] .nav-expand {
        transform: rotate(90deg);
    }
}

.erp-sidebar.collapsed .nav-label {
    opacity: 0;
    pointer-events: none;
}

.erp-sidebar.collapsed .nav-badge {
    display: none;
}

.erp-sidebar.collapsed .nav-expand {
    display: none;
}

.nav-submenu {
    padding-left: 0;
    list-style: none;
    overflow: hidden;

    .nav-link-erp {
        padding-left: 52px;
        font-size: 13px;
    }
}

.erp-sidebar.collapsed .nav-submenu {
    display: none;
}

.sidebar-footer {
    padding: 12px;
    border-top: none;
    margin-top: 8px;
    flex-shrink: 0;
}

.sidebar-toggle-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    padding: 8px;
    background: rgba(255, 255, 255, .04);
    border: none;
    border-radius: var(--erp-radius-sm);
    color: var(--erp-sidebar-text);
    cursor: pointer;
    transition: all var(--erp-transition);

    &:hover {
        background: rgba(255, 255, 255, .08);
        color: #e2e8f0;
    }
}

/* ===== MAIN AREA ===== */
.erp-main {
    flex: 1;
    margin-left: var(--erp-sidebar-width);
    transition: margin-left var(--erp-transition);
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

.erp-sidebar.collapsed ~ .erp-main {
    margin-left: var(--erp-sidebar-collapsed-width);
}

/* ===== TOPBAR ===== */
.erp-topbar {
    height: var(--erp-topbar-height);
    background: var(--erp-card-bg);
    border-bottom: none;
    display: flex;
    align-items: center;
    padding: 0 24px;
    position: sticky;
    top: 0;
    z-index: 1030;
    box-shadow: var(--erp-shadow);
}

.topbar-left {
    display: flex;
    align-items: center;
    gap: 16px;
}

.mobile-menu-btn {
    display: none;
    background: none;
    border: none;
    font-size: 20px;
    color: var(--erp-text);
    cursor: pointer;
    padding: 4px;
}

.topbar-search {
    position: relative;

    input {
        background: var(--erp-content-bg);
        border: 1px solid var(--erp-border);
        border-radius: 8px;
        padding: 7px 14px 7px 36px;
        font-size: 13px;
        width: 280px;
        color: var(--erp-text);
        transition: all var(--erp-transition);

        &:focus {
            outline: none;
            border-color: var(--erp-primary);
            box-shadow: 0 0 0 3px var(--erp-primary-light);
            width: 340px;
        }
    }

    .search-icon {
        position: absolute;
        left: 11px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--erp-text-muted);
        font-size: 14px;
    }
}

.topbar-right {
    margin-left: auto;
    display: flex;
    align-items: center;
    gap: 8px;
}

.topbar-icon-btn {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 8px;
    border: none;
    background: transparent;
    color: var(--erp-text-muted);
    cursor: pointer;
    transition: all var(--erp-transition);
    position: relative;
    font-size: 18px;

    &:hover {
        background: var(--erp-content-bg);
        color: var(--erp-text);
    }

    .notif-dot {
        position: absolute;
        top: 7px;
        right: 7px;
        width: 7px;
        height: 7px;
        background: var(--erp-danger);
        border-radius: 50%;
        border: 1.5px solid var(--erp-card-bg);
    }
}

.topbar-tenant {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 5px 12px;
    background: var(--erp-primary-light);
    border-radius: 8px;
    font-size: 12px;
    font-weight: 500;
    color: var(--erp-primary);
}

.topbar-user {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 4px 8px 4px 4px;
    border-radius: 8px;
    cursor: pointer;
    transition: background var(--erp-transition);
    position: relative;

    &:hover {
        background: var(--erp-content-bg);
    }

    .user-avatar {
        width: 32px;
        height: 32px;
        border-radius: 8px;
        background: linear-gradient(135deg, var(--erp-primary), var(--erp-accent));
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-size: 13px;
        font-weight: 600;
    }

    .user-info { line-height: 1.2; }
    .user-name { font-size: 13px; font-weight: 500; color: var(--erp-text); }
    .user-role { font-size: 11px; color: var(--erp-text-muted); }
}

.topbar-user-dropdown {
    position: absolute;
    top: 100%;
    right: 0;
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    box-shadow: var(--erp-shadow-lg);
    min-width: 220px;
    padding: 8px;
    display: none;
    z-index: 1040;

    &.open { display: block; }

    .dropdown-header {
        padding: 8px 12px;
        font-size: 12px;
        color: var(--erp-text-muted);
        border-bottom: none;
        margin-bottom: 4px;
        background: var(--erp-content-bg);
        border-radius: 6px;

        strong { color: var(--erp-text); }
    }

    .dropdown-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 8px 12px;
        border-radius: 6px;
        font-size: 13px;
        color: var(--erp-text);
        text-decoration: none;
        cursor: pointer;
        transition: background var(--erp-transition);

        &:hover { background: var(--erp-content-bg); }
        &.danger { color: var(--erp-danger); }
    }

    .dropdown-divider {
        height: 6px;
        background: transparent;
        margin: 4px 0;
    }
}

/* Sidebar overlay for mobile */
.sidebar-overlay {
    display: none;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, .4);
    z-index: 1035;

    &.show { display: block; }
}

/* ===== RESPONSIVE ===== */
@include media-breakpoint-down(lg) {
    .erp-sidebar {
        transform: translateX(-100%);

        &.mobile-open { transform: translateX(0); }
    }

    .erp-main { margin-left: 0 !important; }
    .mobile-menu-btn { display: flex; }
    .topbar-search input { width: 180px; }
    .topbar-search input:focus { width: 220px; }
    .topbar-tenant .tenant-name { display: none; }
}

@include media-breakpoint-down(sm) {
    .topbar-search { display: none; }
}

/* Preload fix */
.preload * {
    -webkit-transition: none !important;
    -moz-transition: none !important;
    -o-transition: none !important;
}
```

- [ ] **Step 2: 创建 login.scss**

```scss
@import 'variables';

body, html {
    height: 100%;
}

.login-page {
    min-height: 100vh;
    display: flex;
    background: var(--erp-sidebar-bg);
    position: relative;
    overflow: hidden;
}

.login-left {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    padding: 60px;
    position: relative;
    z-index: 1;

    &::before {
        content: '';
        position: absolute;
        inset: 0;
        background: radial-gradient(ellipse at 30% 50%, rgba(26, 86, 219, .15) 0%, transparent 70%),
                    radial-gradient(ellipse at 70% 80%, rgba(14, 165, 233, .1) 0%, transparent 60%);
        z-index: -1;
    }
}

.login-brand {
    display: flex;
    align-items: center;
    gap: 14px;
    margin-bottom: 48px;

    .brand-logo {
        width: 48px;
        height: 48px;
        background: var(--erp-primary);
        border-radius: 12px;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-weight: 700;
        font-size: 20px;
        font-family: 'Space Grotesk', sans-serif;
    }

    .brand-name {
        font-family: 'Space Grotesk', sans-serif;
        font-size: 24px;
        font-weight: 700;
        color: #f1f5f9;
    }
}

.login-illustration {
    max-width: 420px;
    width: 100%;

    .illus-card {
        background: rgba(255, 255, 255, .04);
        border: 1px solid rgba(255, 255, 255, .06);
        border-radius: 16px;
        padding: 32px;
        backdrop-filter: blur(10px);
    }

    .illus-title {
        font-family: 'Space Grotesk', sans-serif;
        font-size: 28px;
        font-weight: 700;
        color: #f1f5f9;
        margin-bottom: 12px;
        line-height: 1.3;
    }

    .illus-desc {
        font-size: 15px;
        color: #94a3b8;
        line-height: 1.7;
        margin-bottom: 24px;
    }
}

.illus-features {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.illus-feature {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 13px;
    color: #cbd5e1;

    .feat-icon {
        width: 32px;
        height: 32px;
        border-radius: 8px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 14px;
        flex-shrink: 0;

        &.blue { background: rgba(37, 99, 235, .2); color: #60a5fa; }
        &.green { background: rgba(5, 150, 105, .2); color: #34d399; }
        &.amber { background: rgba(217, 119, 6, .2); color: #fbbf24; }
    }
}

.login-right {
    width: 520px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding: 60px;
    background: var(--erp-card-bg);
    border-radius: 24px 0 0 24px;
    box-shadow: -20px 0 60px rgba(0, 0, 0, .15);
}

.login-form-header {
    margin-bottom: 32px;

    h1 {
        font-size: 24px;
        font-weight: 700;
        margin-bottom: 6px;
    }

    p {
        font-size: 14px;
        color: var(--erp-text-muted);
    }
}

.login-tenant-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 6px 14px;
    background: var(--erp-primary-light);
    border-radius: 8px;
    font-size: 12px;
    font-weight: 500;
    color: var(--erp-primary);
    margin-bottom: 24px;
}

.login-form {
    .form-group { margin-bottom: 20px; }

    label {
        display: block;
        font-size: 13px;
        font-weight: 500;
        color: var(--erp-text);
        margin-bottom: 6px;
    }

    .form-input {
        width: 100%;
        padding: 10px 14px;
        border: 1px solid var(--erp-border);
        border-radius: var(--erp-radius-sm);
        font-size: 14px;
        background: var(--erp-content-bg);
        color: var(--erp-text);
        transition: all var(--erp-transition);

        &:focus {
            outline: none;
            border-color: var(--erp-primary);
            box-shadow: 0 0 0 3px var(--erp-primary-light);
        }
    }

    .password-wrapper { position: relative; }

    .password-toggle {
        position: absolute;
        right: 10px;
        top: 50%;
        transform: translateY(-50%);
        background: none;
        border: none;
        color: var(--erp-text-muted);
        cursor: pointer;
        font-size: 16px;
        padding: 4px;
    }

    .form-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 24px;
    }

    .form-check {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        color: var(--erp-text-muted);

        input {
            width: 16px;
            height: 16px;
            accent-color: var(--erp-primary);
        }
    }

    .forgot-link {
        font-size: 13px;
        color: var(--erp-primary);
        text-decoration: none;

        &:hover { text-decoration: underline; }
    }
}

.login-btn {
    width: 100%;
    padding: 12px;
    background: var(--erp-primary);
    color: #fff;
    border: none;
    border-radius: var(--erp-radius-sm);
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all var(--erp-transition);

    &:hover {
        background: var(--erp-primary-dark);
        box-shadow: var(--erp-shadow-md);
    }
}

.login-divider {
    display: flex;
    align-items: center;
    gap: 16px;
    margin: 24px 0;
    font-size: 12px;
    color: var(--erp-text-muted);

    &::before, &::after {
        content: '';
        flex: 1;
        height: 1px;
        background: var(--erp-border);
    }
}

.login-external {
    display: flex;
    gap: 10px;

    .ext-btn {
        flex: 1;
        padding: 10px;
        border: none;
        border-radius: var(--erp-radius-sm);
        background: var(--erp-content-bg);
        color: var(--erp-text);
        font-size: 13px;
        cursor: pointer;
        transition: all var(--erp-transition);
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;

        &:hover {
            background: var(--erp-primary-light);
            color: var(--erp-primary);
        }
    }
}

.login-footer {
    margin-top: 32px;
    text-align: center;
    font-size: 12px;
    color: var(--erp-text-muted);

    a { color: var(--erp-primary); text-decoration: none; }
    a:hover { text-decoration: underline; }
}

@include media-breakpoint-down(lg) {
    .login-left { display: none; }
    .login-right { width: 100%; border-radius: 0; }
}

@include media-breakpoint-down(sm) {
    .login-right { padding: 32px 24px; }
}
```

- [ ] **Step 3: 创建 Layout.cshtml**

```cshtml
@using OrchardCore.DisplayManagement
@using OrchardCore.DisplayManagement.ModelBinding
@using OrchardCore.Environment.Shell
@using OrchardCore.Themes.Services
@using OrchardCore.Users.Models

@inject ThemeTogglerService ThemeTogglerService
@inject IDisplayManager<Navbar> DisplayManager
@inject IUpdateModelAccessor UpdateModelAccessor
@inject ShellSettings ShellSettings
@{
    var adminSettings = Site.GetOrCreate<AdminSettings>();

    // Branding and Navbar are pre-rendered to allow resource injection.
    var brandingHtml = await DisplayAsync(await New.AdminBranding());
    var navbar = await DisplayAsync(await DisplayManager.BuildDisplayAsync(UpdateModelAccessor.ModelUpdater, "DetailAdmin"));
}
<!DOCTYPE html>
<html lang="@Orchard.CultureName()" dir="@Orchard.CultureDir()" data-bs-theme="@await ThemeTogglerService.CurrentTheme()" data-tenant="@ThemeTogglerService.CurrentTenant">
<head>
    <title>@RenderTitleSegments(Site.SiteName, "before")</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <meta http-equiv="x-ua-compatible" content="ie=edge">

    <!-- This script can't wait till the footer -->
    <script asp-name="admin-main" version="1" at="Head"></script>

    <style asp-name="ErpAdminLayout" asp-src="~/ErpAdmin/css/erp-admin-layout.min.css" debug-src="~/ErpAdmin/css/erp-admin-layout.css" at="Head"></style>

    @if (Orchard.IsRightToLeft())
    {
        <style asp-name="bootstrap-rtl" version="5" at="Head"></style>
        <style media="all" asp-name="erp-admin" version="1" depends-on="bootstrap-rtl,ErpAdminLayout" at="Head"></style>
    }
    else
    {
        <style asp-name="bootstrap" version="5" at="Head"></style>
        <style media="all" asp-name="erp-admin" version="1" depends-on="bootstrap,ErpAdminLayout" at="Head"></style>
    }
    <style asp-name="font-awesome" at="Head" version="7"></style>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:ital,opsz,wght@0,9..40,300;0,9..40,400;0,9..40,500;0,9..40,600;0,9..40,700&family=Space+Grotesk:wght@400;500;600;700&display=swap" rel="stylesheet">
    <script asp-name="erp-admin" version="1" at="Foot"></script>

    <resources type="Header" />

    @await RenderSectionAsync("HeadMeta", required: false)
</head>
<body class="preload">
    <div class="erp-wrapper">
        <aside class="erp-sidebar" id="erp-sidebar">
            <div class="erp-sidebar-brand">
                @brandingHtml
            </div>
            <nav class="erp-sidebar-nav">
                @await RenderSectionAsync("Navigation", required: false)
            </nav>
            <div class="sidebar-footer">
                <button type="button" class="sidebar-toggle-btn" onclick="toggleSidebar()" title="@T["Collapse / expand menu"]">
                    <i class="bi bi-chevron-double-left"></i>
                </button>
            </div>
        </aside>

        <div class="sidebar-overlay" id="erp-sidebar-overlay" onclick="closeMobileSidebar()"></div>

        <div class="erp-main">
            <header class="erp-topbar">
                <div class="topbar-left">
                    <button class="mobile-menu-btn" onclick="openMobileSidebar()"><i class="bi bi-list"></i></button>
                    <div class="topbar-search">
                        <i class="bi bi-search search-icon"></i>
                        <input type="text" placeholder="@T["Search..."]">
                    </div>
                </div>
                <div class="topbar-right">
                    <div class="topbar-tenant">
                        <i class="bi bi-building"></i>
                        <span class="tenant-name">@ShellSettings.Name</span>
                    </div>
                    @navbar
                </div>
            </header>

            <main class="erp-content" style="--oc-notify-offset-top: var(--erp-topbar-height);">
                @await RenderSectionAsync("Messages", required: false)
                @await RenderSectionAsync("Breadcrumbs", required: false)
                @await RenderSectionAsync("Title", required: false)
                @await RenderBodyAsync()
            </main>

            @await RenderSectionAsync("Footer", required: false)
        </div>
    </div>
    <div id="confirmRemoveModalMetadata" data-title="@T["Delete"]" data-message="@T["Are you sure you want to remove this element?"]" data-ok-text="@T["Ok"]" data-cancel-text="@T["Cancel"]" data-ok-class="btn-danger" data-cancel-class="btn-secondary"></div>
    <resources type="Footer" />
</body>
</html>
```

- [ ] **Step 4: 创建 Layout-Login.cshtml**

```cshtml
@using OrchardCore.Environment.Shell
@using OrchardCore.Themes.Services

@inject ThemeTogglerService ThemeTogglerService
@inject ShellSettings ShellSettings

<!DOCTYPE html>
<html lang="@Orchard.CultureName()" dir="@Orchard.CultureDir()" data-bs-theme="@await ThemeTogglerService.CurrentTheme()" data-tenant="@ThemeTogglerService.CurrentTenant">
<head>
    <title>@RenderTitleSegments(Site.SiteName, "before")</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <meta http-equiv="x-ua-compatible" content="ie=edge">
    <link type="image/x-icon" rel="shortcut icon" href="~/TheAdmin/favicon.ico" />

    <!-- This script can't wait till the footer -->
    <script asp-name="admin-main" version="1" at="Head"></script>

    @if (Orchard.IsRightToLeft())
    {
        <style asp-name="bootstrap-rtl" version="5" at="Head"></style>
        <style asp-name="ErpAdminLogin" depends-on="bootstrap-rtl" asp-src="~/ErpAdmin/css/login.min.css" debug-src="~/ErpAdmin/css/login.css" at="Foot"></style>
    }
    else
    {
        <style asp-name="bootstrap" version="5" at="Head"></style>
        <style asp-name="ErpAdminLogin" asp-src="~/ErpAdmin/css/login.min.css" debug-src="~/ErpAdmin/css/login.css" at="Foot"></style>
    }

    <style asp-name="font-awesome" at="Head" version="7"></style>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:ital,opsz,wght@0,9..40,300;0,9..40,400;0,9..40,500;0,9..40,600;0,9..40,700&family=Space+Grotesk:wght@400;500;600;700&display=swap" rel="stylesheet">

    <resources type="Header" />
    @await RenderSectionAsync("HeadMeta", required: false)
</head>
<body>
    <div class="login-page">
        <div class="login-left">
            <div class="login-brand">
                <div class="brand-logo">E</div>
                <span class="brand-name">@Site.SiteName</span>
            </div>
            <div class="login-illustration">
                <div class="illus-card">
                    <div class="illus-title">@T["Enterprise ERP"]<br>@T["Management Platform"]</div>
                    <div class="illus-desc">@T["Modular SaaS management system built on OrchardCore CMS. All business operations are completed in the admin panel, with fine-grained role-based access control."]</div>
                    <div class="illus-features">
                        <div class="illus-feature">
                            <div class="feat-icon blue"><i class="bi bi-shield-lock"></i></div>
                            <span>@T["Fine-grained role permissions, data security guaranteed"]</span>
                        </div>
                        <div class="illus-feature">
                            <div class="feat-icon green"><i class="bi bi-puzzle"></i></div>
                            <span>@T["Modular architecture, independent module rendering"]</span>
                        </div>
                        <div class="illus-feature">
                            <div class="feat-icon amber"><i class="bi bi-building"></i></div>
                            <span>@T["Multi-tenant SaaS support, independent brand customization"]</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="login-right">
            <div class="login-form-header">
                <h1>@T["Log in"]</h1>
                <p>@T["Please enter your username and password"]</p>
            </div>
            <div class="login-tenant-badge">
                <i class="bi bi-building"></i> @ShellSettings.Name
            </div>

            @await RenderSectionAsync("Header", required: false)
            @await RenderSectionAsync("Messages", required: false)

            @await RenderBodyAsync()

            <div class="login-footer">
                &copy; @DateTime.UtcNow.Year @Site.SiteName
            </div>
        </div>
    </div>

    @await RenderSectionAsync("Footer", required: false)

    <resources type="Footer" />
</body>
</html>
```

- [ ] **Step 5: 创建 Layout-TwoFactor.cshtml**

```cshtml
@using OrchardCore.Environment.Shell
@using OrchardCore.Themes.Services

@inject ThemeTogglerService ThemeTogglerService

<!DOCTYPE html>
<html lang="@Orchard.CultureName()" dir="@Orchard.CultureDir()" data-bs-theme="@await ThemeTogglerService.CurrentTheme()" data-tenant="@ThemeTogglerService.CurrentTenant">
<head>
    <title>@RenderTitleSegments(Site.SiteName, "before")</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <meta http-equiv="x-ua-compatible" content="ie=edge">
    <link type="image/x-icon" rel="shortcut icon" href="~/TheAdmin/favicon.ico" />

    <!-- This script can't wait till the footer -->
    <script asp-name="admin-main" version="1" at="Head"></script>

    @if (Orchard.IsRightToLeft())
    {
        <style asp-name="bootstrap-rtl" version="5" at="Head"></style>
    }
    else
    {
        <style asp-name="bootstrap" version="5" at="Head"></style>
    }

    <style>
        body, html {
            height: 100%;
        }
    </style>

    <style asp-name="font-awesome" at="Head" version="7"></style>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">

    <resources type="Header" />
    @await RenderSectionAsync("HeadMeta", required: false)
</head>
<body>
    <div class="d-flex align-items-center justify-content-center h-100">
        <div class="container">
            @await RenderSectionAsync("Header", required: false)
            @await RenderSectionAsync("Messages", required: false)

            @await RenderBodyAsync()
        </div>
    </div>

    @await RenderSectionAsync("Footer", required: false)

    <resources type="Footer" />
</body>
</html>
```

- [ ] **Step 6: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Views/Layout.cshtml src/OrchardCore.Themes/ErpAdmin/Views/Layout-Login.cshtml src/OrchardCore.Themes/ErpAdmin/Views/Layout-TwoFactor.cshtml src/OrchardCore.Themes/ErpAdmin/Assets/scss/erp-admin-layout.scss src/OrchardCore.Themes/ErpAdmin/Assets/scss/login.scss
git commit -m "feat(erpadmin): add layout overrides with sidebar, topbar, and login page"
```

---

### Task 4: 导航形状

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Navigation-admin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/NavigationItem-admin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/NavigationItemLink-admin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/NavigationItemText.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/AdminBranding.cshtml`

- [ ] **Step 1: 创建 Navigation-admin.cshtml**

```cshtml
@inject OrchardCore.Settings.ISiteService siteService
@using OrchardCore.Entities
@using OrchardCore.Admin.Models
@using OrchardCore.Navigation
@{
    var adminSettings = (await siteService.GetSiteSettingsAsync()).GetOrCreate<AdminSettings>();
    TagBuilder tag = Tag(Model, "ul");
    tag.Attributes["Id"] = NavigationConstants.AdminMenuId;
    tag.AddCssClass("nav-submenu-root");

    foreach (var item in Model)
    {
        tag.InnerHtml.AppendHtml(await DisplayAsync(item));
    }
}
@if (adminSettings.DisplayMenuFilter)
{
    <input id="filter" class="form-control" placeholder="@T["Filter"]" type="search" data-bs-toggle="tooltip" data-bs-placement="right" data-html="true" title="Ctrl+Shift+F" />
}
<nav id="left-nav">
    @tag
</nav>
@if (adminSettings.DisplayMenuFilter)
{
<script at="Foot">
    $(document).bind('keydown', function (e) {
        if (e.ctrlKey && e.shiftKey && e.which == 70) {
            $('#filter').focus();
            return false;
        }
    });
    jQuery.expr[":"].contains = jQuery.expr.createPseudo(function (arg) {
        return function (elem) {
            return jQuery(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
        };
    });

    function hasChild(list) {
        var filter = $('#filter').val();
        list.children('li').each(function () {
            if ($(this).find("span:contains(" + filter + ")").length > 0) {
                if ($(this).find('ul :first').length > 0) {
                    $(this).show();
                    hasChild($(this).find('ul :first').parent());
                } else {
                    $(this).show();
                }
            } else {
                $(this).hide();
            }
        });
    };
    $('#filter').keyup(function () {
        var list = $('#adminMenu');
        var filter = $('#filter').val();
        if (filter) {
            hasChild(list);
        } else {
            list.find("li").show();
        }
        return false;
    });
</script>
}
```

- [ ] **Step 2: 创建 NavigationItem-admin.cshtml**

```cshtml
@using OrchardCore.Navigation
@{
    TagBuilder li = Tag(Model, "li");
    li.AddCssClass("nav-item-erp");

    // Morphing the shape to keep Model untouched.
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "NavigationItemLink";

    if (Model.Id != null)
    {
        Model.Metadata.Alternates.Add("NavigationItemLink_Id__" + Model.Id);
    }

    // Extract classes that are not icons from 'Model.Classes'.
    var notIconClasses = ((IEnumerable<string>)Model.Classes)
        .Where(c => !c.StartsWith(NavigationConstants.CssClassPrefix, StringComparison.OrdinalIgnoreCase))
        .ToArray();

    if (notIconClasses.Length > 0)
    {
        li.Attributes["class"] = string.Join(' ', notIconClasses);
        li.AddCssClass("nav-item-erp");
    }
    else
    {
        li.Attributes.Remove("class");
        li.AddCssClass("nav-item-erp");
    }

    if ((bool)Model.Selected)
    {
        li.AddCssClass("active");
    }

    // Render sub-items (MenuItem).
    if (Model.HasItems)
    {
        li.AddCssClass("has-items");
        var ul = new TagBuilder("ul");
        ul.AddCssClass("collapse");
        ul.AddCssClass("nav-submenu");
        ul.Attributes["Id"] = "m" + Model.GetHashCode().ToString();
        ul.Attributes["data-title"] = Model.Text;

        if ((bool)Model.Selected)
        {
            ul.AddCssClass("show");
        }

        foreach (var item in Model)
        {
            ul.InnerHtml.AppendHtml(await DisplayAsync(item));
        }

        li.InnerHtml.AppendHtml(await DisplayAsync(Model));
        li.InnerHtml.AppendHtml(ul);
    }
    else
    {
        li.InnerHtml.AppendHtml(await DisplayAsync(Model));
    }
}

@li
```

- [ ] **Step 3: 创建 NavigationItemLink-admin.cshtml**

```cshtml
@using OrchardCore.Localization
@using OrchardCore.Navigation
@{
    // Morphing the shape to keep Model untouched.
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "NavigationItemText";

    if (Model.Id != null)
    {
        Model.Metadata.Alternates.Add("NavigationItemText_Id__" + Model.Id);
    }

    var isToggleOnlyItem = Model.HasItems && (Model.Href == null || Model.Href.ToString() == "#");
    TagBuilder tag = Tag(Model, isToggleOnlyItem ? "button" : "a");
    tag.Attributes["id"] = null;
    tag.Attributes["data-admin-hash"] = Model.Hash;
    tag.AddCssClass("nav-link-erp");

    if (isToggleOnlyItem)
    {
        tag.Attributes["type"] = "button";
    }
    else
    {
        tag.Attributes["href"] = Model.Href;
    }

    if (!string.IsNullOrEmpty(Model.Target))
    {
        tag.Attributes["target"] = Model.Target;
    }

    if (Model.HasItems)
    {
        tag.Attributes["data-bs-toggle"] = "collapse";
        tag.Attributes["data-bs-target"] = "#m" + Model.GetHashCode().ToString();
        tag.Attributes["aria-expanded"] = "false";
        tag.Attributes["aria-controls"] = "m" + Model.GetHashCode().ToString();
        tag.TagRenderMode = TagRenderMode.Normal;

        if ((bool)Model.Selected)
        {
            tag.Attributes["aria-expanded"] = "true";
        }
    }

    tag.InnerHtml.AppendHtml(await DisplayAsync(Model));

    if (Model.HasItems)
    {
        tag.InnerHtml.AppendHtml(Html.Raw("<span class=\"nav-expand\"><i class=\"bi bi-chevron-right\"></i></span>"));
    }
}

@tag
```

- [ ] **Step 4: 创建 NavigationItemText.cshtml**

```cshtml
@using System.Linq
@using OrchardCore
@using OrchardCore.AdminMenu
@using OrchardCore.Localization.Data

@inject IDataLocalizer D

@{
    var prefix = "icon-class-";

    //extract icon classes from Model.Classes
    var iconClasses = ((IEnumerable<string>)Model.Classes)
                    .ToList()
                    .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Substring(prefix.Length));

    var context = OrchardCore.AdminMenu.DataLocalizationContext.AdminMenu(Model.Item.MenuName);
}

@if (iconClasses.Any())
{
    <span class="nav-icon"><i class="@string.Join(" ", iconClasses)" aria-hidden="true"></i></span>
}
else
{
    <span class="nav-icon"><i class="bi bi-circle" aria-hidden="true" style="font-size:8px;"></i></span>
}

<span class="nav-label">@D[Model.Text, context]</span>
```

- [ ] **Step 5: 创建 AdminBranding.cshtml**

```cshtml
@inject IOptions<AdminOptions> AdminOptions
<zone name="HeadMeta">
    <link asp-src="~/OrchardCore.Admin/favicon.ico" type="image/x-icon" rel="shortcut icon" />
</zone>
<a class="navbar-brand" href="@Url.Content("~/" + AdminOptions.Value.AdminUrlPrefix)" style="display:flex;align-items:center;text-decoration:none;">
    <div class="brand-logo">E</div>
    <span class="brand-text">@Site.SiteName</span>
</a>
```

- [ ] **Step 6: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Views/Navigation-admin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/NavigationItem-admin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/NavigationItemLink-admin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/NavigationItemText.cshtml src/OrchardCore.Themes/ErpAdmin/Views/AdminBranding.cshtml
git commit -m "feat(erpadmin): add sidebar navigation shapes with multi-level support"
```

---

### Task 5: 导航栏和用户菜单形状

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Navbar.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/NavbarUserMenu.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/UserMenu.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/UserMenuItems-SignedUser.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/UserMenuItems-SignOut.cshtml`

- [ ] **Step 1: 创建 Navbar.DetailAdmin.cshtml**

```cshtml
@if (Model.Content == null)
{
    return;
}

<button class="topbar-icon-btn erp-theme-toggle-btn" title="@T["Toggle theme"]" onclick="toggleErpTheme()">
    <i class="bi bi-moon-stars"></i>
</button>

@await DisplayAsync(Model.Content)
```

- [ ] **Step 2: 创建 NavbarUserMenu.cshtml**

```cshtml
@using OrchardCore.DisplayManagement.ModelBinding
@using OrchardCore.Users.Models

@inject IDisplayManager<UserMenu> DisplayManager
@inject IUpdateModelAccessor UpdateModelAccessor

@await DisplayAsync(await DisplayManager.BuildDisplayAsync(UpdateModelAccessor.ModelUpdater, (string)Model.Metadata.DisplayType))
```

- [ ] **Step 3: 创建 UserMenu.cshtml**

```cshtml
@if (!User.Identity.IsAuthenticated)
{
    @if (Model.AnonymousContent?.Items != null && Model.AnonymousContent.Items.Count > 0)
    {
        <a class="topbar-icon-btn" asp-route-area="OrchardCore.Users" asp-controller="Account" asp-action="Login" title="@T["Log in"]">
            <i class="bi bi-box-arrow-in-right"></i>
        </a>
        return;
    }

    <a class="topbar-icon-btn" asp-route-area="OrchardCore.Users" asp-controller="Account" asp-action="Login" title="@T["Log in"]">
        <i class="bi bi-box-arrow-in-right"></i>
    </a>
    return;
}

<div class="topbar-user" onclick="toggleUserDropdown()">
    <div class="user-avatar">@(User.Identity.Name?.Substring(0, 1).ToUpper())</div>
    <div class="user-info">
        <div class="user-name">@User.Identity.Name</div>
        <div class="user-role">@T["Administrator"]</div>
    </div>
    <div class="topbar-user-dropdown" id="erp-user-dropdown">
        @if (Model.Header != null)
        {
            @await DisplayAsync(Model.Header)
        }
        @if (Model.Content != null)
        {
            @await DisplayAsync(Model.Content)
        }
    </div>
</div>
```

- [ ] **Step 4: 创建 UserMenuItems-SignedUser.cshtml**

```cshtml
<div class="dropdown-header">
    @T["Signed in as"] <strong>@User.Identity.Name</strong>
</div>
```

- [ ] **Step 5: 创建 UserMenuItems-SignOut.cshtml**

```cshtml
<a class="dropdown-item danger" href="#" onclick="document.getElementById('erp-logout-form').submit(); return false;">
    <i class="bi bi-box-arrow-right"></i> @T["Log off"]
</a>
<form id="erp-logout-form" asp-route-area="OrchardCore.Users" asp-controller="Account" asp-action="LogOff" method="post" class="d-none no-multisubmit">
    @Html.AntiForgeryToken()
</form>
```

- [ ] **Step 6: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Views/Navbar.DetailAdmin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/NavbarUserMenu.cshtml src/OrchardCore.Themes/ErpAdmin/Views/UserMenu.cshtml src/OrchardCore.Themes/ErpAdmin/Views/UserMenuItems-SignedUser.cshtml src/OrchardCore.Themes/ErpAdmin/Views/UserMenuItems-SignOut.cshtml
git commit -m "feat(erpadmin): add navbar and user menu shapes with dropdown"
```

---

### Task 6: 内容显示形状 (DetailAdmin)

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Content.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/ContentPart.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Content.SummaryAdmin.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/ContentsAdminList.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Stereotype.DetailAdmin.cshtml`

- [ ] **Step 1: 创建 Content.DetailAdmin.cshtml**

```cshtml
@using OrchardCore.Mvc.Utilities
@using OrchardCore.ContentManagement

@{
    Model.Classes.Add("content-item");
    ContentItem contentItem = Model.ContentItem;
    var contentTypeClassName = contentItem.ContentType.HtmlClassify();
    Model.Classes.Add(contentTypeClassName);

    var tag = Tag(Model, "article");
}

<zone Name="Title"><h1>@RenderTitleSegments(contentItem.DisplayText)</h1></zone>

<div class="detail-header">
    <div class="detail-title-row">
        <h2>@contentItem.DisplayText</h2>
        @if (Model.Meta != null)
        {
            @await DisplayAsync(Model.Meta)
        }
    </div>
    <div class="detail-meta">
        <span><i class="bi bi-upc-scan"></i> @contentItem.ContentItemId.Substring(0, 8)</span>
        <span><i class="bi bi-folder"></i> @contentItem.ContentType</span>
        <span><i class="bi bi-calendar"></i> @contentItem.CreatedUtc?.ToString("yyyy-MM-dd")</span>
        <span><i class="bi bi-person"></i> @contentItem.Owner</span>
    </div>
</div>

<div class="detail-body">
    <div class="detail-main">
        <article>
            @if (Model.Header != null)
            {
                <div class="detail-card">
                    <div class="detail-card-header"><i class="bi bi-info-circle"></i> @T["Header"]</div>
                    <div class="detail-card-body">
                        @await DisplayAsync(Model.Header)
                    </div>
                </div>
            }
            @await DisplayAsync(Model.Content)
            @if (Model.Footer != null)
            {
                <div class="detail-card">
                    <div class="detail-card-header"><i class="bi bi-link-45deg"></i> @T["Footer"]</div>
                    <div class="detail-card-body">
                        @await DisplayAsync(Model.Footer)
                    </div>
                </div>
            }
        </article>
    </div>
    <div class="detail-sidebar-panel">
        @if (Model.Actions != null)
        {
            <div class="detail-card">
                <div class="detail-card-header"><i class="bi bi-lightning"></i> @T["Actions"]</div>
                <div class="detail-card-body" style="display:flex;flex-direction:column;gap:8px;">
                    @await DisplayAsync(Model.Actions)
                </div>
            </div>
        }
    </div>
</div>
```

- [ ] **Step 2: 创建 ContentPart.DetailAdmin.cshtml**

```cshtml
@using OrchardCore.Mvc.Utilities

@{
    string name = Model.Metadata.Name;
}

@if (Model.Content != null)
{
    <div class="detail-card">
        <div class="detail-card-header">
            <i class="bi bi-puzzle"></i> @name.HtmlClassify()
        </div>
        <div class="detail-card-body">
            <div class="contentpart contentpart-@name.HtmlClassify()">
                @await DisplayAsync(Model.Content)
            </div>
        </div>
    </div>
}
```

- [ ] **Step 3: 创建 Content.SummaryAdmin.cshtml**

```cshtml
@using OrchardCore.Contents
@using OrchardCore.ContentManagement
@using Microsoft.AspNetCore.Html
@inject IAuthorizationService AuthorizationService
@{
    ContentItem contentItem = Model.ContentItem;
}

<div class="row">
    <div class="col-lg col-12 title d-flex align-items-center">
        <div class="selectors-container d-flex">
            @if (Model.Selectors != null)
            {
                <div class="selectors cursor-pointer">
                    @await DisplayAsync(Model.Selectors)
                </div>
            }
            @if (Model.Checkbox != null)
            {
                @await DisplayAsync(Model.Checkbox)
            }
        </div>
        <div class="summary d-flex flex-column flex-md-row">
            <div class="contentitem me-2">
                @if (await AuthorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
                {
                    <a admin-for="@contentItem" asp-route-returnUrl="@FullRequestPath" class="cell-link" />
                }
                else
                {
                    @contentItem
                }
            </div>
            <div class="contenttype me-1">
                <span class="status-badge neutral"><span class="status-dot"></span> @contentItem.ContentType</span>
            </div>
            @if (Model.Header != null)
            {
                <div class="header me-1">
                    @await DisplayAsync(Model.Header)
                </div>
            }
            @if (Model.Tags != null)
            {
                <div class="tags me-1">
                    @await DisplayAsync(Model.Tags)
                </div>
            }
            @if (Model.Meta != null)
            {
                <div class="metadata me-1">
                    @await DisplayAsync(Model.Meta)
                </div>
            }
        </div>
    </div>
    <div class="col-lg-auto col-12 d-flex justify-content-end">
        <div class="actions">
            @if (Model.Actions != null)
            {
                @await DisplayAsync(Model.Actions)
            }

            @if (Model.ActionsMenu != null && Model.ActionsMenu.HasItems)
            {
                <button type="button" class="action-dots" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    <i class="bi bi-three-dots-vertical"></i>
                </button>
                <div class="dropdown-menu dropdown-menu-end">
                    @await DisplayAsync(Model.ActionsMenu)
                </div>
            }
        </div>
    </div>
</div>

@if (Model.Content != null)
{
    <div class="row">
        <div class="col primary">
            @await DisplayAsync(Model.Content)
        </div>
    </div>
}
```

- [ ] **Step 4: 创建 ContentsAdminList.cshtml**

```cshtml
@model ListContentsViewModel

<script asp-name="bootstrap-select" at="Foot"></script>

<!-- Hidden submit button do not remove -->
<input type="submit" name="submit.Filter" id="submitFilter" class="visually-hidden" />
<input type="submit" name="submit.BulkAction" class="visually-hidden" />
@Html.HiddenFor(o => o.Options.BulkAction)

<div class="erp-table-card">
    <div class="table-toolbar">
        <div class="table-toolbar-left">
            @await DisplayAsync(Model.Header.Search)
        </div>
        <div class="table-toolbar-right">
            @await DisplayAsync(Model.Header.Create)
        </div>
    </div>
    <div style="overflow-x:auto;">
        <table class="erp-table">
            <thead>
                <tr>
                    <th>@await DisplayAsync(Model.Header.Summary)</th>
                    <th class="text-end">@await DisplayAsync(Model.Header.Actions)</th>
                </tr>
            </thead>
            <tbody>
                @if (Model.ContentItems.Count > 0)
                {
                    @foreach (var contentItemSummary in Model.ContentItems)
                    {
                        <tr>
                            <td>@await DisplayAsync(contentItemSummary)</td>
                            <td class="text-end"></td>
                        </tr>
                    }
                }
                else
                {
                    <tr>
                        <td colspan="2">
                            <div class="erp-empty-state">
                                <div class="empty-icon"><i class="bi bi-inbox"></i></div>
                                <div class="empty-title">@T["No results found"]</div>
                                <div class="empty-desc">@T["Try adjusting your search or filter criteria"]</div>
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
    <div class="table-footer">
        @await DisplayAsync(Model.Pager)
    </div>
</div>

<script at="Foot" depends-on="jQuery">
    $(function () {
        var actions = $("#actions");
        var items = $("#items");
        var filters = $(".filter");
        var selectAllCtrl = $("#select-all");
        var selectedItems = $("#selected-items");
        var itemsCheckboxes = $(":checkbox[name='itemIds']");

        $('.selectpicker:not(.nosubmit)').on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
            $("[name='submit.Filter']").click();
        });

        $(".dropdown-menu .dropdown-item").filter(function () {
            return $(this).data("action");
        }).on("click", function () {
            if ($(":checkbox[name='itemIds']:checked").length > 1) {
                var $this = $(this);
                confirmDialog({
                    ...$this.data(), callback: function (r) {
                        if (r) {
                            $("[name='Options.BulkAction']").val($this.data("action"));
                            $("[name='submit.BulkAction']").click();
                        }
                    }
                });
            }
        });

        function displayActionsOrFilters() {
            if ($(":checkbox[name='itemIds']:checked").length > 1) {
                actions.show();
                filters.hide();
                selectedItems.show();
                items.hide();
            }
            else {
                actions.hide();
                filters.show();
                selectedItems.hide();
                items.show();
            }
        }

        selectAllCtrl.click(function () {
            itemsCheckboxes.not(this).prop("checked", this.checked);
            selectedItems.text($(":checkbox[name='itemIds']:checked").length + ' @T["selected"]');
            displayActionsOrFilters();
        });

        itemsCheckboxes.on("click", function () {
            var itemsCount = $(":checkbox[name='itemIds']").length;
            var selectedItemsCount = $(":checkbox[name='itemIds']:checked").length;

            selectAllCtrl.prop("checked", selectedItemsCount == itemsCount);
            selectAllCtrl.prop("indeterminate", selectedItemsCount > 0 && selectedItemsCount < itemsCount);

            selectedItems.text(selectedItemsCount + ' @T["selected"]');
            displayActionsOrFilters();
        });
    })
</script>
```

- [ ] **Step 5: 创建 Stereotype.DetailAdmin.cshtml**

```cshtml
@{
    Model.Metadata.Type = "Content_DetailAdmin";
    Model.Metadata.Alternates.Clear();
    @await DisplayAsync(Model)
}
```

- [ ] **Step 6: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Views/Content.DetailAdmin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/ContentPart.DetailAdmin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/Content.SummaryAdmin.cshtml src/OrchardCore.Themes/ErpAdmin/Views/ContentsAdminList.cshtml src/OrchardCore.Themes/ErpAdmin/Views/Stereotype.DetailAdmin.cshtml
git commit -m "feat(erpadmin): add DetailAdmin content display shapes with card layout"
```

---

### Task 7: 登录和认证形状

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Account/Login.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/LoginFormCredentials.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/LoginFormForgotPassword.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/LoginFormRegisterUser.cshtml`

- [ ] **Step 1: 创建 Account/Login.cshtml**

```cshtml
@using Microsoft.AspNetCore.Identity
@using OrchardCore.Users.Models

@inject SignInManager<IUser> SignInManager

@{
    ViewLayout = "Layout__Login";

    var loginProviders = await SignInManager.GetExternalAuthenticationSchemesAsync();
    var disableLocalLogin = Site.GetOrCreate<LoginSettings>().DisableLocalLogin;
    var hasExternalProviders = loginProviders.Any();
}

<style asp-name="font-awesome" at="Head" version="7"></style>

@if (!ViewContext.ModelState.IsValid)
{
    <div class="mb-3">
        <div asp-validation-summary="ModelOnly"></div>
    </div>
}

@if (!disableLocalLogin)
{
    <form class="login-form" asp-route-area="OrchardCore.Users" asp-controller="Account" asp-action="Login" asp-route-returnurl="@ViewData["ReturnUrl"]" method="post" novalidate>
        @await DisplayAsync(Model)
    </form>
}

@if (hasExternalProviders)
{
    <div class="login-divider">@T["Or sign in with another service"]</div>
    <div class="login-external">
        <form asp-controller="ExternalAuthentications" asp-action="ExternalLogin" asp-route-returnurl="@ViewData["ReturnUrl"]" method="post" class="no-multisubmit" style="display:contents;">
            @foreach (var provider in loginProviders)
            {
                <button type="submit" class="ext-btn" name="provider" value="@provider.Name" title="@T["Log in using your {0} account", provider.DisplayName]">
                    <i class="bi bi-box-arrow-in-right"></i> @provider.DisplayName
                </button>
            }
        </form>
    </div>
}
```

- [ ] **Step 2: 创建 LoginFormCredentials.cshtml**

```cshtml
@model LoginViewModel

<div class="form-group" asp-validation-class-for="UserName">
    <label asp-for="UserName">@T["Username or email address"]</label>
    <input asp-for="UserName" class="form-input" autofocus tabindex="1" placeholder="@T["Enter your username"]" />
    <span asp-validation-for="UserName" class="text-danger"></span>
</div>

<div class="form-group" asp-validation-class-for="Password">
    <label asp-for="Password">@T["Password"]</label>
    <div class="password-wrapper">
        <input asp-for="Password" class="form-input" tabindex="2" placeholder="@T["Enter your password"]" />
        <button tabindex="-1" class="password-toggle" type="button" id="togglePassword"><i class="bi bi-eye"></i></button>
    </div>
    <span asp-validation-for="Password" class="text-danger"></span>
</div>

<div class="form-row">
    <div class="form-check">
        <input asp-for="RememberMe" tabindex="3">
        <label asp-for="RememberMe">@T["Remember me"]</label>
    </div>
    <a asp-controller="ResetPassword" asp-action="ForgotPassword" class="forgot-link">@T["Forgot your password?"]</a>
</div>

<button type="submit" class="login-btn" tabindex="4">@T["Log in"]</button>

<script at="Foot">
    let togglePassword = document.querySelector('#togglePassword');
    let password = document.querySelector('#@Html.IdFor(m => m.Password)');

    togglePassword.addEventListener('click', function (e) {
        type = password.getAttribute('type') === 'password' ? 'text' : 'password';
        password.setAttribute('type', type);

        const icon = this.querySelector('i');
        if (type === 'password') {
            icon.classList.remove('bi-eye-slash');
            icon.classList.add('bi-eye');
        } else {
            icon.classList.remove('bi-eye');
            icon.classList.add('bi-eye-slash');
        }
    });
</script>
```

- [ ] **Step 3: 创建 LoginFormForgotPassword.cshtml**

```cshtml
<div style="text-align:center;margin-top:16px;">
    <a asp-controller="ResetPassword" asp-action="ForgotPassword" class="forgot-link">@T["Forgot your password?"]</a>
</div>
```

- [ ] **Step 4: 创建 LoginFormRegisterUser.cshtml**

```cshtml
<div style="text-align:center;margin-top:16px;">
    <a asp-controller="Registration" asp-action="Register" asp-route-returnurl="@ViewData["ReturnUrl"]" class="forgot-link">@T["Register as a new user"]</a>
</div>
```

- [ ] **Step 5: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Views/Account/Login.cshtml src/OrchardCore.Themes/ErpAdmin/Views/LoginFormCredentials.cshtml src/OrchardCore.Themes/ErpAdmin/Views/LoginFormForgotPassword.cshtml src/OrchardCore.Themes/ErpAdmin/Views/LoginFormRegisterUser.cshtml
git commit -m "feat(erpadmin): add login and auth shapes with ERP-styled forms"
```

---

### Task 8: MetadataContentsHandler

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Handlers/MetadataContentsHandler.cs`

- [ ] **Step 1: 创建 MetadataContentsHandler.cs**

```csharp
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Themes.ErpAdmin.Handlers;

public class MetadataContentsHandler : ContentHandlerBase
{
    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<ContentItemMetadata>(metadata =>
        {
            // Override the display route to point to Admin/Display for the DetailAdmin display type
            metadata.DisplayRouteValues = new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Display" },
                { "ContentItemId", context.ContentItem.ContentItemId }
            };

            metadata.AdminRouteValues = new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Display" },
                { "ContentItemId", context.ContentItem.ContentItemId }
            };

            return Task.CompletedTask;
        });
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Handlers/MetadataContentsHandler.cs
git commit -m "feat(erpadmin): add MetadataContentsHandler to route content display to Admin/Display"
```

---

### Task 9: 租户品牌定制

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Models/TenantBranding.cs`
- Create: `src/OrchardCore.Themes/ErpAdmin/Drivers/TenantBrandingNavbarDisplayDriver.cs`

- [ ] **Step 1: 创建 TenantBranding.cs**

```csharp
namespace OrchardCore.Themes.ErpAdmin.Models;

public class TenantBranding
{
    public string LogoUrl { get; set; }
    public string PrimaryColor { get; set; }
    public string SidebarStyle { get; set; } // "dark" | "light"
    public string BrandName { get; set; }
}
```

- [ ] **Step 2: 创建 TenantBrandingNavbarDisplayDriver.cs**

```csharp
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Themes.ErpAdmin.Drivers;

public sealed class TenantBrandingNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("TenantBrandingStyles", model)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1");
    }
}
```

- [ ] **Step 3: 创建 TenantBrandingStyles.cshtml 视图**

创建文件 `src/OrchardCore.Themes/ErpAdmin/Views/TenantBrandingStyles.cshtml`：

```cshtml
@{
    // This shape injects tenant branding CSS variable overrides into the navbar.
    // In a full implementation, this would read from ISiteService or tenant settings.
    // For now, it renders an empty block that can be extended by tenant-specific modules.
}
```

- [ ] **Step 4: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Models/TenantBranding.cs src/OrchardCore.Themes/ErpAdmin/Drivers/TenantBrandingNavbarDisplayDriver.cs src/OrchardCore.Themes/ErpAdmin/Views/TenantBrandingStyles.cshtml
git commit -m "feat(erpadmin): add TenantBranding model and NavbarDisplayDriver for brand customization"
```

---

### Task 10: SCSS 组件

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_data-table.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_stat-card.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_status-badge.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_action-bar.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_filter-panel.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_detail-panel.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/_empty-state.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/themes/light/_index.scss`
- Create: `src/OrchardCore.Themes/ErpAdmin/Assets/scss/themes/dark/_index.scss`

- [ ] **Step 1: 创建 _data-table.scss**

```scss
.erp-table-card {
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    box-shadow: var(--erp-shadow);
    overflow: hidden;
}

.table-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: none;
    gap: 12px;
    flex-wrap: wrap;
    background: var(--erp-content-bg);
    margin-bottom: 0;
}

.table-toolbar-left {
    display: flex;
    align-items: center;
    gap: 10px;
}

.table-filter-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 7px 14px;
    background: var(--erp-card-bg);
    border: 1px solid var(--erp-border);
    border-radius: var(--erp-radius-sm);
    font-size: 13px;
    color: var(--erp-text);
    cursor: pointer;
    transition: all var(--erp-transition);

    &:hover {
        border-color: var(--erp-primary);
        color: var(--erp-primary);
    }

    &.active {
        background: var(--erp-primary-light);
        border-color: var(--erp-primary);
        color: var(--erp-primary);
    }
}

.table-search {
    position: relative;

    input {
        background: var(--erp-card-bg);
        border: 1px solid var(--erp-border);
        border-radius: var(--erp-radius-sm);
        padding: 7px 12px 7px 32px;
        font-size: 13px;
        width: 200px;
        color: var(--erp-text);

        &:focus {
            outline: none;
            border-color: var(--erp-primary);
        }
    }

    .search-icon {
        position: absolute;
        left: 10px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--erp-text-muted);
        font-size: 13px;
    }
}

.table-toolbar-right {
    display: flex;
    align-items: center;
    gap: 8px;
}

.erp-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;

    thead th {
        padding: 12px 16px;
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: .5px;
        color: var(--erp-text-muted);
        background: var(--erp-content-bg);
        border-bottom: none;
        white-space: nowrap;
        text-align: left;
    }

    tbody td {
        padding: 12px 16px;
        font-size: 13px;
        border-bottom: none;
        vertical-align: middle;
    }

    tbody tr {
        &:nth-child(odd) { background: var(--erp-row-alt); }
        &:nth-child(even) { background: transparent; }
        &:hover { background: var(--erp-primary-light); }
    }

    .cell-id {
        font-family: 'Space Grotesk', monospace;
        font-size: 12px;
        color: var(--erp-text-muted);
    }

    .cell-primary {
        font-weight: 500;
        color: var(--erp-text);
    }

    .cell-link {
        color: var(--erp-primary);
        text-decoration: none;
        font-weight: 500;

        &:hover { text-decoration: underline; }
    }
}

.table-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 20px;
    border-top: none;
    font-size: 13px;
    color: var(--erp-text-muted);
    background: var(--erp-content-bg);
}

.erp-pagination {
    display: flex;
    gap: 4px;

    .page-btn {
        min-width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 6px;
        border: none;
        background: var(--erp-card-bg);
        color: var(--erp-text);
        font-size: 13px;
        cursor: pointer;
        transition: all var(--erp-transition);

        &:hover {
            background: var(--erp-primary-light);
            color: var(--erp-primary);
        }

        &.active {
            background: var(--erp-primary);
            color: #fff;
        }
    }
}
```

- [ ] **Step 2: 创建 _stat-card.scss**

```scss
.stat-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;
    margin-bottom: 24px;
}

.stat-card {
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    padding: 20px;
    box-shadow: var(--erp-shadow);
    transition: all var(--erp-transition);
    position: relative;
    overflow: hidden;

    &:hover {
        box-shadow: var(--erp-shadow-md);
        transform: translateY(-1px);
    }

    .stat-icon {
        width: 40px;
        height: 40px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 18px;
        margin-bottom: 14px;

        &.blue { background: #dbeafe; color: #2563eb; }
        &.green { background: #d1fae5; color: #059669; }
        &.amber { background: #fef3c7; color: #d97706; }
        &.rose { background: #ffe4e6; color: #e11d48; }
    }

    .stat-value {
        font-family: 'Space Grotesk', sans-serif;
        font-size: 26px;
        font-weight: 700;
        line-height: 1;
        margin-bottom: 4px;
    }

    .stat-label {
        font-size: 13px;
        color: var(--erp-text-muted);
    }

    .stat-trend {
        display: inline-flex;
        align-items: center;
        gap: 3px;
        font-size: 12px;
        font-weight: 600;
        margin-top: 8px;
        padding: 2px 8px;
        border-radius: 20px;

        &.up { background: #d1fae5; color: #059669; }
        &.down { background: #ffe4e6; color: #e11d48; }
    }
}
```

- [ ] **Step 3: 创建 _status-badge.scss**

```scss
.status-badge {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    padding: 3px 10px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;

    .status-dot {
        width: 6px;
        height: 6px;
        border-radius: 50%;
    }

    &.success { background: #d1fae5; color: #065f46; .status-dot { background: #10b981; } }
    &.warning { background: #fef3c7; color: #92400e; .status-dot { background: #f59e0b; } }
    &.danger { background: #ffe4e6; color: #9f1239; .status-dot { background: #ef4444; } }
    &.info { background: #dbeafe; color: #1e40af; .status-dot { background: #3b82f6; } }
    &.neutral { background: #f1f5f9; color: #475569; .status-dot { background: #94a3b8; } }
}
```

- [ ] **Step 4: 创建 _action-bar.scss**

```scss
.erp-action-bar {
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    box-shadow: var(--erp-shadow);
    padding: 12px 20px;
    margin-bottom: 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;

    .action-bar-left {
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .action-bar-right {
        display: flex;
        align-items: center;
        gap: 8px;
    }
}
```

- [ ] **Step 5: 创建 _filter-panel.scss**

```scss
.filter-panel {
    position: fixed;
    top: 0;
    right: -380px;
    width: 380px;
    height: 100vh;
    background: var(--erp-card-bg);
    border-left: none;
    box-shadow: var(--erp-shadow-lg);
    z-index: 1050;
    transition: right var(--erp-transition);
    display: flex;
    flex-direction: column;

    &.open { right: 0; }
}

.filter-panel-header {
    padding: 16px 20px;
    border-bottom: none;
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: var(--erp-content-bg);

    h3 {
        font-size: 15px;
        font-weight: 600;
        margin: 0;
    }
}

.filter-panel-body {
    flex: 1;
    padding: 20px;
    overflow-y: auto;
}

.filter-group {
    margin-bottom: 20px;

    label {
        font-size: 12px;
        font-weight: 600;
        color: var(--erp-text-muted);
        margin-bottom: 6px;
        display: block;
    }

    select, input {
        width: 100%;
        padding: 8px 12px;
        border: 1px solid var(--erp-border);
        border-radius: var(--erp-radius-sm);
        font-size: 13px;
        background: var(--erp-content-bg);
        color: var(--erp-text);
    }
}

.filter-panel-footer {
    padding: 16px 20px;
    border-top: none;
    display: flex;
    gap: 8px;
    background: var(--erp-content-bg);
}

.filter-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, .3);
    z-index: 1045;
    opacity: 0;
    pointer-events: none;
    transition: opacity var(--erp-transition);

    &.open {
        opacity: 1;
        pointer-events: auto;
    }
}
```

- [ ] **Step 6: 创建 _detail-panel.scss**

```scss
.detail-header {
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    box-shadow: var(--erp-shadow);
    padding: 24px;
    margin-bottom: 20px;
}

.detail-title-row {
    display: flex;
    align-items: center;
    gap: 12px;

    h2 {
        font-size: 20px;
        font-weight: 700;
        margin: 0;
    }
}

.detail-meta {
    display: flex;
    align-items: center;
    gap: 16px;
    margin-top: 10px;
    font-size: 12px;
    color: var(--erp-text-muted);
    flex-wrap: wrap;

    span {
        display: flex;
        align-items: center;
        gap: 4px;
    }
}

.detail-body {
    display: grid;
    grid-template-columns: 1fr 320px;
    gap: 20px;
}

.detail-main {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.detail-sidebar-panel {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.detail-card {
    background: var(--erp-card-bg);
    border: none;
    border-radius: var(--erp-radius);
    box-shadow: var(--erp-shadow);
    overflow: hidden;
}

.detail-card-header {
    padding: 14px 20px;
    border-bottom: none;
    font-size: 13px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
    background: var(--erp-content-bg);
}

.detail-card-body {
    padding: 20px;
}

.detail-field {
    margin-bottom: 16px;

    &:last-child { margin-bottom: 0; }

    .field-label {
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: .5px;
        color: var(--erp-text-muted);
        margin-bottom: 4px;
    }

    .field-value {
        font-size: 14px;
        color: var(--erp-text);
    }
}

@include media-breakpoint-down(lg) {
    .detail-body {
        grid-template-columns: 1fr;
    }
}
```

- [ ] **Step 7: 创建 _empty-state.scss**

```scss
.erp-empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 24px;
    text-align: center;

    .empty-icon {
        font-size: 48px;
        color: var(--erp-text-muted);
        margin-bottom: 16px;
        opacity: .5;
    }

    .empty-title {
        font-size: 16px;
        font-weight: 600;
        color: var(--erp-text);
        margin-bottom: 8px;
    }

    .empty-desc {
        font-size: 13px;
        color: var(--erp-text-muted);
        max-width: 320px;
    }

    .empty-action {
        margin-top: 20px;
    }
}
```

- [ ] **Step 8: 创建 themes/light/_index.scss**

```scss
/* The light theme is also the default theme. */
/* Do not wrap these classes for the light theme specific. */

:root {
    --erp-primary-light: #e8edfb;
    --erp-content-bg: #f8fafc;
    --erp-card-bg: #ffffff;
    --erp-border: #e2e8f0;
    --erp-text: #1e293b;
    --erp-text-muted: #64748b;
    --erp-shadow: 0 1px 3px rgba(0, 0, 0, .06), 0 1px 2px rgba(0, 0, 0, .04);
    --erp-shadow-md: 0 4px 6px rgba(0, 0, 0, .05), 0 2px 4px rgba(0, 0, 0, .04);
    --erp-shadow-lg: 0 10px 25px rgba(0, 0, 0, .08);
    --erp-row-alt: #f8fafc;
}
```

- [ ] **Step 9: 创建 themes/dark/_index.scss**

```scss
@import '../../../../../../../node_modules/bootstrap/scss/mixins/_color-mode';

@include color-mode(dark) {
    --erp-primary-light: #1e293b;
    --erp-content-bg: #0f172a;
    --erp-card-bg: #1e293b;
    --erp-border: #334155;
    --erp-text: #f1f5f9;
    --erp-text-muted: #94a3b8;
    --erp-shadow: 0 1px 3px rgba(0, 0, 0, .2);
    --erp-shadow-md: 0 4px 6px rgba(0, 0, 0, .3);
    --erp-shadow-lg: 0 10px 25px rgba(0, 0, 0, .4);
    --erp-row-alt: #162032;
}
```

- [ ] **Step 10: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Assets/scss/components/ src/OrchardCore.Themes/ErpAdmin/Assets/scss/themes/
git commit -m "feat(erpadmin): add SCSS components and light/dark theme files"
```

---

### Task 11: Placement 和 Pager

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/placement.json`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_CurrentPage.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_First.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_Gap.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_Last.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_Link.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_Next.cshtml`
- Create: `src/OrchardCore.Themes/ErpAdmin/Views/Pager_Previous.cshtml`

- [ ] **Step 1: 创建 placement.json**

```json
{
  "Content_DetailAdmin": [
    {
      "displayType": "DetailAdmin",
      "differentiator": "Parts_Contents_Publish",
      "place": "Actions:5"
    }
  ],
  "Parts_Contents_Publish": [
    {
      "displayType": "Detail",
      "place": "Content:5"
    }
  ],
  "Parts_Contents_Publish_SummaryAdmin": [
    {
      "displayType": "SummaryAdmin",
      "place": "Actions:5"
    }
  ],
  "Parts_Contents_Clone_SummaryAdmin": [
    {
      "displayType": "SummaryAdmin",
      "place": "Actions:6"
    }
  ]
}
```

- [ ] **Step 2: 创建 Pager.cshtml**

```cshtml
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Links";
}

<nav aria-label="@T["Listing pages"]" class="erp-pagination">
    @await DisplayAsync(Model)
</nav>
```

- [ ] **Step 3: 创建 Pager_CurrentPage.cshtml**

```cshtml
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.AddCssClass("active");
        parentTag.MergeAttribute("aria-current", "page");
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 4: 创建 Pager_Link.cshtml**

```cshtml
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "ActionLink";
    Model.Classes.Add("page-link");
    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.AddCssClass("page-btn");
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 5: 创建 Pager_Previous.cshtml**

```cshtml
@using Microsoft.AspNetCore.Html

@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var value = new HtmlContentBuilder();
    value.AppendHtml("<i class=\"bi bi-chevron-left\"></i>");
    Model.Value = value;

    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.MergeAttribute("title", T["Go to previous page."].Value);
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 6: 创建 Pager_Next.cshtml**

```cshtml
@using Microsoft.AspNetCore.Html

@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var value = new HtmlContentBuilder();
    value.AppendHtml("<i class=\"bi bi-chevron-right\"></i>");
    Model.Value = value;

    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.MergeAttribute("title", T["Go to next page."].Value);
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 7: 创建 Pager_Gap.cshtml**

```cshtml
@using Microsoft.AspNetCore.Html

@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var value = new HtmlContentBuilder();
    value.AppendHtml("<i class=\"bi bi-three-dots\"></i>");
    Model.Value = value;
}

@await DisplayAsync(Model)
```

- [ ] **Step 8: 创建 Pager_First.cshtml**

```cshtml
@using Microsoft.AspNetCore.Html

@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var value = new HtmlContentBuilder();
    value.AppendHtml("<i class=\"bi bi-chevron-double-left\"></i>");
    Model.Value = value;

    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.MergeAttribute("title", T["Go to first page."].Value);
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 9: 创建 Pager_Last.cshtml**

```cshtml
@using Microsoft.AspNetCore.Html

@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    var value = new HtmlContentBuilder();
    value.AppendHtml("<i class=\"bi bi-chevron-double-right\"></i>");
    Model.Value = value;

    var parentTag = Model.Tag as TagBuilder;

    if (parentTag != null)
    {
        parentTag.MergeAttribute("title", T["Go to last page."].Value);
    }
}

@await DisplayAsync(Model)
```

- [ ] **Step 10: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/placement.json src/OrchardCore.Themes/ErpAdmin/Views/Pager*.cshtml
git commit -m "feat(erpadmin): add placement.json and pager shape overrides with ERP styling"
```

---

### Task 12: Recipe

**Files:**
- Create: `src/OrchardCore.Themes/ErpAdmin/Recipes/erpadmin.recipe.json`

- [ ] **Step 1: 创建 erpadmin.recipe.json**

```json
{
  "name": "ErpAdmin",
  "displayName": "ERP Admin",
  "description": "Creates an ERP Admin site with content management features and the ErpAdmin theme.",
  "author": "The Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "issetuprecipe": true,
  "categories": [ "default" ],
  "tags": [ "erp", "admin" ],

  "variables": {},

  "steps": [
    {
      "name": "feature",
      "enable": [
        "OrchardCore.HomeRoute",
        "OrchardCore.Admin",
        "OrchardCore.Diagnostics",
        "OrchardCore.DynamicCache",
        "OrchardCore.Features",
        "OrchardCore.Navigation",
        "OrchardCore.Recipes",
        "OrchardCore.Resources",
        "OrchardCore.Roles",
        "OrchardCore.Security",
        "OrchardCore.Settings",
        "OrchardCore.Themes",
        "OrchardCore.Users",

        "OrchardCore.Alias",
        "OrchardCore.Autoroute",
        "OrchardCore.Html",
        "OrchardCore.ContentFields",
        "OrchardCore.ContentPreview",
        "OrchardCore.Contents",
        "OrchardCore.ContentTypes",
        "OrchardCore.CustomSettings",
        "OrchardCore.Deployment",
        "OrchardCore.Deployment.Remote",
        "OrchardCore.Flows",
        "OrchardCore.Indexing",
        "OrchardCore.Layers",
        "OrchardCore.Lists",
        "OrchardCore.Markdown",
        "OrchardCore.Media",
        "OrchardCore.Menu",
        "OrchardCore.Queries",
        "OrchardCore.Shortcodes.Templates",
        "OrchardCore.Title",
        "OrchardCore.Templates",
        "OrchardCore.Widgets",

        "TheAdmin"
      ]
    },
    {
      "name": "themes",
      "admin": "ErpAdmin",
      "site": ""
    },
    {
      "name": "Roles",
      "Roles": [
        {
          "Name": "Moderator",
          "Description": "Grants users the ability to moderate content.",
          "Permissions": []
        },
        {
          "Name": "Editor",
          "Description": "Grants users the ability to edit existing content.",
          "Permissions": []
        },
        {
          "Name": "Author",
          "Description": "Grants users the ability to create content.",
          "Permissions": []
        },
        {
          "Name": "Contributor",
          "Description": "Grants users the ability to contribute content.",
          "Permissions": []
        }
      ]
    }
  ]
}
```

- [ ] **Step 2: 提交**

```bash
git add src/OrchardCore.Themes/ErpAdmin/Recipes/erpadmin.recipe.json
git commit -m "feat(erpadmin): add setup recipe for ERP Admin theme"
```

---

## 自检清单

### 1. Spec 覆盖度

| Spec 要求 | 对应 Task |
|-----------|----------|
| 主题继承 TheAdmin | Task 1 (Manifest.cs BaseTheme) |
| Layout 覆盖（侧边栏+顶栏） | Task 3 |
| 登录页布局 | Task 3 (Layout-Login.cshtml) + Task 7 |
| 双因素认证布局 | Task 3 (Layout-TwoFactor.cshtml) |
| 侧边栏导航（多级） | Task 4 |
| AdminBranding | Task 4 |
| Navbar + 用户菜单 | Task 5 |
| Content.DetailAdmin | Task 6 |
| ContentPart.DetailAdmin | Task 6 |
| Content.SummaryAdmin | Task 6 |
| ContentsAdminList | Task 6 |
| Stereotype.DetailAdmin | Task 6 |
| Account/Login | Task 7 |
| LoginFormCredentials | Task 7 |
| LoginFormForgotPassword | Task 7 |
| LoginFormRegisterUser | Task 7 |
| MetadataContentsHandler | Task 8 |
| TenantBranding 模型 | Task 9 |
| TenantBrandingNavbarDisplayDriver | Task 9 |
| SCSS 变量 + CSS 自定义属性 | Task 2 + Task 10 |
| ERP 组件（7个） | Task 10 |
| 亮/暗主题 | Task 10 |
| placement.json | Task 11 |
| Pager 覆盖 | Task 11 |
| Recipe | Task 12 |
| 色块分区设计 | Task 10 (所有组件无 border，用背景色+阴影) |
| 移动端响应式 | Task 3 (erp-admin-layout.scss 响应式断点) |
| 租户品牌 CSS 变量覆盖 | Task 9 (TenantBrandingStyles.cshtml) |

### 2. 占位符扫描

无 TBD、TODO、implement later、fill in details 等占位符。所有步骤包含完整代码。

### 3. 类型一致性

- `MetadataContentsHandler` 在 Task 8 中定义，在 Task 1 的 `Startup.cs` 中注册，命名空间一致：`OrchardCore.Themes.ErpAdmin.Handlers`
- `TenantBrandingNavbarDisplayDriver` 在 Task 9 中定义，在 Task 1 的 `Startup.cs` 中注册，命名空间一致：`OrchardCore.Themes.ErpAdmin.Drivers`
- `ResourceManagementOptionsConfiguration` 在 Task 2 中定义，在 Task 1 的 `Startup.cs` 中注册，命名空间一致：`OrchardCore.Themes.ErpAdmin`
- CSS 类名在 SCSS 和 cshtml 文件中保持一致（如 `erp-sidebar`, `erp-topbar`, `nav-link-erp`, `status-badge` 等）
- 资源名称在 `Assets.json` 和 `ResourceManagementOptionsConfiguration.cs` 中保持一致
