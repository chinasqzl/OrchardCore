using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Themes.ErpAdmin.Handlers;

public class MetadataContentsHandler : ContentHandlerBase
{
    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<ContentItemMetadata>(metadata =>
        {
            // Override the display route to point to Admin/Display for the DetailAdmin display type
            metadata.DisplayRouteValues = new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Display" },
                { "ContentItemId", context.ContentItem.ContentItemId }
            };

            metadata.AdminRouteValues = new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Display" },
                { "ContentItemId", context.ContentItem.ContentItemId }
            };

            return Task.CompletedTask;
        });
    }
}
