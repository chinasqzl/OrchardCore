using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Comments.Drivers;
using OrchardCore.Comments.Handlers;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.Permissions;
using OrchardCore.Comments.Services;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Comments;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<CommentablePartViewModel>();
            o.MemberAccessStrategy.Register<CommentViewModel>();
            o.MemberAccessStrategy.Register<CommentablePartSettingsViewModel>();
        });

        services.AddContentPart<CommentablePart>()
            .UseDisplayDriver<CommentablePartDisplayDriver>();

        services.AddContentPart<CommentPart>()
            .AddHandler<CommentPartHandler>();

        services.AddScoped<IContentTypePartDefinitionDisplayDriver, CommentablePartSettingsDisplayDriver>();

        services.AddIndexProvider<CommentPartIndexProvider>();

        services.AddScoped<ICommentService, CommentService>();

        services.AddDataMigration<Migrations>();

        services.AddScoped<IPermissionProvider, CommentPermissionsProvider>();

        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // Routes are auto-registered via [Admin] and [Route] attributes on controllers.
    }
}
