namespace OrchardCore.Comments.ViewModels;

public class CommentCreateViewModel
{
    public string CommentedOn { get; set; }
    public string RepliedOn { get; set; }
    public string Text { get; set; }
    public List<string> Attachments { get; set; } = [];
}
