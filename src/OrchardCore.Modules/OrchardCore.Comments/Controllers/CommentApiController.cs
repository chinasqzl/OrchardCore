using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Media.Fields;

namespace OrchardCore.Comments.Controllers;

[Route("api/comments")]
[ApiController]
[IgnoreAntiforgeryToken]
public class CommentApiController : ControllerBase
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICommentService _commentService;

    public CommentApiController(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        ICommentService commentService)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
        _commentService = commentService;
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

        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(commentedItem.ContentType);
        var partDefinition = typeDefinition?.Parts.FirstOrDefault(p => p.PartDefinition.Name == "CommentablePart");
        var settings = partDefinition?.GetSettings<CommentablePartSettings>() ?? new CommentablePartSettings();

        if (!settings.AllowComments)
        {
            return BadRequest("Comments are not allowed for this content type.");
        }

        var comment = await _contentManager.NewAsync("Comment");

        var commentPart = comment.GetOrCreate<CommentPart>();
        commentPart.CommentedOn = model.CommentedOn;
        commentPart.RepliedOn = model.RepliedOn;
        commentPart.Status = settings.RequireApproval ? CommentStatus.Pending : settings.DefaultStatus;
        commentPart.CreatedUtc = DateTime.UtcNow;
        commentPart.IsDeleted = false;

        var isAdmin = await _authorizationService.AuthorizeAsync(User, CommentPermissions.AdminReply);
        commentPart.IsAdminReply = isAdmin;

        // Set TextField "CommentText" on CommentPart
        commentPart.Alter<TextField>("CommentText", field =>
        {
            field.Text = model.Text;
        });

        // Set MediaField "Attachment" on CommentPart
        if (model.Attachments?.Count > 0)
        {
            commentPart.Alter<MediaField>("Attachment", field =>
            {
                field.Paths = model.Attachments.ToArray();
                field.MediaTexts = model.Attachments.Select(p => System.IO.Path.GetFileName(p)).ToArray();
            });
        }

        comment.Apply(commentPart);

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

        commentPart.IsDeleted = true;
        comment.Apply(commentPart);
        await _contentManager.UpdateAsync(comment);
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

        var commentPart = comment.GetOrCreate<CommentPart>();
        commentPart.Alter<TextField>("CommentText", field =>
        {
            field.Text = model.Text;
        });
        comment.Apply(commentPart);

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
            items = comments.Select(c =>
            {
                var commentPart = c.As<CommentPart>();
                var textField = commentPart?.Get<TextField>("CommentText");
                return new
                {
                    id = c.ContentItemId,
                    text = textField?.Text,
                    author = c.Author,
                    createdUtc = c.CreatedUtc,
                };
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
