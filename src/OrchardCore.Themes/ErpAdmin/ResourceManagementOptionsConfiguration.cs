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
            .SetDependencies("the-admin")
            .SetUrl("~/ErpAdmin/js/erpadmin/ErpAdmin.min.js", "~/ErpAdmin/js/erpadmin/ErpAdmin.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("ErpAdmin")
            .SetUrl("~/ErpAdmin/css/erpadmin/ErpAdmin.min.css", "~/ErpAdmin/css/erpadmin/ErpAdmin.css")
            .SetDependencies("the-admin")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
