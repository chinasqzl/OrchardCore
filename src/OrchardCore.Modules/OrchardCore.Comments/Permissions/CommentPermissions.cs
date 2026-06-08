using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments.Permissions;

public static class CommentPermissions
{
    public static readonly Permission ManageComments = new("ManageComments", "Manage all comments");
    public static readonly Permission CreateComments = new("CreateComments", "Create comments", new[] { ManageComments });
    public static readonly Permission EditOwnComment = new("EditOwnComment", "Edit own comments", new[] { CreateComments });
    public static readonly Permission DeleteOwnComment = new("DeleteOwnComment", "Delete own comments", new[] { CreateComments });
    public static readonly Permission AdminReply = new("AdminReply", "Reply as admin", new[] { CreateComments });
}

public class CommentPermissionsProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<Permission>>(
        [
            CommentPermissions.ManageComments,
            CommentPermissions.CreateComments,
            CommentPermissions.EditOwnComment,
            CommentPermissions.DeleteOwnComment,
            CommentPermissions.AdminReply,
        ]);
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return
        [
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions =
                [
                    CommentPermissions.ManageComments,
                    CommentPermissions.CreateComments,
                    CommentPermissions.EditOwnComment,
                    CommentPermissions.DeleteOwnComment,
                    CommentPermissions.AdminReply,
                ]
            },
            new PermissionStereotype
            {
                Name = "Authenticated",
                Permissions =
                [
                    CommentPermissions.CreateComments,
                    CommentPermissions.EditOwnComment,
                    CommentPermissions.DeleteOwnComment,
                ]
            },
        ];
    }
}
