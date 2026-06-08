using Microsoft.Extensions.Localization;
using OrchardCore.Comments.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.Comments;

public class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder.Add(S["Content"], content =>
        {
            content.Add(S["Comments"], S["Comments"].Value, entry =>
            {
                entry.Action("List", "Admin", new { area = "OrchardCore.Comments" })
                    .Permission(CommentPermissions.ManageComments)
                    .LocalNav();
            });
        });

        return ValueTask.CompletedTask;
    }
}
