using OrchardCore;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace ErpAdmin.Drivers;

public sealed class TenantBrandingNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("TenantBranding", model)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1");
    }
}
