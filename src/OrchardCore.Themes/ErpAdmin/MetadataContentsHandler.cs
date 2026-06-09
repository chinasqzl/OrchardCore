using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace ErpAdmin;

public class MetadataContentsHandler : ContentHandlerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MetadataContentsHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        if (context.Aspect is ContentItemMetadata metadata)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Path.StartsWithSegments("/Admin") == true)
            {
                metadata.DisplayRouteValues = new RouteValueDictionary
                {
                    { "Area", "OrchardCore.Contents" },
                    { "Controller", "Admin" },
                    { "Action", "Display" },
                    { "contentItemId", context.ContentItem.ContentItemId },
                    { "displayType", "DetailAdmin" },
                };
            }
        }

        return Task.CompletedTask;
    }
}