using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Themes.ErpAdmin.Drivers;
using OrchardCore.Themes.ErpAdmin.Handlers;

namespace OrchardCore.Themes.ErpAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, TenantBrandingNavbarDisplayDriver>();
        services.AddScoped<IContentHandler, MetadataContentsHandler>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
