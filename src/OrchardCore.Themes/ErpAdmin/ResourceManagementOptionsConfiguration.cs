using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace ErpAdmin;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("ErpAdmin")
            .SetDependencies("jQuery")
            .SetUrl("~/TheAdmin/js/theadmin/ErpAdmin.min.js", "~/TheAdmin/js/theadmin/ErpAdmin.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("ErpAdmin")
            .SetUrl("~/TheAdmin/css/ErpAdmin.min.css", "~/TheAdmin/css/ErpAdmin.css")
            .SetDependencies("the-admin")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}