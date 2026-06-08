using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Comments.Controllers;

[Admin("Comments/{action}", "Comments.{action}")]
public class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICommentService _commentService;
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        ICommentService commentService,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        INotifier notifier,
        IHtmlLocalizer<AdminController> localizer)
    {
        _authorizationService = authorizationService;
        _commentService = commentService;
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _notifier = notifier;
        H = localizer;
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
            commentShapes.Add(await _contentItemDisplayManager.BuildDisplayAsync(comment, _updateModelAccessor.ModelUpdater, "SummaryAdmin"));
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

        await _notifier.SuccessAsync(H["Bulk action completed successfully."]);
        return RedirectToAction(nameof(List));
    }
}
