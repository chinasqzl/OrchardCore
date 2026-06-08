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

    public override async Task UnpublishedAsync(PublishContentContext context, CommentPart part)
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
