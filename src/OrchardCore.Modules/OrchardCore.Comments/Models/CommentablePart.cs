using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentablePart : ContentPart
{
    public int CommentCount { get; set; }
    public bool Closed { get; set; }
}
