using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "ErpAdmin",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "ERP Admin theme with sidebar layout, color-block design system, and tenant branding.",
    BaseTheme = "TheAdmin",
    Dependencies =
    [
        "OrchardCore.Themes",
    ],
    Tags =
    [
        ManifestConstants.AdminTag,
    ]
)]
