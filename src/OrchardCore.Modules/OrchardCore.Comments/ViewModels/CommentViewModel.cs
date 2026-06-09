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
