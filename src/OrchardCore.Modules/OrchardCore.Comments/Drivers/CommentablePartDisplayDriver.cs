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

            Initialize<CommentablePartViewModel>(GetDisplayShapeType(context), model =>
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
