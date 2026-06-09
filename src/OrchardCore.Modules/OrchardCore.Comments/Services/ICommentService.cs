using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Services;

public interface ICommentService
{
    Task<IEnumerable<ContentItem>> GetCommentsAsync(string commentedOn, CommentStatus? status = null);
    Task<int> GetApprovedCommentCountAsync(string commentedOn);
    Task<IEnumerable<ContentItem>> GetCommentsForAdminListAsync(CommentStatus? status = null, int? page = null, int? pageSize = null);
    Task SyncCommentCountAsync(string commentedOn);
}
