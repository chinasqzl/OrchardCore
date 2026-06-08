using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentPart : ContentPart
{
    public string CommentedOn { get; set; }
    public string RepliedOn { get; set; }
    public bool IsAdminReply { get; set; }
    public bool IsDeleted { get; set; }
    public CommentStatus Status { get; set; }
    public DateTime? CreatedUtc { get; set; }
}
