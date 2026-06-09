# OrchardCore Comments Module - Design Spec

## Overview

为 OrchardCore CMS 新增评论模块（`OrchardCore.Comments`），以 ContentPart 机制赋予任意内容类型可评论功能。评论显示在内容项的 DetailAdmin 和 Detail 视图中。当前阶段面向 Admin 后台，未来可扩展前台主题。

## Decisions

| Decision | Choice |
|----------|--------|
| 数据模型 | 混合模式：评论作为独立 ContentItem + 被评论内容附加 CommentablePart 元数据 |
| 回复模式 | 单层回复（RepliedOn 指向父评论，不支持无限嵌套） |
| 审核机制 | 可选审核（CommentablePartSettings.RequireApproval 配置） |
| 提交方式 | AJAX 无刷新提交 |
| 身份要求 | 仅登录用户可评论 |
| 架构方案 | 自定义 CommentPart + CommentPartIndex，不依赖 Lists 模块 |
| 使用场景 | 当前 Admin 后台，保留前台扩展性 |

## Naming Convention

为避免命名混淆，模块中有两个不同的 Part：

- **CommentPart**：附加在**评论 ContentItem** 上，存储评论逻辑数据（CommentedOn、RepliedOn、Status 等）
- **CommentablePart**：附加在**被评论的内容项**上，存储元数据（CommentCount、Closed），使该内容类型"可评论"

## Data Model

### Comment ContentType（Migration 中定义）

评论是独立的 ContentItem，类型名为 `Comment`。

```
Comment ContentType:
├── CommentPart              (评论 ContentItem 上的 Part - 评论逻辑数据)
│   ├── CommentedOn          string      被评论内容的 ContentItemId
│   ├── RepliedOn            string      被回复评论的 ContentItemId（null = 顶级评论）
│   ├── IsAdminReply         bool        管理员回复标识
│   ├── IsDeleted            bool        软删除标记
│   ├── Status               CommentStatus   Pending / Approved / Rejected
│   └── CreatedUtc           DateTime    评论时间
├── CommentText              (TextField, Editor=TextArea)
│   └── Text                 string      评论正文
└── Attachment               (MediaField, 可选)
    ├── Paths                string[]    媒体路径
    └── MediaTexts           string[]    媒体描述
```

### CommentablePart（附加到被评论内容上）

这是一个**可附加的 ContentPart**，管理员可为任意 ContentType 添加，使该内容类型"可评论"：

```
CommentablePart:
├── CommentCount             int         已审核通过的评论总数（由 Handler 自动维护）
└── Closed                   bool        是否关闭评论
```

### CommentablePartSettings（类型级设置）

```
CommentablePartSettings:
├── RequireApproval          bool        是否需要审核（默认 false）
├── AllowComments            bool        是否允许评论（默认 true）
└── DefaultStatus            CommentStatus  新评论默认状态（默认 Approved）
```

### CommentStatus 枚举

```csharp
public enum CommentStatus
{
    Pending,
    Approved,
    Rejected
}
```

### CommentPartIndex（YesSql 索引）

索引**所有状态的评论**，通过 Status 字段筛选。

```
CommentPartIndex : MapIndex
├── ContentItemId             string      评论的 ContentItemId
├── CommentedOn               string      被评论内容的 ContentItemId
├── RepliedOn                 string      被回复评论的 ContentItemId
├── Status                    string      Pending / Approved / Rejected
├── IsAdminReply              bool
├── IsDeleted                 bool
├── CreatedUtc                DateTime
└── Published                 bool
```

## Module Structure

```
OrchardCore.Comments/
├── Manifest.cs
├── Startup.cs
├── Migrations.cs
├── placement.json
│
├── Models/
│   ├── CommentPart.cs                    # 评论 ContentItem 上的 Part
│   ├── CommentablePart.cs                # 被评论内容上的 Part
│   ├── CommentablePartSettings.cs        # CommentablePart 类型级设置
│   └── CommentStatus.cs                  # 枚举
│
├── ViewModels/
│   ├── CommentablePartViewModel.cs       # DetailAdmin 展示 VM
│   ├── CommentablePartEditViewModel.cs   # 编辑设置 VM
│   ├── CommentablePartSettingsViewModel.cs  # 类型设置 VM
│   └── CommentCreateViewModel.cs         # 创建评论 VM
│
├── Drivers/
│   ├── CommentablePartDisplayDriver.cs   # 被评论内容的评论区 Driver
│   └── CommentablePartSettingsDisplayDriver.cs  # 类型设置 Driver
│
├── Handlers/
│   └── CommentPartHandler.cs             # 评论数同步 Handler
│
├── Indexes/
│   └── CommentPartIndex.cs               # Index + IndexProvider
│
├── Controllers/
│   ├── AdminController.cs                # 独立评论管理列表页
│   └── CommentApiController.cs           # AJAX API
│
├── Permissions/
│   └── CommentPermissions.cs
│
├── Services/
│   ├── ICommentService.cs
│   └── CommentService.cs
│
├── Views/
│   ├── CommentablePart.DetailAdmin.cshtml       # DetailAdmin 评论区主视图
│   ├── CommentablePart.SummaryAdmin.cshtml      # SummaryAdmin 评论数显示
│   ├── CommentablePart.Edit.cshtml              # CommentablePart 设置编辑
│   ├── CommentablePart.cshtml                   # Detail 展示（前台预留）
│   ├── CommentablePartSettings.Edit.cshtml      # 类型级设置编辑
│   ├── TextField-CommentText.DetailAdmin.cshtml  # 覆盖评论正文渲染
│   ├── MediaField-Attachment.DetailAdmin.cshtml  # 覆盖附件渲染
│   ├── Admin/
│   │   └── List.cshtml                          # 独立评论管理列表页
│   └── _ViewImports.cshtml
│
└── Assets/
    └── Scripts/
        └── comments.js                          # AJAX 交互脚本
```

## Alternate View Strategy

数据层复用 TextField 和 MediaField，展示层通过 Alternate 视图重写为一体化风格。

### 候补视图映射

| 视图文件 | ShapeType | 职责 |
|---------|-----------|------|
| `TextField-CommentText.DetailAdmin.cshtml` | `textfield_detailadmin__commenttext` | 去掉 label 包装，直接渲染评论正文 |
| `MediaField-Attachment.DetailAdmin.cshtml` | `mediafield_detailadmin__attachment` | 改为紧凑文件列表样式（非默认缩略图） |
| `CommentablePart.DetailAdmin.cshtml` | `commentablepart_detailadmin` | 组合渲染整个评论区 |

### 候补解析原理

文件名 `TextField-CommentText.DetailAdmin.cshtml` 的解析过程：

1. `BasicShapeTemplateHarvester` 解析：
   - 文件名 `TextField-CommentText`，lastDot 在 `CommentText` 前
   - displayType = `DetailAdmin`
   - Adjust: `textfield__commenttext`（`-` → `__`）
   - 插入 displayType: `textfield_detailadmin__commenttext`
2. `ContentPartAlternatesFactory` 生成的 alternate 包含此模式
3. 此 alternate 优先级高于默认的 `TextField.DetailAdmin.cshtml`

## API Design

### CommentApiController

路由前缀：`api/comments`（area = OrchardCore.Comments）

所有 POST 端点需要 `RequestVerificationToken` header + 登录授权。

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/api/comments/create` | 创建评论 | CreateComments |
| POST | `/api/comments/{id}/status` | 更新评论状态（审核） | ManageComments |
| POST | `/api/comments/{id}/delete` | 软删除评论 | ManageComments or DeleteOwnComment |
| POST | `/api/comments/{id}/update` | 编辑评论（仅评论者） | EditOwnComment |
| GET | `/api/comments/list` | 获取评论列表 | CreateComments |

#### Create Request Body

```json
{
  "commentedOn": "content-item-id",
  "repliedOn": "parent-comment-id-or-null",
  "text": "评论内容",
  "attachments": ["path1", "path2"]
}
```

#### Create Response

```json
{
  "success": true,
  "commentHtml": "<div>...</div>",
  "commentId": "new-comment-content-item-id"
}
```

### AdminController

路由：`Admin/Comments/{action}`（area = OrchardCore.Comments）

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Admin/Comments/List` | 评论列表页（支持按状态/内容类型/日期筛选） |
| POST | `/Admin/Comments/BatchAction` | 批量操作（Approve/Reject/Delete） |

## Permissions

```
CommentPermissions:
├── ManageComments        管理所有评论（审核/删除任意评论）
├── CreateComments        创建评论（登录用户默认拥有）
├── EditOwnComment        编辑自己的评论
├── DeleteOwnComment      删除自己的评论
└── AdminReply            管理员回复标识
```

角色分配建议：
- **Administrator**: 所有权限
- **Authenticated**: CreateComments + EditOwnComment + DeleteOwnComment

## Core Interaction Flows

### DetailAdmin 评论区

1. 用户访问内容项 DetailAdmin 页面
2. `CommentablePartDisplayDriver.Display()` 查询该内容的评论列表（通过 CommentPartIndex）
3. `CommentablePart.DetailAdmin.cshtml` 渲染：评论列表 + 底部输入表单
4. 用户填写评论，点击提交
5. JS 拦截表单，AJAX POST 到 `CommentApiController.Create()`
6. API 创建 Comment ContentItem，返回新评论 HTML
7. JS 动态追加评论到列表

### 评论回复

1. 每条评论旁有"回复"按钮
2. 点击后在评论下方展开回复表单
3. 提交时携带 `RepliedOn` = 父评论 ContentItemId
4. 渲染时回复显示在父评论下方，带缩进和"回复 XXX"标识

### 评论审核

1. 当 `CommentablePartSettings.RequireApproval = true`，新评论 Status = Pending
2. Pending 评论在 DetailAdmin 中显示为半透明 + "待审核"标签
3. 管理员可点击"批准"或"拒绝"按钮
4. AJAX 调用 `CommentApiController.UpdateStatus()` 更新状态

### 独立评论管理页

1. Admin 菜单中添加"评论"入口
2. `AdminController.List()` 查询所有评论，支持按状态/内容类型/日期筛选
3. 支持批量操作：批准、拒绝、删除

### 用户自管理

1. 评论者可在限定时间内编辑/删除自己的评论
2. 评论旁显示"编辑"/"删除"按钮（仅评论者和管理员可见）
3. 编辑通过 AJAX 弹出编辑表单

### 管理员回复标识

1. 管理员回复时 `IsAdminReply = true`
2. 渲染时显示特殊样式：高亮背景 + "管理员"标签 + 紫色左边框

## CommentPartHandler Auto-Sync

Handler 自动同步被评论内容上的 `CommentablePart.CommentCount`，仅统计已审核通过的评论：

```
CommentPartHandler:
├── 评论 Status 变为 Approved 时         → CommentablePart.CommentCount++
├── 评论 Status 从 Approved 变为其他时    → CommentablePart.CommentCount--
├── 评论被删除且之前是 Approved 时        → CommentablePart.CommentCount--
└── 评论 Created 但 Status=Pending 时     → CommentablePart.CommentCount 不变
```

索引记录所有状态的评论，CommentCount 只统计 Approved。

## UI Design

### DetailAdmin 评论界面

- 卡片式评论布局，评论文本与附件视觉一体化
- 管理员回复：紫色左边框 + "管理员"标签
- 回复缩进：子评论缩进 40px，带"回复 XXX"标识
- 待审核评论：半透明显示 + 批准/拒绝按钮
- 评论输入区：textarea + 附件上传一体化
- 附件展示：紧凑文件列表（图标 + 文件名 + 大小），非默认缩略图

### SummaryAdmin

在被评论内容的 SummaryAdmin 视图中显示评论数，如 "3 条评论"。

## Startup Registration

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // CommentablePart (附加在被评论内容上，使内容可评论)
    services.AddContentPart<CommentablePart>()
        .UseDisplayDriver<CommentablePartDisplayDriver>()
        .AddHandler<CommentablePartHandler>();

    // 类型级设置
    services.AddScoped<IContentTypePartDefinitionDisplayDriver, CommentablePartSettingsDisplayDriver>();

    // 索引
    services.AddIndexProvider<CommentPartIndexProvider>();

    // 服务
    services.AddScoped<ICommentService, CommentService>();

    // 数据迁移
    services.AddDataMigration<Migrations>();

    // Liquid 模板支持
    services.Configure<TemplateOptions>(o =>
    {
        o.MemberAccessStrategy.Register<CommentablePartViewModel>();
    });
}
```

## Migration Outline

```csharp
public async Task<int> CreateAsync()
{
    // 1. 注册 CommentablePart 为可附加 Part（附加到被评论内容上）
    await _contentDefinitionManager.AlterPartDefinitionAsync("CommentablePart", builder => builder
        .Attachable()
        .WithDescription("Enables comments for content items.")
    );

    // 2. 定义 Comment ContentType（评论本身）
    await _contentDefinitionManager.AlterTypeDefinitionAsync("Comment", builder => builder
        .WithPart("CommentPart", part => part
            .WithPosition("0")
        )
        .WithPart("CommentText", "TextField", part => part
            .WithPosition("1")
            .WithSettings(new TextFieldSettings
            {
                Hint = "Comment content"
            })
        )
        .WithPart("Attachment", "MediaField", part => part
            .WithPosition("2")
            .WithSettings(new MediaFieldSettings
            {
                Multiple = true,
                AllowedExtensions = ["jpg", "jpeg", "png", "gif", "pdf", "doc", "docx", "xls", "xlsx"]
            })
        )
    );

    return 1;
}
```

## Future Extensibility

- Detail 视图（前台主题）：预留 `CommentablePart.cshtml`，未来实现前台评论展示
- 评论通知：可通过 OrchardCore.Notifications 或工作流实现
- 评论点赞：可扩展 CommentPart 增加 LikeCount
- 评论排序：支持按时间/热度排序
- XSS 防护：评论正文存储前做 HTML Sanitization
