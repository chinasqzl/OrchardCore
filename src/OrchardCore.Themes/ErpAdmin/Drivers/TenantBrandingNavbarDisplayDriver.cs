using OrchardCore;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace ErpAdmin.Drivers;

public sealed class TenantBrandingNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IAdminThemeService _adminThemeService;

    public TenantBrandingNavbarDisplayDriver(IAdminThemeService adminThemeService)
    {
        _adminThemeService = adminThemeService;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("TenantBranding", model)
            .RenderWhen(async () =>
            {
                var adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();
                return adminThemeName == "ErpAdmin";
            })
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1");
    }
}
