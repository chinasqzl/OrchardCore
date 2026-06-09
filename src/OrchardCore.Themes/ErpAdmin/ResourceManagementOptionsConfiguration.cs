using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.ErpAdmin;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("erp-admin")
            .SetDependencies("bootstrap", "admin-main", "theme-manager", "jquery")
            .SetUrl("~/ErpAdmin/js/erpadmin/ErpAdmin.min.js", "~/ErpAdmin/js/erpadmin/ErpAdmin.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("erp-admin")
            .SetUrl("~/ErpAdmin/css/ErpAdmin.min.css", "~/ErpAdmin/css/ErpAdmin.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
