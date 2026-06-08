using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Comments.Indexes;

public class CommentPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string CommentedOn { get; set; }
    public string RepliedOn { get; set; }
    public string Status { get; set; }
    public bool IsAdminReply { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}

public class CommentPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<CommentPartIndex>()
            .Map(contentItem =>
            {
                if (!contentItem.Latest && !contentItem.Published)
                {
                    return null;
                }

                if (!contentItem.TryGet<CommentPart>(out var commentPart))
                {
                    return null;
                }

                if (commentPart.IsDeleted)
                {
                    return null;
                }

                return new CommentPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    CommentedOn = commentPart.CommentedOn,
                    RepliedOn = commentPart.RepliedOn,
                    Status = commentPart.Status.ToString(),
                    IsAdminReply = commentPart.IsAdminReply,
                    IsDeleted = commentPart.IsDeleted,
                    CreatedUtc = commentPart.CreatedUtc,
                    Published = contentItem.Published,
                    Latest = contentItem.Latest,
                };
            });
    }
}
