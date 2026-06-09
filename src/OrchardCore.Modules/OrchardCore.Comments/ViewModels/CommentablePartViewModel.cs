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
