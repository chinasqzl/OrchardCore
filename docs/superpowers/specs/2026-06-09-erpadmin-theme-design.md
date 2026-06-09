# ErpAdmin 主题设计文档

## 概述

ErpAdmin 是一个基于 OrchardCore CMS 的管理后台主题，继承自 TheAdmin，专为 SaaS 化 ERP 场景设计。主题仅负责布局框架和设计系统，不包含任何业务逻辑——ERP 功能由独立模块自行实现渲染。

### 核心原则

- **纯主题**：只提供布局、样式、形状模板，业务由模块负责
- **模块化渲染**：模块自带 AdminMenu、Controller、Driver、Shape、ViewModel
- **移动优先**：响应式设计，侧边栏移动端折叠为汉堡菜单
- **色块分区**：用背景色差异和阴影分层，避免线条分隔
- **租户感知**：支持租户级品牌定制（Logo、主色、布局偏好）

## 架构

### 主题继承

```
TheAdmin (BaseTheme)
  └── ErpAdmin (继承)
        ├── 覆盖: Layout.cshtml, Navigation-*, Layout-Login.cshtml
        ├── 新增: DetailAdmin 形状模板
        ├── 新增: ERP 设计系统 SCSS
        ├── 新增: 租户品牌定制驱动
        ├── 新增: MetadataContentsHandler (路由覆盖)
        └── 新增: placement.json (管理场景区域放置规则)
```

### 视图解析顺序

OrchardCore 的 `ThemeViewLocationExpander` 确保视图按以下顺序查找：

1. **ErpAdmin**（当前主题）→ 2. **TheAdmin**（基主题）→ 3. **各模块**（原始定义）

ErpAdmin 通过提供同名视图文件覆盖模块的形状模板。

## 布局结构

### 管理后台布局

```
┌─────────────────────────────────────────────┐
│  顶部导航栏 (Navbar)                         │
│  [品牌Logo] [搜索] [通知] [用户菜单] [租户]   │
├──────────┬──────────────────────────────────┤
│ 侧边栏    │  内容区                           │
│ ──────── │  ┌─────────────────────────────┐ │
│ 📊 仪表板 │  │ 面包屑 / 标题 / 操作按钮      │ │
│ 📦 产品   │  ├─────────────────────────────┤ │
│ 📋 订单   │  │                             │ │
│ 🏢 客户   │  │  模块渲染区域                │ │
│ 📊 报表   │  │  (Shape + Placement 控制)   │ │
│ ⚙️ 设置   │  │                             │ │
│           │  └─────────────────────────────┘ │
│ [折叠按钮]│                                  │
└──────────┴──────────────────────────────────┘
```

- 侧边栏宽度：260px（折叠 72px）
- 顶栏高度：56px
- 移动端：侧边栏滑出式，内容区全宽

### 登录页布局

```
┌──────────────────────┬────────────────┐
│                      │                │
│   品牌展示区          │   登录表单区    │
│   (深色背景)          │   (白色背景)    │
│                      │                │
│   - Logo + 名称      │   - 租户标识    │
│   - 平台介绍         │   - 用户名/密码  │
│   - 特性列表         │   - 记住我      │
│                      │   - 外部登录    │
│                      │                │
└──────────────────────┴────────────────┘
```

移动端：隐藏左侧品牌区，表单区全屏。

## 设计系统

### 技术栈

- **CSS 框架**：Bootstrap 5（扩展，不替换）
- **预处理**：SCSS，通过 CSS 自定义属性实现主题色动态切换
- **图标**：Bootstrap Icons（Font Awesome 7 兼容）
- **字体**：Space Grotesk（标题）+ DM Sans（正文）
- **打包**：Parcel（沿用 TheAdmin 的 Assets.json 配置）

### 色块分区设计

视觉分层通过背景色差异和阴影实现，避免线条分隔：

| 层级 | 亮色模式 | 暗色模式 | 用途 |
|------|---------|---------|------|
| 页面背景 | `#f8fafc` | `#0f172a` | 内容区底色 |
| 卡片背景 | `#ffffff` | `#1e293b` | 卡片、面板 |
| 区块背景 | `#f1f5f9` | `#162032` | 表头、工具栏、交替行 |
| 侧边栏 | `#0f172a` | `#0f172a` | 导航区 |

卡片无 border，仅用 `box-shadow` 浮起。表格用交替行背景色区分行。

### ERP 专用组件

| 组件 | SCSS 文件 | 用途 |
|------|----------|------|
| 数据表格 | `_data-table.scss` | 带排序/筛选/分页/批量操作的管理表格 |
| 统计卡片 | `_stat-card.scss` | 仪表板关键指标展示 |
| 状态标签 | `_status-badge.scss` | 订单/库存等业务状态标识 |
| 操作按钮组 | `_action-bar.scss` | 列表页/详情页的上下文操作 |
| 筛选面板 | `_filter-panel.scss` | 侧滑式高级筛选 |
| 详情面板 | `_detail-panel.scss` | 管理详情页的主从布局 |
| 空状态 | `_empty-state.scss` | 无数据时的引导提示 |

### CSS 自定义属性（租户品牌定制）

```scss
:root {
    --erp-primary: #1a56db;       // 主色（可被租户覆盖）
    --erp-primary-light: #e8edfb;
    --erp-primary-dark: #1240a8;
    --erp-accent: #0ea5e9;
    --erp-sidebar-bg: #0f172a;
    // ... 更多变量
}
```

主题 Driver 读取当前租户品牌设置，将覆盖值注入 Layout 的 `<style>` 块。

## 形状覆盖映射

### 布局与页面结构

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| Layout | `Views/Layout.cshtml` | TheAdmin | 覆盖 |
| Layout__Login | `Views/Layout-Login.cshtml` | TheAdmin | 新增 |
| Layout-TwoFactor | `Views/Layout-TwoFactor.cshtml` | TheAdmin | 覆盖 |

### 导航与菜单

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| Navigation-admin | `Views/Navigation-admin.cshtml` | TheAdmin | 覆盖 |
| NavigationItem-admin | `Views/NavigationItem-admin.cshtml` | TheAdmin | 覆盖 |
| NavigationItemLink-admin | `Views/NavigationItemLink-admin.cshtml` | TheAdmin | 覆盖 |
| NavigationItemText | `Views/NavigationItemText.cshtml` | TheAdmin | 覆盖 |
| AdminBranding | `Views/AdminBranding.cshtml` | OrchardCore.Admin | 新增 |

### 导航栏与用户菜单

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| Navbar.DetailAdmin | `Views/Navbar.DetailAdmin.cshtml` | OrchardCore.Admin | 新增 |
| NavbarUserMenu | `Views/NavbarUserMenu.cshtml` | OrchardCore.Users | 新增 |
| UserMenu | `Views/UserMenu.cshtml` | OrchardCore.Users | 新增 |
| UserMenuItems-SignedUser | `Views/UserMenuItems-SignedUser.cshtml` | OrchardCore.Users | 新增 |
| UserMenuItems-SignOut | `Views/UserMenuItems-SignOut.cshtml` | OrchardCore.Users | 新增 |
| ToggleTheme | `Views/ToggleTheme.cshtml` | TheAdmin | 继承 |

### 内容显示 (DetailAdmin / SummaryAdmin)

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| Content.DetailAdmin | `Views/Content.DetailAdmin.cshtml` | OrchardCore.Contents | 新增 |
| ContentPart.DetailAdmin | `Views/ContentPart.DetailAdmin.cshtml` | OrchardCore.Contents | 新增 |
| Content.SummaryAdmin | `Views/Content.SummaryAdmin.cshtml` | OrchardCore.Contents | 新增 |
| ContentsAdminList | `Views/ContentsAdminList.cshtml` | OrchardCore.Contents | 新增 |
| Stereotype.DetailAdmin | `Views/Stereotype.DetailAdmin.cshtml` | OrchardCore.Contents | 新增（为不同 Stereotype 如 Widget/MenuItem 提供管理详情入口） |

### 登录与认证

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| Account/Login | `Views/Account/Login.cshtml` | OrchardCore.Users | 新增 |
| LoginFormCredentials | `Views/LoginFormCredentials.cshtml` | OrchardCore.Users | 新增 |
| LoginFormForgotPassword | `Views/LoginFormForgotPassword.cshtml` | OrchardCore.Users | 新增 |
| LoginFormRegisterUser | `Views/LoginFormRegisterUser.cshtml` | OrchardCore.Users | 新增 |

### 通知与分页

| 形状名称 | ErpAdmin 视图文件 | 来源 | 类型 |
|---------|------------------|------|------|
| NotifyMessages | `Views/NotifyMessages.cshtml` | TheAdmin | 继承 |
| Message | `Views/Message.cshtml` | TheAdmin | 继承 |
| Pager | `Views/Pager.cshtml` | TheAdmin | 覆盖 |
| Pager_* | `Views/Pager_*.cshtml` | TheAdmin | 覆盖 |

## DetailAdmin 机制

### 路由覆盖

`MetadataContentsHandler` 覆盖 `ContentItemMetadata` 的路由值，将所有内容项的显示路由指向 Admin/Display：

```csharp
public class MetadataContentsHandler : ContentHandlerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MetadataContentsHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<ContentItemMetadata>(async contentItemMetadata =>
        {
            contentItemMetadata.DisplayRouteValues = new RouteValueDictionary
            {
                {"Area", "OrchardCore.Contents"},
                {"Controller", "Admin"},
                {"Action", "Display"},
                {"ContentItemId", context.ContentItem.ContentItemId}
            };
            contentItemMetadata.AdminRouteValues = new RouteValueDictionary
            {
                {"Area", "OrchardCore.Contents"},
                {"Controller", "Admin"},
                {"Action", "Display"},
                {"ContentItemId", context.ContentItem.ContentItemId}
            };
        });
    }
}
```

Startup 注册：`services.AddScoped<IContentHandler, MetadataContentsHandler>();`

### 形状模板渲染流程

1. 主题激活后，`MetadataContentsHandler` 将 Detail 路由指向 Admin/Display
2. Admin/Display 使用 `DetailAdmin` 显示类型渲染内容
3. 主题提供通用 `Content.DetailAdmin.cshtml`，将内容组织为管理视角：
   - 头部：标题 + 状态标签 + 元数据（编号/分类/创建者/时间）
   - 主内容区：各 ContentPart 的 DetailAdmin 形状
   - 侧边栏：快捷操作 + 元数据 + 活动时间线
4. 主题的 `placement.json` 定义管理场景下的区域放置规则
5. 模块可提供自己的 `Content-MyType.DetailAdmin.cshtml` 覆盖通用模板

## 租户品牌定制

### 模型

```csharp
public class TenantBranding
{
    public string LogoUrl { get; set; }
    public string PrimaryColor { get; set; }
    public string SidebarStyle { get; set; } // "dark" | "light"
    public string BrandName { get; set; }
}
```

### 驱动

`TenantBrandingNavbarDisplayDriver` 读取当前租户品牌设置，将 CSS 变量覆盖注入 Layout。

### 实现方式

在 `Layout.cshtml` 的 `<head>` 中动态注入：

```html
<style>
:root {
    --erp-primary: @Model.TenantBranding.PrimaryColor;
    --erp-primary-light: @Lighten(Model.TenantBranding.PrimaryColor);
    --erp-primary-dark: @Darken(Model.TenantBranding.PrimaryColor);
}
</style>
```

## 文件结构

```
src/OrchardCore.Themes/ErpAdmin/
├── Manifest.cs                              # 主题声明，BaseTheme = TheAdmin
├── Startup.cs                               # 服务注册
├── Assets.json                              # 资源构建配置（Parcel）
├── ResourceManagementOptionsConfiguration.cs # 资源注册
├── placement.json                           # 管理场景区域放置规则
├── Assets/
│   ├── package.json
│   ├── scss/
│   │   ├── ErpAdmin.scss                   # 主入口
│   │   ├── erp-admin-layout.scss           # 布局样式
│   │   ├── _variables.scss                 # ERP 专用变量
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
│   │       └── dark/
│   └── js/
│       └── ErpAdmin/
│           └── ErpAdmin.ts                  # 主题交互逻辑
├── Handlers/
│   └── MetadataContentsHandler.cs          # 覆盖内容项路由
├── Drivers/
│   └── TenantBrandingNavbarDisplayDriver.cs
├── Models/
│   └── TenantBranding.cs
├── Views/
│   ├── _ViewImports.cshtml
│   ├── Layout.cshtml                        # 覆盖主布局
│   ├── Layout-Login.cshtml                  # 登录页布局
│   ├── Layout-TwoFactor.cshtml              # 双因素认证布局
│   ├── Navigation-admin.cshtml              # 侧边栏导航
│   ├── NavigationItem-admin.cshtml
│   ├── NavigationItemLink-admin.cshtml
│   ├── NavigationItemText.cshtml
│   ├── AdminBranding.cshtml                 # 管理品牌区
│   ├── Navbar.DetailAdmin.cshtml            # 管理导航栏
│   ├── NavbarUserMenu.cshtml                # 用户菜单下拉
│   ├── UserMenu.cshtml
│   ├── UserMenuItems-SignedUser.cshtml
│   ├── UserMenuItems-SignOut.cshtml
│   ├── Content.DetailAdmin.cshtml           # 通用管理详情模板
│   ├── ContentPart.DetailAdmin.cshtml
│   ├── Content.SummaryAdmin.cshtml          # 列表摘要模板
│   ├── ContentsAdminList.cshtml             # 管理列表模板
│   ├── Stereotype.DetailAdmin.cshtml
│   ├── Account/
│   │   └── Login.cshtml                     # 登录页
│   ├── LoginFormCredentials.cshtml
│   ├── LoginFormForgotPassword.cshtml
│   ├── LoginFormRegisterUser.cshtml
│   ├── Pager.cshtml                         # 分页
│   └── NotifyMessages.cshtml                # 通知（可选覆盖）
└── Recipes/
    └── erpadmin.recipe.json                 # 主题初始化配方
```

## 关键设计决策

| 决策 | 选择 | 理由 |
|------|------|------|
| 主题关系 | 继承 TheAdmin | 复用基础设施，最小化代码量，可独立分发 |
| 视觉风格 | 色块分区 + 阴影 | 现代、柔和，减少视觉噪音 |
| DetailAdmin 实现 | 形状模板覆盖 | 符合 OC 设计哲学，模块可自行覆盖 |
| 路由重定向 | MetadataContentsHandler | 主题级全局生效，无需模块配合 |
| 品牌定制 | CSS 自定义属性 | 运行时动态切换，无需重新编译 |
| 移动端 | 侧边栏滑出式 | 符合移动优先原则 |
