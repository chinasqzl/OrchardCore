# OrchardCore Comments Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the OrchardCore.Comments module that enables any content type to have comments, displayed in DetailAdmin views with AJAX submission.

**Architecture:** Comments are independent ContentItems (type "Comment") with a CommentPart for logic data, TextField for content, and MediaField for attachments. A separate CommentablePart is attached to commented content items for metadata (comment count, closed status). Display uses Alternate views to render TextField and MediaField in an integrated comment-card style.

**Tech Stack:** OrchardCore CMS (C#, Razor, YesSql), ASP.NET Core MVC, JavaScript (fetch API)

---

## File Structure

```
src/OrchardCore.Modules/OrchardCore.Comments/
├── Manifest.cs
├── Startup.cs
├── Migrations.cs
├── OrchardCore.Comments.csproj
├── placement.json
├── Models/
│   ├── CommentPart.cs                    # 评论 ContentItem 上的 Part
│   ├── CommentablePart.cs                # 被评论内容上的 Part
│   ├── CommentablePartSettings.cs        # 类型级设置
│   └── CommentStatus.cs                  # 枚举
├── ViewModels/
│   ├── CommentablePartViewModel.cs       # DetailAdmin 展示 VM
│   ├── CommentablePartEditViewModel.cs   # 编辑设置 VM
│   ├── CommentablePartSettingsViewModel.cs
│   ├── CommentCreateViewModel.cs         # 创建评论 VM
│   └── CommentViewModel.cs              # 单条评论 VM
├── Drivers/
│   ├── CommentablePartDisplayDriver.cs   # 评论区 Driver
│   └── CommentablePartSettingsDisplayDriver.cs
├── Handlers/
│   └── CommentPartHandler.cs             # 评论数同步
├── Indexes/
│   └── CommentPartIndex.cs               # Index + IndexProvider
├── Controllers/
│   ├── AdminController.cs                # 独立评论管理列表页
│   └── CommentApiController.cs           # AJAX API
├── Permissions/
│   └── CommentPermissions.cs
├── Services/
│   ├── ICommentService.cs
│   └── CommentService.cs
├── AdminMenu.cs
├── Views/
│   ├── _ViewImports.cshtml
│   ├── CommentablePart.DetailAdmin.cshtml
│   ├── CommentablePart.SummaryAdmin.cshtml
│   ├── CommentablePart.Edit.cshtml
│   ├── CommentablePart.cshtml
│   ├── CommentablePartSettings.Edit.cshtml
│   ├── TextField-CommentText.DetailAdmin.cshtml
│   ├── MediaField-Attachment.DetailAdmin.cshtml
│   ├── Admin/
│   │   └── List.cshtml
│   └── Comment/
│       └── CommentCard.cshtml            # 单条评论卡片 Partial
└── Assets/
    ├── Assets.json
    └── Scripts/
        └── comments.js
```

---

### Task 1: Project Scaffolding

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Manifest.cs`

- [ ] **Step 1: Create the project file**

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <Title>OrchardCore Comments</Title>
    <Description>$(OCCMSDescription)

    The comments module enables content items to have comments.</Description>
    <PackageTags>$(PackageTags) OrchardCoreCMS</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentManagement.Abstractions\OrchardCore.ContentManagement.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentManagement.Display\OrchardCore.ContentManagement.Display.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentManagement\OrchardCore.ContentManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Contents.TagHelpers\OrchardCore.Contents.TagHelpers.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentTypes.Abstractions\OrchardCore.ContentTypes.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Data.Abstractions\OrchardCore.Data.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Liquid.Abstractions\OrchardCore.Liquid.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ResourceManagement\OrchardCore.ResourceManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Media.Core\OrchardCore.Media.Core.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create the Manifest.cs**

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Comments",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The comments module enables content items to have comments.",
    Dependencies = ["OrchardCore.Contents", "OrchardCore.ContentFields", "OrchardCore.Media"],
    Category = "Content Management"
)]
```

- [ ] **Step 3: Add project to solution**

Run: `dotnet sln src/OrchardCore.Modules/OrchardCore.Modules.sln add src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`

- [ ] **Step 4: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds (no compile errors, though no code yet)

- [ ] **Step 5: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/
git commit -m "feat(comments): scaffold OrchardCore.Comments project"
```

---

### Task 2: Models and CommentStatus Enum

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Models/CommentStatus.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Models/CommentPart.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Models/CommentablePart.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Models/CommentablePartSettings.cs`

- [ ] **Step 1: Create CommentStatus enum**

```csharp
namespace OrchardCore.Comments.Models;

public enum CommentStatus
{
    Pending,
    Approved,
    Rejected
}
```

- [ ] **Step 2: Create CommentPart model**

```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentPart : ContentPart
{
    /// <summary>
    /// The ContentItemId of the content item being commented on.
    /// </summary>
    public string CommentedOn { get; set; }

    /// <summary>
    /// The ContentItemId of the parent comment being replied to. Null for top-level comments.
    /// </summary>
    public string RepliedOn { get; set; }

    /// <summary>
    /// Whether this comment is an admin reply.
    /// </summary>
    public bool IsAdminReply { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The moderation status of the comment.
    /// </summary>
    public CommentStatus Status { get; set; }

    /// <summary>
    /// When the comment was created.
    /// </summary>
    public DateTime? CreatedUtc { get; set; }
}
```

- [ ] **Step 3: Create CommentablePart model**

```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentablePart : ContentPart
{
    /// <summary>
    /// Count of approved comments. Auto-maintained by CommentPartHandler.
    /// </summary>
    public int CommentCount { get; set; }

    /// <summary>
    /// Whether comments are closed for this content item.
    /// </summary>
    public bool Closed { get; set; }
}
```

- [ ] **Step 4: Create CommentablePartSettings model**

```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentablePartSettings
{
    /// <summary>
    /// Whether new comments require approval before being visible.
    /// </summary>
    public bool RequireApproval { get; set; }

    /// <summary>
    /// Whether comments are allowed for this content type.
    /// </summary>
    public bool AllowComments { get; set; } = true;

    /// <summary>
    /// The default status for new comments.
    /// </summary>
    public CommentStatus DefaultStatus { get; set; } = CommentStatus.Approved;
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Models/
git commit -m "feat(comments): add CommentPart, CommentablePart, and CommentStatus models"
```

---

### Task 3: CommentPartIndex and IndexProvider

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Indexes/CommentPartIndex.cs`

- [ ] **Step 1: Create CommentPartIndex and IndexProvider**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Comments.Indexes;

public class CommentPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string CommentedOn { get; set; }
    public string RepliedOn { get; set; }
    public string Status { get; set; }
    public bool IsAdminReply { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}

public class CommentPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<CommentPartIndex>()
            .Map(contentItem =>
            {
                if (!contentItem.Latest && !contentItem.Published)
                {
                    return null;
                }

                if (!contentItem.TryGet<CommentPart>(out var commentPart))
                {
                    return null;
                }

                // Skip soft-deleted comments from index
                if (commentPart.IsDeleted)
                {
                    return null;
                }

                return new CommentPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    CommentedOn = commentPart.CommentedOn,
                    RepliedOn = commentPart.RepliedOn,
                    Status = commentPart.Status.ToString(),
                    IsAdminReply = commentPart.IsAdminReply,
                    IsDeleted = commentPart.IsDeleted,
                    CreatedUtc = commentPart.CreatedUtc,
                    Published = contentItem.Published,
                    Latest = contentItem.Latest,
                };
            });
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Indexes/
git commit -m "feat(comments): add CommentPartIndex and IndexProvider"
```

---

### Task 4: Permissions

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Permissions/CommentPermissions.cs`

- [ ] **Step 1: Create CommentPermissions**

```csharp
using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments.Permissions;

public static class CommentPermissions
{
    public static readonly Permission ManageComments = new("ManageComments", "Manage all comments");
    public static readonly Permission CreateComments = new("CreateComments", "Create comments", new[] { ManageComments });
    public static readonly Permission EditOwnComment = new("EditOwnComment", "Edit own comments", new[] { CreateComments });
    public static readonly Permission DeleteOwnComment = new("DeleteOwnComment", "Delete own comments", new[] { CreateComments });
    public static readonly Permission AdminReply = new("AdminReply", "Reply as admin", new[] { CreateComments });

    public static IEnumerable<Permission> GetPermissions()
    {
        return
        [
            ManageComments,
            CreateComments,
            EditOwnComment,
            DeleteOwnComment,
            AdminReply,
        ];
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Permissions/
git commit -m "feat(comments): add comment permissions"
```

---

### Task 5: ICommentService and CommentService

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Services/ICommentService.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Services/CommentService.cs`

- [ ] **Step 1: Create ICommentService**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Services;

public interface ICommentService
{
    /// <summary>
    /// Get comments for a content item, ordered by CreatedUtc.
    /// </summary>
    Task<IEnumerable<ContentItem>> GetCommentsAsync(string commentedOn, CommentStatus? status = null);

    /// <summary>
    /// Get the count of approved comments for a content item.
    /// </summary>
    Task<int> GetApprovedCommentCountAsync(string commentedOn);

    /// <summary>
    /// Get all comments for admin list with filtering.
    /// </summary>
    Task<IEnumerable<ContentItem>> GetCommentsForAdminListAsync(CommentStatus? status = null, int? page = null, int? pageSize = null);

    /// <summary>
    /// Sync the CommentCount on the CommentablePart of the commented content item.
    /// </summary>
    Task SyncCommentCountAsync(string commentedOn);
}
```

- [ ] **Step 2: Create CommentService**

```csharp
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Comments.Services;

public class CommentService : ICommentService
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;

    public CommentService(ISession session, IContentManager contentManager)
    {
        _session = session;
        _contentManager = contentManager;
    }

    public async Task<IEnumerable<ContentItem>> GetCommentsAsync(string commentedOn, CommentStatus? status = null)
    {
        var query = _session.Query<ContentItem, CommentPartIndex>(x =>
            x.CommentedOn == commentedOn &&
            x.Published &&
            !x.IsDeleted);

        if (status.HasValue)
        {
            var statusStr = status.Value.ToString();
            query = query.With<CommentPartIndex>(x => x.Status == statusStr);
        }
        else
        {
            query = query.With<CommentPartIndex>(x => x.Status == CommentStatus.Approved.ToString());
        }

        query = query.With<CommentPartIndex>(x => x.CreatedUtc != null)
            .OrderBy(x => x.CreatedUtc);

        return await query.ListAsync(_contentManager);
    }

    public async Task<int> GetApprovedCommentCountAsync(string commentedOn)
    {
        return await _session.Query<ContentItem, CommentPartIndex>(x =>
            x.CommentedOn == commentedOn &&
            x.Status == CommentStatus.Approved.ToString() &&
            x.Published &&
            !x.IsDeleted).CountAsync();
    }

    public async Task<IEnumerable<ContentItem>> GetCommentsForAdminListAsync(CommentStatus? status = null, int? page = null, int? pageSize = null)
    {
        var query = _session.Query<ContentItem, CommentPartIndex>(x => !x.IsDeleted && x.Latest);

        if (status.HasValue)
        {
            var statusStr = status.Value.ToString();
            query = query.With<CommentPartIndex>(x => x.Status == statusStr);
        }

        query = query.With<CommentPartIndex>(x => x.CreatedUtc != null)
            .OrderByDescending(x => x.CreatedUtc);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ListAsync(_contentManager);
    }

    public async Task SyncCommentCountAsync(string commentedOn)
    {
        var contentItem = await _contentManager.GetAsync(commentedOn, VersionOptions.Published);
        if (contentItem == null)
        {
            return;
        }

        if (!contentItem.TryGet<CommentablePart>(out var commentablePart))
        {
            return;
        }

        var count = await GetApprovedCommentCountAsync(commentedOn);
        commentablePart.CommentCount = count;
        contentItem.Apply(commentablePart);
        await _contentManager.UpdateAsync(contentItem);
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Services/
git commit -m "feat(comments): add ICommentService and CommentService"
```

---

### Task 6: ViewModels

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/CommentViewModel.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/CommentablePartViewModel.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/CommentablePartEditViewModel.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/CommentablePartSettingsViewModel.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/CommentCreateViewModel.cs`

- [ ] **Step 1: Create CommentViewModel**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.ViewModels;

public class CommentViewModel
{
    public ContentItem ContentItem { get; set; }
    public CommentPart CommentPart { get; set; }
    public string Author { get; set; }
    public string CommentText { get; set; }
    public List<string> AttachmentPaths { get; set; } = [];
    public List<string> AttachmentNames { get; set; } = [];
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanModerate { get; set; }
}
```

- [ ] **Step 2: Create CommentablePartViewModel**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.ViewModels;

public class CommentablePartViewModel
{
    public CommentablePart CommentablePart { get; set; }
    public ContentItem ContentItem { get; set; }
    public CommentablePartSettings Settings { get; set; }
    public IEnumerable<CommentViewModel> Comments { get; set; } = [];
    public bool IsAuthorizedToComment { get; set; }
    public bool IsAdminReplyAllowed { get; set; }
}
```

- [ ] **Step 3: Create CommentablePartEditViewModel**

```csharp
using OrchardCore.Comments.Models;

namespace OrchardCore.Comments.ViewModels;

public class CommentablePartEditViewModel
{
    public bool Closed { get; set; }
}
```

- [ ] **Step 4: Create CommentablePartSettingsViewModel**

```csharp
using OrchardCore.Comments.Models;

namespace OrchardCore.Comments.ViewModels;

public class CommentablePartSettingsViewModel
{
    public CommentablePartSettings Settings { get; set; } = new();
}
```

- [ ] **Step 5: Create CommentCreateViewModel**

```csharp
namespace OrchardCore.Comments.ViewModels;

public class CommentCreateViewModel
{
    public string CommentedOn { get; set; }
    public string RepliedOn { get; set; }
    public string Text { get; set; }
    public List<string> Attachments { get; set; } = [];
}
```

- [ ] **Step 6: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/ViewModels/
git commit -m "feat(comments): add view models"
```

---

### Task 7: CommentPartHandler

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Handlers/CommentPartHandler.cs`

- [ ] **Step 1: Create CommentPartHandler**

The handler syncs CommentablePart.CommentCount when a comment's status changes.

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Services;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Comments.Handlers;

public class CommentPartHandler : ContentPartHandler<CommentPart>
{
    private readonly ICommentService _commentService;

    public CommentPartHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public override async Task PublishedAsync(PublishContentContext context, CommentPart part)
    {
        if (part.IsDeleted)
        {
            return;
        }

        await _commentService.SyncCommentCountAsync(part.CommentedOn);
    }

    public override async Task UnpublishedAsync(UnpublishContentContext context, CommentPart part)
    {
        if (part.IsDeleted)
        {
            return;
        }

        await _commentService.SyncCommentCountAsync(part.CommentedOn);
    }

    public override async Task RemovedAsync(RemoveContentContext context, CommentPart part)
    {
        if (!part.IsDeleted)
        {
            await _commentService.SyncCommentCountAsync(part.CommentedOn);
        }
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Handlers/
git commit -m "feat(comments): add CommentPartHandler for comment count sync"
```

---

### Task 8: Drivers

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Drivers/CommentablePartDisplayDriver.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Drivers/CommentablePartSettingsDisplayDriver.cs`

- [ ] **Step 1: Create CommentablePartDisplayDriver**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Comments.Drivers;

public class CommentablePartDisplayDriver : ContentPartDisplayDriver<CommentablePart>
{
    private readonly ICommentService _commentService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentManager _contentManager;

    public CommentablePartDisplayDriver(
        ICommentService commentService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IContentManager contentManager)
    {
        _commentService = commentService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _contentManager = contentManager;
    }

    public override IDisplayResult Display(CommentablePart part, BuildPartDisplayContext context)
    {
        return Combine(
            Initialize<CommentablePartViewModel>(GetDisplayShapeType(context), async model =>
            {
                await PopulateViewModelAsync(model, part, context);
            })
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:20")
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:20"),

            Initialize<CommentablePartViewModel>("CommentablePartSummaryAdmin", model =>
            {
                model.CommentablePart = part;
                model.ContentItem = part.ContentItem;
            })
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
        );
    }

    public override IDisplayResult Edit(CommentablePart part, BuildPartEditorContext context)
    {
        return Initialize<CommentablePartEditViewModel>(GetEditorShapeType(context), model =>
        {
            model.Closed = part.Closed;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(CommentablePart part, UpdatePartEditorContext context)
    {
        var model = new CommentablePartEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Closed);
        part.Closed = model.Closed;
        return Edit(part, context);
    }

    private async Task PopulateViewModelAsync(CommentablePartViewModel model, CommentablePart part, BuildPartDisplayContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var settings = context.TypePartDefinition.GetSettings<CommentablePartSettings>();

        model.CommentablePart = part;
        model.ContentItem = part.ContentItem;
        model.Settings = settings;
        model.IsAuthorizedToComment = user != null && await _authorizationService.AuthorizeAsync(user, CommentPermissions.CreateComments);
        model.IsAdminReplyAllowed = user != null && await _authorizationService.AuthorizeAsync(user, CommentPermissions.AdminReply);

        var commentItems = await _commentService.GetCommentsAsync(part.ContentItem.ContentItemId);

        var isModerator = user != null && await _authorizationService.AuthorizeAsync(user, CommentPermissions.ManageComments);

        var commentViewModels = new List<CommentViewModel>();
        foreach (var commentItem in commentItems)
        {
            var commentPart = commentItem.As<CommentPart>();
            if (commentPart == null) continue;

            var canEdit = user != null &&
                (await _authorizationService.AuthorizeAsync(user, CommentPermissions.EditOwnComment) &&
                 commentItem.Author == user.Identity?.Name);

            var canDelete = user != null &&
                ((await _authorizationService.AuthorizeAsync(user, CommentPermissions.DeleteOwnComment) &&
                  commentItem.Author == user.Identity?.Name) ||
                 isModerator);

            var commentText = commentItem.Content?.CommentText?.Text?.ToString() ?? "";
            var attachmentPaths = new List<string>();
            var attachmentNames = new List<string>();

            if (commentItem.Content?.Attachment?.Paths != null)
            {
                foreach (var path in commentItem.Content.Attachment.Paths)
                {
                    attachmentPaths.Add(path.ToString());
                }
            }
            if (commentItem.Content?.Attachment?.MediaTexts != null)
            {
                foreach (var text in commentItem.Content.Attachment.MediaTexts)
                {
                    attachmentNames.Add(text.ToString());
                }
            }

            commentViewModels.Add(new CommentViewModel
            {
                ContentItem = commentItem,
                CommentPart = commentPart,
                Author = commentItem.Author,
                CommentText = commentText,
                AttachmentPaths = attachmentPaths,
                AttachmentNames = attachmentNames,
                CanEdit = canEdit,
                CanDelete = canDelete,
                CanModerate = isModerator,
            });
        }

        model.Comments = commentViewModels;
    }
}
```

- [ ] **Step 2: Create CommentablePartSettingsDisplayDriver**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Comments.Drivers;

public class CommentablePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    public override IDisplayResult Edit(ContentTypePartDefinition model)
    {
        if (!string.Equals(model.PartDefinition.Name, "CommentablePart", StringComparison.Ordinal))
        {
            return null;
        }

        return Initialize<CommentablePartSettingsViewModel>("CommentablePartSettings_Edit", viewModel =>
        {
            var settings = model.GetSettings<CommentablePartSettings>();
            viewModel.Settings = settings;
        })
        .Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
    {
        if (!string.Equals(model.PartDefinition.Name, "CommentablePart", StringComparison.Ordinal))
        {
            return null;
        }

        var viewModel = new CommentablePartSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix,
            m => m.Settings.RequireApproval,
            m => m.Settings.AllowComments,
            m => m.Settings.DefaultStatus);

        context.Builder.WithSettings(viewModel.Settings);
        return Edit(model);
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Drivers/
git commit -m "feat(comments): add CommentablePartDisplayDriver and SettingsDisplayDriver"
```

---

### Task 9: Migrations

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Migrations.cs`

- [ ] **Step 1: Create Migrations**

```csharp
using OrchardCore.Comments.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Comments;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        // Register CommentablePart as attachable part
        await _contentDefinitionManager.AlterPartDefinitionAsync("CommentablePart", builder => builder
            .Attachable()
            .WithDescription("Enables comments for content items.")
        );

        // Define Comment ContentType
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Comment", builder => builder
            .WithSetting("Creatable", "false")
            .WithSetting("Listable", "false")
            .WithSetting("Draftable", "false")
            .WithSetting("Securable", "false")
            .WithPart("CommentPart", part => part
                .WithPosition("0")
            )
            .WithPart("CommentText", "TextField", part => part
                .WithPosition("1")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Comment content",
                    Editor = EditorType.TextArea,
                })
            )
            .WithPart("Attachment", "MediaField", part => part
                .WithPosition("2")
                .WithSettings(new MediaFieldSettings
                {
                    Multiple = true,
                    AllowedFileExtensions = [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"],
                })
            )
        );

        return 1;
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Migrations.cs
git commit -m "feat(comments): add Migrations for CommentablePart and Comment ContentType"
```

---

### Task 10: Startup and Placement

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Startup.cs`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/placement.json`

- [ ] **Step 1: Create Startup.cs**

```csharp
using Fluid;
using OrchardCore.Comments.Drivers;
using OrchardCore.Comments.Handlers;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<CommentablePartViewModel>();
            o.MemberAccessStrategy.Register<CommentViewModel>();
            o.MemberAccessStrategy.Register<CommentablePartSettingsViewModel>();
        });

        // CommentablePart (attached to commented content items)
        services.AddContentPart<CommentablePart>()
            .UseDisplayDriver<CommentablePartDisplayDriver>();

        // CommentPart (on Comment ContentItem) - no display driver needed, handled by API
        services.AddContentPart<CommentPart>()
            .AddHandler<CommentPartHandler>();

        // Type-level settings
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, CommentablePartSettingsDisplayDriver>();

        // Index
        services.AddIndexProvider<CommentPartIndexProvider>();

        // Services
        services.AddScoped<ICommentService, CommentService>();

        // Migrations
        services.AddDataMigration<Migrations>();

        // Permissions
        services.AddScoped<IPermissionProvider, CommentPermissions>();

        // Admin menu
        services.AddScoped<INavigationProvider, AdminMenu>();
    }
}
```

- [ ] **Step 2: Create placement.json**

```json
{
  "CommentablePart": [
    {
      "place": "Content:20",
      "displayType": "DetailAdmin"
    },
    {
      "place": "Content:20",
      "displayType": "Detail"
    },
    {
      "place": "Meta:5",
      "displayType": "SummaryAdmin"
    }
  ]
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Startup.cs src/OrchardCore.Modules/OrchardCore.Comments/placement.json
git commit -m "feat(comments): add Startup and placement.json"
```

---

### Task 11: AdminMenu

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/AdminMenu.cs`

- [ ] **Step 1: Create AdminMenu**

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Comments.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.Comments;

public class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder.Add(S["Content"], content =>
        {
            content.Add(S["Comments"], S["Comments"].Value, entry =>
            {
                entry.Action("List", "Admin", new { area = "OrchardCore.Comments" })
                    .Permission(CommentPermissions.ManageComments)
                    .LocalNav();
            });
        });

        return ValueTask.CompletedTask;
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/AdminMenu.cs
git commit -m "feat(comments): add AdminMenu for comments list page"
```

---

### Task 12: CommentApiController

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Controllers/CommentApiController.cs`

- [ ] **Step 1: Create CommentApiController**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Comments.Controllers;

[Route("api/comments")]
[ApiController]
[Area("OrchardCore.Comments")]
public class CommentApiController : ControllerBase
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICommentService _commentService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CommentApiController(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        ICommentService commentService,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
        _commentService = commentService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CommentCreateViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommentPermissions.CreateComments))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(model.CommentedOn) || string.IsNullOrWhiteSpace(model.Text))
        {
            return BadRequest("CommentedOn and Text are required.");
        }

        // Verify the commented-on content item exists and has CommentablePart
        var commentedItem = await _contentManager.GetAsync(model.CommentedOn, VersionOptions.Published);
        if (commentedItem == null)
        {
            return NotFound("Content item not found.");
        }

        if (!commentedItem.TryGet<CommentablePart>(out var commentablePart))
        {
            return BadRequest("This content item does not support comments.");
        }

        if (commentablePart.Closed)
        {
            return BadRequest("Comments are closed for this content item.");
        }

        // Get settings
        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(commentedItem.ContentType);
        var partDefinition = typeDefinition?.Parts.FirstOrDefault(p => p.PartDefinition.Name == "CommentablePart");
        var settings = partDefinition?.GetSettings<CommentablePartSettings>() ?? new CommentablePartSettings();

        if (!settings.AllowComments)
        {
            return BadRequest("Comments are not allowed for this content type.");
        }

        // Create comment ContentItem
        var comment = await _contentManager.NewAsync("Comment");

        var commentPart = comment.As<CommentPart>();
        commentPart.CommentedOn = model.CommentedOn;
        commentPart.RepliedOn = model.RepliedOn;
        commentPart.Status = settings.RequireApproval ? CommentStatus.Pending : settings.DefaultStatus;
        commentPart.CreatedUtc = DateTime.UtcNow;
        commentPart.IsDeleted = false;

        // Check admin reply
        var isAdmin = await _authorizationService.AuthorizeAsync(User, CommentPermissions.AdminReply);
        commentPart.IsAdminReply = isAdmin;

        comment.Apply(commentPart);

        // Set comment text via TextField
        comment.Content.CommentText = new { Text = model.Text };

        // Set attachments via MediaField
        if (model.Attachments?.Count > 0)
        {
            comment.Content.Attachment = new
            {
                Paths = model.Attachments,
                MediaTexts = model.Attachments.Select(p => System.IO.Path.GetFileName(p)).ToList()
            };
        }

        comment.Author = User.Identity?.Name;
        comment.DisplayText = model.Text.Length > 50 ? model.Text[..50] + "..." : model.Text;

        await _contentManager.CreateAsync(comment, VersionOptions.Published);

        return Ok(new
        {
            success = true,
            commentId = comment.ContentItemId,
            status = commentPart.Status.ToString()
        });
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommentPermissions.ManageComments))
        {
            return Forbid();
        }

        var comment = await _contentManager.GetAsync(id, VersionOptions.Latest);
        if (comment == null)
        {
            return NotFound();
        }

        var commentPart = comment.As<CommentPart>();
        var oldStatus = commentPart.Status;
        commentPart.Status = Enum.Parse<CommentStatus>(model.Status);
        comment.Apply(commentPart);

        await _contentManager.UpdateAsync(comment);
        await _contentManager.PublishAsync(comment);

        return Ok(new { success = true });
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var comment = await _contentManager.GetAsync(id, VersionOptions.Latest);
        if (comment == null)
        {
            return NotFound();
        }

        var commentPart = comment.As<CommentPart>();
        var isOwner = comment.Author == User.Identity?.Name;
        var canManage = await _authorizationService.AuthorizeAsync(User, CommentPermissions.ManageComments);

        if (!isOwner && !canManage)
        {
            if (!isOwner || !await _authorizationService.AuthorizeAsync(User, CommentPermissions.DeleteOwnComment))
            {
                return Forbid();
            }
        }

        // Soft delete
        commentPart.IsDeleted = true;
        comment.Apply(commentPart);
        await _contentManager.UpdateAsync(comment);

        // Unpublish to hide from display
        await _contentManager.UnpublishAsync(comment);

        return Ok(new { success = true });
    }

    [HttpPost("{id}/update")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCommentModel model)
    {
        var comment = await _contentManager.GetAsync(id, VersionOptions.Latest);
        if (comment == null)
        {
            return NotFound();
        }

        var isOwner = comment.Author == User.Identity?.Name;
        if (!isOwner || !await _authorizationService.AuthorizeAsync(User, CommentPermissions.EditOwnComment))
        {
            return Forbid();
        }

        comment.Content.CommentText = new { Text = model.Text };
        comment.DisplayText = model.Text.Length > 50 ? model.Text[..50] + "..." : model.Text;

        await _contentManager.UpdateAsync(comment);
        await _contentManager.PublishAsync(comment);

        return Ok(new { success = true });
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] string commentedOn, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommentPermissions.CreateComments))
        {
            return Forbid();
        }

        var comments = await _commentService.GetCommentsAsync(commentedOn);
        return Ok(new
        {
            items = comments.Select(c => new
            {
                id = c.ContentItemId,
                text = c.Content?.CommentText?.Text?.ToString(),
                author = c.Author,
                createdUtc = c.CreatedUtc,
            }),
            totalCount = comments.Count()
        });
    }
}

public class UpdateStatusModel
{
    public string Status { get; set; }
}

public class UpdateCommentModel
{
    public string Text { get; set; }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Controllers/
git commit -m "feat(comments): add CommentApiController for AJAX operations"
```

---

### Task 13: AdminController

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Controllers/AdminController.cs`

- [ ] **Step 1: Create AdminController**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Comments.Controllers;

[Admin("Comments/{action}", "Comments.{action}")]
public class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICommentService _commentService;
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        ICommentService commentService,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        INotifier notifier,
        IStringLocalizer<AdminController> localizer)
    {
        _authorizationService = authorizationService;
        _commentService = commentService;
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _notifier = notifier;
        S = localizer;
    }

    public async Task<IActionResult> List(CommentStatus? status = null, int page = 1, int pageSize = 20)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommentPermissions.ManageComments))
        {
            return Forbid();
        }

        var comments = await _commentService.GetCommentsForAdminListAsync(status, page, pageSize);

        var commentShapes = new List<dynamic>();
        foreach (var comment in comments)
        {
            commentShapes.Add(await _contentItemDisplayManager.BuildDisplayAsync(comment, this, "SummaryAdmin"));
        }

        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;

        return View(commentShapes);
    }

    [HttpPost]
    public async Task<IActionResult> BatchAction(string[] itemIds, string action)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommentPermissions.ManageComments))
        {
            return Forbid();
        }

        if (itemIds == null || itemIds.Length == 0)
        {
            return RedirectToAction(nameof(List));
        }

        foreach (var id in itemIds)
        {
            var comment = await _contentManager.GetAsync(id, VersionOptions.Latest);
            if (comment == null) continue;

            var commentPart = comment.As<CommentPart>();

            switch (action)
            {
                case "Approve":
                    commentPart.Status = CommentStatus.Approved;
                    comment.Apply(commentPart);
                    await _contentManager.UpdateAsync(comment);
                    await _contentManager.PublishAsync(comment);
                    break;
                case "Reject":
                    commentPart.Status = CommentStatus.Rejected;
                    comment.Apply(commentPart);
                    await _contentManager.UpdateAsync(comment);
                    await _contentManager.PublishAsync(comment);
                    break;
                case "Delete":
                    commentPart.IsDeleted = true;
                    comment.Apply(commentPart);
                    await _contentManager.UpdateAsync(comment);
                    await _contentManager.UnpublishAsync(comment);
                    break;
            }
        }

        await _notifier.SuccessAsync(S["Bulk action completed successfully."]);
        return RedirectToAction(nameof(List));
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Controllers/AdminController.cs
git commit -m "feat(comments): add AdminController for comments management page"
```

---

### Task 14: Views

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/_ViewImports.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/CommentablePart.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/CommentablePart.SummaryAdmin.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/CommentablePart.Edit.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/CommentablePart.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/CommentablePartSettings.Edit.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/TextField-CommentText.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/MediaField-Attachment.DetailAdmin.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/Comment/CommentCard.cshtml`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Views/Admin/List.cshtml`

- [ ] **Step 1: Create _ViewImports.cshtml**

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
@addTagHelper *, OrchardCore.Contents.TagHelpers
```

- [ ] **Step 2: Create CommentablePart.DetailAdmin.cshtml**

This is the main comment section view rendered in DetailAdmin.

```cshtml
@model OrchardCore.Comments.ViewModels.CommentablePartViewModel

<div class="comments-section" data-commented-on="@Model.ContentItem.ContentItemId">
    <div class="d-flex align-items-center justify-content-between mb-3">
        <h5 class="mb-0">
            @T["Comments"]
            <span class="badge bg-secondary ms-1">@Model.CommentablePart.CommentCount</span>
        </h5>
        @if (Model.IsAuthorizedToComment && !Model.CommentablePart.Closed)
        {
            <button type="button" class="btn btn-primary btn-sm" onclick="comments.toggleForm()">
                <i class="fas fa-plus"></i> @T["Write a comment"]
            </button>
        }
    </div>

    @if (Model.CommentablePart.Closed)
    {
        <div class="alert alert-info">@T["Comments are closed."]</div>
    }

    <!-- Comment list -->
    <div class="comments-list">
        @foreach (var comment in Model.Comments.Where(c => string.IsNullOrEmpty(c.CommentPart.RepliedOn)))
        {
            <partial name="Comment/CommentCard" model="comment" />
            <!-- Replies -->
            var replies = Model.Comments.Where(c => c.CommentPart.RepliedOn == comment.ContentItem.ContentItemId);
            foreach (var reply in replies)
            {
                <div class="ms-4" style="border-left: 3px solid #6f42c1; margin-left: 40px;">
                    <partial name="Comment/CommentCard" model="reply" />
                </div>
            }
        }
    </div>

    <!-- Comment form -->
    @if (Model.IsAuthorizedToComment && !Model.CommentablePart.Closed)
    {
        <div id="comment-form" class="card mt-3" style="display: none;">
            <div class="card-body">
                <h6 class="card-title">@T["Write a comment"]</h6>
                <textarea id="comment-text" class="form-control mb-2" rows="4" placeholder="@T["Write your comment..."]"></textarea>

                <div class="d-flex align-items-center gap-2 mb-2">
                    <label class="btn btn-outline-primary btn-sm" for="comment-attachment">
                        <i class="fas fa-paperclip"></i> @T["Add attachment"]
                        <input type="file" id="comment-attachment" multiple style="display: none;" />
                    </label>
                    <small class="text-muted">@T["Supports doc, pdf, png, jpg (max 10MB)"]</small>
                </div>
                <div id="comment-attachment-list" class="d-flex gap-1 flex-wrap mb-2"></div>

                <input type="hidden" id="comment-replied-on" value="" />

                <div class="d-flex justify-content-end gap-2">
                    <button type="button" class="btn btn-secondary btn-sm" onclick="comments.toggleForm()">@T["Cancel"]</button>
                    <button type="button" class="btn btn-primary btn-sm" onclick="comments.submit()">@T["Submit"]</button>
                </div>
            </div>
        </div>
    }
</div>

<script at="Foot" asp-name="comments-js" asp-src="~/OrchardCore.Comments/Scripts/comments.min.js"></script>
```

- [ ] **Step 3: Create Comment/CommentCard.cshtml**

```cshtml
@model OrchardCore.Comments.ViewModels.CommentViewModel

<div class="card mb-2 @(Model.CommentPart.IsAdminReply ? "border-start border-3 border-purple" : "")" style="@(Model.CommentPart.Status == CommentStatus.Pending ? "opacity: 0.65;" : "")">
    <div class="card-body py-2 px-3">
        <div class="d-flex align-items-center mb-1">
            <div class="avatar-circle me-2" style="width:32px;height:32px;background:@(Model.CommentPart.IsAdminReply ? "#6f42c1" : "#20c997");color:#fff;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:bold;">
                @Model.Author?.Substring(0, Math.Min(1, Model.Author?.Length ?? 0))
            </div>
            <div>
                <strong>@Model.Author</strong>
                @if (Model.CommentPart.IsAdminReply)
                {
                    <span class="badge bg-warning text-dark ms-1" style="font-size:10px;">@T["Admin"]</span>
                }
                @if (Model.CommentPart.Status == CommentStatus.Pending)
                {
                    <span class="badge bg-warning text-dark ms-1" style="font-size:10px;">@T["Pending"]</span>
                }
                <div class="text-muted" style="font-size:12px;">@Model.CommentPart.CreatedUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")</div>
            </div>
            <div class="ms-auto">
                @if (Model.CanEdit)
                {
                    <a href="#" class="text-primary me-2" style="font-size:12px;" onclick="comments.edit('@Model.ContentItem.ContentItemId')">@T["Edit"]</a>
                }
                @if (Model.CanDelete)
                {
                    <a href="#" class="text-danger me-2" style="font-size:12px;" onclick="comments.deleteComment('@Model.ContentItem.ContentItemId')">@T["Delete"]</a>
                }
                @if (Model.CanModerate && Model.CommentPart.Status == CommentStatus.Pending)
                {
                    <button class="btn btn-success btn-xs me-1" style="font-size:11px;" onclick="comments.updateStatus('@Model.ContentItem.ContentItemId', 'Approved')">@T["Approve"]</button>
                    <button class="btn btn-danger btn-xs" style="font-size:11px;" onclick="comments.updateStatus('@Model.ContentItem.ContentItemId', 'Rejected')">@T["Reject"]</button>
                }
                <a href="#" class="text-primary ms-2" style="font-size:12px;" onclick="comments.reply('@Model.ContentItem.ContentItemId')">@T["Reply"]</a>
            </div>
        </div>
        <div class="comment-text">@Html.Raw(Model.CommentText)</div>
        @if (Model.AttachmentPaths?.Count > 0)
        {
            <div class="mt-2 p-2 rounded" style="background:#f0f4ff;border:1px solid #c5d3f7;">
                <div class="text-primary fw-bold" style="font-size:12px;">@T["Attachments"]</div>
                <div class="d-flex gap-1 flex-wrap mt-1">
                    @for (var i = 0; i < Model.AttachmentPaths.Count; i++)
                    {
                        <div class="bg-white border rounded px-2 py-1" style="font-size:13px;">
                            <a href="@Model.AttachmentPaths[i]" target="_blank" class="text-primary">@(Model.AttachmentNames.Count > i ? Model.AttachmentNames[i] : System.IO.Path.GetFileName(Model.AttachmentPaths[i]))</a>
                        </div>
                    }
                </div>
            </div>
        }
    </div>
</div>
```

- [ ] **Step 4: Create CommentablePart.SummaryAdmin.cshtml**

```cshtml
@model OrchardCore.Comments.ViewModels.CommentablePartViewModel

<span class="badge bg-info">
    <i class="fas fa-comment"></i> @Model.CommentablePart.CommentCount @T["comments"]
</span>
```

- [ ] **Step 5: Create CommentablePart.Edit.cshtml**

```cshtml
@model OrchardCore.Comments.ViewModels.CommentablePartEditViewModel

<div class="form-check">
    <input type="checkbox" asp-for="Closed" class="form-check-input" />
    <label asp-for="Closed" class="form-check-label">@T["Close comments"]</label>
    <span class="hint">@T["Check to prevent new comments on this content item."]</span>
</div>
```

- [ ] **Step 6: Create CommentablePart.cshtml (Detail - placeholder)**

```cshtml
@model OrchardCore.Comments.ViewModels.CommentablePartViewModel

@* Frontend Detail view - placeholder for future implementation *@
```

- [ ] **Step 7: Create CommentablePartSettings.Edit.cshtml**

```cshtml
@model OrchardCore.Comments.ViewModels.CommentablePartSettingsViewModel

<div class="form-check">
    <input type="checkbox" asp-for="Settings.AllowComments" class="form-check-input" />
    <label asp-for="Settings.AllowComments" class="form-check-label">@T["Allow comments"]</label>
</div>

<div class="form-check">
    <input type="checkbox" asp-for="Settings.RequireApproval" class="form-check-input" />
    <label asp-for="Settings.RequireApproval" class="form-check-label">@T["Require approval"]</label>
    <span class="hint">@T["When enabled, new comments require admin approval before being visible."]</span>
</div>

<div class="mb-3">
    <label asp-for="Settings.DefaultStatus" class="form-label">@T["Default status for new comments"]</label>
    <select asp-for="Settings.DefaultStatus" class="form-select" asp-items="Html.GetEnumSelectList<CommentStatus>()"></select>
</div>
```

- [ ] **Step 8: Create TextField-CommentText.DetailAdmin.cshtml**

This Alternate view overrides the default TextField rendering for the CommentText field in DetailAdmin context.

```cshtml
@* This view is intentionally minimal - the comment text is rendered directly in CommentCard.cshtml *@
@* The TextField-CommentText alternate prevents the default TextField rendering (with label wrapper) *@
```

- [ ] **Step 9: Create MediaField-Attachment.DetailAdmin.cshtml**

```cshtml
@* This view is intentionally minimal - attachments are rendered directly in CommentCard.cshtml *@
@* The MediaField-Attachment alternate prevents the default MediaField rendering (with thumbnail grid) *@
```

- [ ] **Step 10: Create Admin/List.cshtml**

```cshtml
@model IEnumerable<dynamic>

@using OrchardCore.Comments.Models

@{
    var status = ViewBag.Status as CommentStatus?;
}

<h1>@T["Comments"]</h1>

<form method="get" class="mb-3">
    <div class="row">
        <div class="col-auto">
            <select name="status" class="form-select" onchange="this.form.submit()">
                <option value="">@T["All statuses"]</option>
                <option value="Pending" selected="@(status == CommentStatus.Pending)">@T["Pending"]</option>
                <option value="Approved" selected="@(status == CommentStatus.Approved)">@T["Approved"]</option>
                <option value="Rejected" selected="@(status == CommentStatus.Rejected)">@T["Rejected"]</option>
            </select>
        </div>
    </div>
</form>

<form method="post" asp-action="BatchAction" asp-controller="Admin" asp-area="OrchardCore.Comments">
    <div class="mb-2">
        <button type="submit" name="action" value="Approve" class="btn btn-success btn-sm">@T["Approve selected"]</button>
        <button type="submit" name="action" value="Reject" class="btn btn-warning btn-sm">@T["Reject selected"]</button>
        <button type="submit" name="action" value="Delete" class="btn btn-danger btn-sm">@T["Delete selected"]</button>
    </div>

    <ul class="list-group">
        @foreach (var shape in Model)
        {
            <li class="list-group-item">
                @await DisplayAsync(shape)
            </li>
        }
    </ul>
</form>
```

- [ ] **Step 11: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 12: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Views/
git commit -m "feat(comments): add all views for CommentablePart, alternates, and admin list"
```

---

### Task 15: JavaScript (comments.js)

**Files:**
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Assets/Scripts/comments.js`
- Create: `src/OrchardCore.Modules/OrchardCore.Comments/Assets/Assets.json`

- [ ] **Step 1: Create comments.js**

```javascript
const comments = {
    getAntiForgeryToken: function () {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    },

    getHeaders: function () {
        return {
            'RequestVerificationToken': this.getAntiForgeryToken(),
            'Content-Type': 'application/json'
        };
    },

    toggleForm: function (repliedOn) {
        const form = document.getElementById('comment-form');
        const repliedOnInput = document.getElementById('comment-replied-on');

        if (form.style.display === 'none') {
            form.style.display = 'block';
            if (repliedOn) {
                repliedOnInput.value = repliedOn;
            } else {
                repliedOnInput.value = '';
            }
            document.getElementById('comment-text').focus();
        } else {
            form.style.display = 'none';
            repliedOnInput.value = '';
            document.getElementById('comment-text').value = '';
        }
    },

    submit: async function () {
        const section = document.querySelector('.comments-section');
        const commentedOn = section.dataset.commentedOn;
        const text = document.getElementById('comment-text').value.trim();
        const repliedOn = document.getElementById('comment-replied-on').value;

        if (!text) {
            alert('Please enter a comment.');
            return;
        }

        try {
            const response = await fetch('/api/comments/create', {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({
                    commentedOn: commentedOn,
                    repliedOn: repliedOn || null,
                    text: text,
                    attachments: []
                })
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Reload the page to show the new comment
                    // In a more sophisticated implementation, we would dynamically add the comment card
                    window.location.reload();
                }
            } else if (response.status === 403) {
                alert('You are not authorized to comment.');
            } else {
                const error = await response.json();
                alert('Error: ' + (error.message || 'Failed to submit comment.'));
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    reply: function (commentId) {
        this.toggleForm(commentId);
    },

    deleteComment: async function (commentId) {
        if (!confirm('Are you sure you want to delete this comment?')) return;

        try {
            const response = await fetch(`/api/comments/${commentId}/delete`, {
                method: 'POST',
                headers: this.getHeaders()
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    updateStatus: async function (commentId, status) {
        try {
            const response = await fetch(`/api/comments/${commentId}/status`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({ status: status })
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    edit: async function (commentId) {
        const newText = prompt('Edit your comment:');
        if (!newText) return;

        try {
            const response = await fetch(`/api/comments/${commentId}/update`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({ text: newText })
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    }
};
```

- [ ] **Step 2: Create Assets.json**

```json
[
  {
    "action": "min",
    "name": "comments",
    "source": "Assets/Scripts/comments.js",
    "tags": ["admin", "js"]
  }
]
```

- [ ] **Step 3: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/Assets/
git commit -m "feat(comments): add comments.js and Assets.json"
```

---

### Task 16: Fix Startup - Add Route Registration

**Files:**
- Modify: `src/OrchardCore.Modules/OrchardCore.Comments/Startup.cs`

- [ ] **Step 1: Update Startup to add API route and permission provider**

The Startup needs to register routes for the API controller and Admin controller. Add a `Configure` method.

```csharp
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Comments.Drivers;
using OrchardCore.Comments.Handlers;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Comments;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<CommentablePartViewModel>();
            o.MemberAccessStrategy.Register<CommentViewModel>();
            o.MemberAccessStrategy.Register<CommentablePartSettingsViewModel>();
        });

        // CommentablePart (attached to commented content items)
        services.AddContentPart<CommentablePart>()
            .UseDisplayDriver<CommentablePartDisplayDriver>();

        // CommentPart (on Comment ContentItem)
        services.AddContentPart<CommentPart>()
            .AddHandler<CommentPartHandler>();

        // Type-level settings
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, CommentablePartSettingsDisplayDriver>();

        // Index
        services.AddIndexProvider<CommentPartIndexProvider>();

        // Services
        services.AddScoped<ICommentService, CommentService>();

        // Migrations
        services.AddDataMigration<Migrations>();

        // Permissions
        services.AddScoped<IPermissionProvider, CommentPermissionsProvider>();

        // Admin menu
        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "CommentsAdmin",
            areaName: "OrchardCore.Comments",
            pattern: "Admin/Comments/{action}",
            defaults: new { controller = "Admin", action = "List" }
        );

        routes.MapAreaControllerRoute(
            name: "CommentsApi",
            areaName: "OrchardCore.Comments",
            pattern: "api/comments/{action}/{id?}",
            defaults: new { controller = "CommentApi" }
        );
    }
}
```

Note: We also need to rename `CommentPermissions` to `CommentPermissionsProvider` since it needs to implement `IPermissionProvider`.

- [ ] **Step 2: Update CommentPermissions to implement IPermissionProvider**

```csharp
using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments.Permissions;

public static class CommentPermissions
{
    public static readonly Permission ManageComments = new("ManageComments", "Manage all comments");
    public static readonly Permission CreateComments = new("CreateComments", "Create comments", new[] { ManageComments });
    public static readonly Permission EditOwnComment = new("EditOwnComment", "Edit own comments", new[] { CreateComments });
    public static readonly Permission DeleteOwnComment = new("DeleteOwnComment", "Delete own comments", new[] { CreateComments });
    public static readonly Permission AdminReply = new("AdminReply", "Reply as admin", new[] { CreateComments });
}

public class CommentPermissionsProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<Permission>>(
        [
            CommentPermissions.ManageComments,
            CommentPermissions.CreateComments,
            CommentPermissions.EditOwnComment,
            CommentPermissions.DeleteOwnComment,
            CommentPermissions.AdminReply,
        ]);
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return
        [
            new PermissionStereotype
            {
                Stereotype = "Administrator",
                Permissions =
                [
                    CommentPermissions.ManageComments,
                    CommentPermissions.CreateComments,
                    CommentPermissions.EditOwnComment,
                    CommentPermissions.DeleteOwnComment,
                    CommentPermissions.AdminReply,
                ]
            },
            new PermissionStereotype
            {
                Stereotype = "Authenticated",
                Permissions =
                [
                    CommentPermissions.CreateComments,
                    CommentPermissions.EditOwnComment,
                    CommentPermissions.DeleteOwnComment,
                ]
            },
        ];
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add src/OrchardCore.Modules/OrchardCore.Comments/
git commit -m "feat(comments): add route registration and IPermissionProvider implementation"
```

---

### Task 17: Full Build Verification

- [ ] **Step 1: Run full solution build**

Run: `dotnet build src/OrchardCore.Modules/OrchardCore.Comments/OrchardCore.Comments.csproj`
Expected: Build succeeds with no errors

- [ ] **Step 2: Check for any missing using directives or namespace issues**

Fix any compilation errors that arise.

- [ ] **Step 3: Final commit**

```bash
git add -A src/OrchardCore.Modules/OrchardCore.Comments/
git commit -m "feat(comments): complete OrchardCore.Comments module implementation"
```
