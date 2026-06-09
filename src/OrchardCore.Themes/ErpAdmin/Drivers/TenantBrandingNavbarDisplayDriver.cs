using OrchardCore;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;

namespace ErpAdmin.Drivers;

public sealed class TenantBrandingNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IShellHost _shellHost;

    public TenantBrandingNavbarDisplayDriver(IShellHost shellHost)
    {
        _shellHost = shellHost;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("TenantBranding", model)
            .RenderWhen(async () =>
            {
                // Only render when ErpAdmin theme is the active admin theme
                var shellSettings = _shellHost.GetSettings();
                return shellSettings?.Name == "ErpAdmin" ||
                       shellSettings?.AdminThemeId == "ErpAdmin";
            })
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1");
    }
}
