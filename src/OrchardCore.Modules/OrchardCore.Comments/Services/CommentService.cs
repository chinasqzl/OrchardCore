using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Comments.Services;

public class CommentService : ICommentService
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;

    public CommentService(ISession session, IContentManager contentManager)
    {
        _session = session;
        _contentManager = contentManager;
    }

    public async Task<IEnumerable<ContentItem>> GetCommentsAsync(string commentedOn, CommentStatus? status = null)
    {
        IQuery<ContentItem> query = _session.Query<ContentItem>()
            .With<CommentPartIndex>(x =>
                x.CommentedOn == commentedOn &&
                x.Published &&
                !x.IsDeleted);

        if (status.HasValue)
        {
            var statusStr = status.Value.ToString();
            query = query.With<CommentPartIndex>(x => x.Status == statusStr);
        }
        else
        {
            query = query.With<CommentPartIndex>(x => x.Status == CommentStatus.Approved.ToString());
        }

        query = query.With<CommentPartIndex>(x => x.CreatedUtc != null)
            .OrderBy(x => x.CreatedUtc);

        return await query.ListAsync(_contentManager);
    }

    public async Task<int> GetApprovedCommentCountAsync(string commentedOn)
    {
        return await _session.Query<ContentItem, CommentPartIndex>(x =>
            x.CommentedOn == commentedOn &&
            x.Status == CommentStatus.Approved.ToString() &&
            x.Published &&
            !x.IsDeleted).CountAsync();
    }

    public async Task<IEnumerable<ContentItem>> GetCommentsForAdminListAsync(CommentStatus? status = null, int? page = null, int? pageSize = null)
    {
        IQuery<ContentItem> query = _session.Query<ContentItem>()
            .With<CommentPartIndex>(x => !x.IsDeleted && x.Latest);

        if (status.HasValue)
        {
            var statusStr = status.Value.ToString();
            query = query.With<CommentPartIndex>(x => x.Status == statusStr);
        }

        query = query.With<CommentPartIndex>(x => x.CreatedUtc != null)
            .OrderByDescending(x => x.CreatedUtc);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ListAsync(_contentManager);
    }

    public async Task SyncCommentCountAsync(string commentedOn)
    {
        var contentItem = await _contentManager.GetAsync(commentedOn, VersionOptions.Published);
        if (contentItem == null)
        {
            return;
        }

        if (!contentItem.TryGet<CommentablePart>(out var commentablePart))
        {
            return;
        }

        var count = await GetApprovedCommentCountAsync(commentedOn);
        commentablePart.CommentCount = count;
        contentItem.Apply(commentablePart);
        await _contentManager.UpdateAsync(contentItem);
    }
}
