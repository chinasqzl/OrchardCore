using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using ErpAdmin.Drivers;

namespace ErpAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentHandler, MetadataContentsHandler>();
        services.AddDisplayDriver<Navbar, TenantBrandingNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
