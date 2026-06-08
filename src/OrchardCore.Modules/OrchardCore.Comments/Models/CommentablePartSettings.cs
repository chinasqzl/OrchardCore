namespace OrchardCore.Comments.Models;

public class CommentablePartSettings
{
    public bool RequireApproval { get; set; }
    public bool AllowComments { get; set; } = true;
    public CommentStatus DefaultStatus { get; set; } = CommentStatus.Approved;
}
